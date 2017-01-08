using System.Reflection;
using GraphQLCore.Execution;

namespace GraphQLCore.Type
{
    using Complex;
    using Exceptions;
    using Language.AST;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Translation;
    using Utils;

    public abstract class GraphQLInputObjectType<T> : GraphQLInputObjectType
        where T : class, new()
    {
        public override Type SystemType { get; protected set; }

        public GraphQLInputObjectType(string name, string description) : base(name, description)
        {
            this.SystemType = typeof(T);
        }

        public void Field<TProperty>(string fieldName, Expression<Func<T, TProperty>> accessor)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            var returnType = ReflectionUtilities.GetReturnValueFromLambdaExpression(accessor);

            if (this.IsInterfaceOrCollectionOfInterfaces(returnType))
                throw new GraphQLException("Can't set accessor to interface based field");

            this.Fields.Add(fieldName, GraphQLInputObjectTypeFieldInfo.CreateAccessorFieldInfo(fieldName, accessor));
        }

        public override object GetFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (!(astValue is GraphQLObjectValue))
                return null;

            var objectAstValue = (GraphQLObjectValue)astValue;
            var result = new T();

            foreach (var field in this.Fields)
            {
                var astField = GetFieldFromAstObjectValue(objectAstValue, field.Key);

                if (astField == null)
                    continue;
                
                object value = this.GetValueFromField(schemaRepository, field.Value, astField);

                if (value == null && astField.Value.Kind == ASTNodeKind.Variable)
                {
                    value = schemaRepository.VariableResolver.GetValue((GraphQLVariable)astField.Value);
                    value = ReflectionUtilities.ChangeValueType(value, field.Value.SystemType);
                }

                this.AssignValueToObjectField(result, field.Value, value);
            }

            return result;
        }

        private static GraphQLObjectField GetFieldFromAstObjectValue(GraphQLObjectValue objectAstValue, string fieldName)
        {
            return objectAstValue.Fields.FirstOrDefault(e => e.Name.Value == fieldName);
        }

        private void AssignValueToObjectField(T result, GraphQLInputObjectTypeFieldInfo field, object value)
        {
            if (ReflectionUtilities.IsCollection(field.SystemType))
                value = ReflectionUtilities.ChangeToCollection(value, field.SystemType);

            ReflectionUtilities.MakeSetterFromLambda(field.Lambda)
                    .DynamicInvoke(result, value);
        }

        private object GetValueFromField(
            ISchemaRepository schemaRepository,
            GraphQLFieldInfo field,
            GraphQLObjectField astField)
        {
            var graphQLType = schemaRepository.GetSchemaInputTypeFor(field.SystemType);
            var value = graphQLType.GetFromAst(astField.Value, schemaRepository);

            return value;
        }

        private bool IsInterfaceOrCollectionOfInterfaces(Type type)
        {
            if (ReflectionUtilities.IsCollection(type))
                return this.IsInterfaceOrCollectionOfInterfaces(ReflectionUtilities.GetCollectionMemberType(type));

            if (ReflectionUtilities.IsInterface(type))
                return true;

            return false;
        }
    }
}