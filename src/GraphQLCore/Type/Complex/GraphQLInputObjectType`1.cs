namespace GraphQLCore.Type
{
    using Complex;
    using Exceptions;
    using Language.AST;
    using System;
    using System.Collections.Generic;
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

        public InputFieldDefinitionBuilder<TProperty> Field<TProperty>(string fieldName, Expression<Func<T, TProperty>> accessor, string description = null)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            var returnType = ReflectionUtilities.GetReturnValueFromLambdaExpression(accessor);

            if (this.IsInterfaceOrCollectionOfInterfaces(returnType))
                throw new GraphQLException("Can't set accessor to interface based field");

            var fieldInfo = GraphQLInputObjectTypeFieldInfo.CreateAccessorFieldInfo(fieldName, accessor, description);
            this.Fields.Add(fieldName, fieldInfo);

            return new InputFieldDefinitionBuilder<TProperty>(fieldInfo);
        }

        public override Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (!(astValue is GraphQLObjectValue))
                return Result.Invalid;

            var objectAstValue = (GraphQLObjectValue)astValue;
            var result = new T();

            foreach (var field in this.Fields)
            {
                var astField = GetFieldFromAstObjectValue(objectAstValue, field.Key);
                Result fieldResult;

                if (astField == null && field.Value.DefaultValue.IsSet)
                    fieldResult = new Result(field.Value.DefaultValue.Value);
                else
                    fieldResult = this.GetField(astField, field.Value, schemaRepository);

                if (!fieldResult.IsValid)
                    return Result.Invalid;

                if (fieldResult.Value == null && field.Value.DefaultValue.IsSet)
                    fieldResult = new Result(field.Value.DefaultValue.Value);

                this.AssignValueToObjectField(result, field.Value, fieldResult.Value);
            }

            return new Result(result);
        }

        protected override GraphQLValue GetAst(object value, ISchemaRepository schemaRepository)
        {
            if (!(value is T))
                return null;

            var fieldNodes = new List<GraphQLObjectField>();

            foreach (var field in this.Fields)
            {
                var fieldName = field.Key;
                var fieldInfo = field.Value;
                var fieldType = fieldInfo.GetGraphQLType(schemaRepository) as GraphQLInputType;

                var objectValue = fieldInfo.Lambda.Compile().DynamicInvoke(value);
                var fieldValue = fieldType.GetAstFromValue(objectValue, schemaRepository);

                fieldNodes.Add(new GraphQLObjectField()
                {
                    Name = new GraphQLName() { Value = fieldName },
                    Value = fieldValue
                });
            }

            return new GraphQLObjectValue()
            {
                Fields = fieldNodes
            };
        }

        private static GraphQLObjectField GetFieldFromAstObjectValue(GraphQLObjectValue objectAstValue, string fieldName)
        {
            return objectAstValue.Fields.FirstOrDefault(e => e.Name.Value == fieldName);
        }

        private Result GetField(GraphQLObjectField astField, GraphQLInputObjectTypeFieldInfo fieldInfo, ISchemaRepository schemaRepository)
        {
            return this.GetValueFromField(schemaRepository, fieldInfo, astField);
        }

        private void AssignValueToObjectField(T result, GraphQLInputObjectTypeFieldInfo field, object value)
        {
            value = ReflectionUtilities.ChangeValueType(value, field.SystemType);

            ReflectionUtilities.MakeSetterFromLambda(field.Lambda)
                    .DynamicInvoke(result, value);
        }

        private Result GetValueFromField(
            ISchemaRepository schemaRepository,
            GraphQLFieldInfo field,
            GraphQLObjectField astField)
        {
            var graphQLType = schemaRepository.GetSchemaInputTypeFor(field.SystemType);
            var result = graphQLType.GetFromAst(astField?.Value, schemaRepository);

            return result;
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