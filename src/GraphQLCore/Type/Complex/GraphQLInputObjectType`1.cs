namespace GraphQLCore.Type
{
    using Exceptions;
    using Language.AST;
    using System;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLInputObjectType<T> : GraphQLInputObjectType
        where T : class, new()
    {
        public GraphQLInputObjectType(string name, string description) : base(name, description)
        {
        }

        public void Field<TProperty>(string fieldName, Expression<Func<T, TProperty>> accessor)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            var returnType = ReflectionUtilities.GetReturnValueFromLambdaExpression(accessor);

            if (this.IsInterfaceOrCollectionOfInterfaces(returnType))
                throw new GraphQLException("Can't set accessor to interface based field");

            this.Fields.Add(fieldName, this.CreateFieldInfo(fieldName, accessor));
        }

        public override object GetFromAst(GraphQLValue astValue)
        {
            return null;
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