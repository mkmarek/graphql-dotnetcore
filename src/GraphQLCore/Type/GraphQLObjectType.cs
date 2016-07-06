namespace GraphQLCore.Type
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLObjectType : GraphQLComplexType
    {
        public GraphQLObjectType(string name, string description) : base(name, description)
        {
            this.fields = new Dictionary<string, GraphQLObjectTypeFieldInfo>();
        }

        public void Field<TFieldType>(string fieldName, LambdaExpression fieldLambda)
        {
            this.AddField(fieldName, fieldLambda);
        }

        internal virtual object ResolveField(
            GraphQLFieldSelection field, IList<GraphQLArgument> arguments, object parent)
        {
            var name = this.GetFieldName(field);

            if (this.fields.ContainsKey(name))
            {
                var fieldInfo = this.fields[this.GetFieldName(field)];
                return this.ProcessField(TypeUtilities.InvokeWithArguments(arguments, fieldInfo.Lambda));
            }

            return null;
        }

        protected virtual void AddField(string fieldName, LambdaExpression resolver)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.fields.Add(fieldName, new GraphQLObjectTypeFieldInfo()
            {
                Name = fieldName,
                IsResolver = true,
                Lambda = resolver,
                Arguments = this.GetArguments(resolver),
                ReturnValueType = ReflectionUtilities.GetReturnValueFromLambdaExpression(resolver)
            });
        }

        protected string GetFieldName(GraphQLFieldSelection selection)
        {
            return selection.Name?.Value ?? selection.Alias?.Value;
        }

        protected object ProcessField(object input)
        {
            if (input == null)
                return null;

            if (ReflectionUtilities.IsEnum(input.GetType()))
                return input.ToString();

            return input;
        }

        private Dictionary<string, GraphQLObjectTypeArgumentInfo> GetArguments(LambdaExpression resolver)
        {
            return resolver.Parameters.Select(e => new GraphQLObjectTypeArgumentInfo()
            {
                Name = e.Name,
                Type = e.Type
            }).ToDictionary(e => e.Name);
        }
    }
}