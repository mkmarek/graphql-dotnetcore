namespace GraphQLCore.Execution
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type.Complex;
    using Type.Translation;

    public class ExecutedField
    {
        public GraphQLObjectTypeFieldInfo FieldInfo { get; set; }
        public GraphQLFieldSelection Selection { get; set; }
        public IList<GraphQLArgument> Arguments { get; set; }
        public IEnumerable<object> Path { get; set; }
        public IList<GraphQLException> Errors { get; set; }

        public IFieldExpression GetExpression(ISchemaRepository schemaRepository, object parent)
        {
            if (this.FieldInfo.IsResolver)
                return ResolverExpression.Create(this.FieldInfo.Lambda, schemaRepository, parent, this.Arguments);
            return AccessorExpression.Create(this.FieldInfo.Lambda, parent);
        }
    }
}
