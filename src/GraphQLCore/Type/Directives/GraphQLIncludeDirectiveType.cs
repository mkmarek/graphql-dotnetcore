namespace GraphQLCore.Type.Directives
{
    using GraphQLCore.Type.Scalar;
    using GraphQLCore.Type.Translation;
    using Language.AST;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class GraphQLIncludeDirectiveType : GraphQLDirectiveType
    {
        public GraphQLIncludeDirectiveType()
            : base(
                "include",
                "Directs the executor to include this field or fragment only when the `if` argument is true.",
                DirectiveLocation.FIELD,
                DirectiveLocation.FRAGMENT_SPREAD,
                DirectiveLocation.INLINE_FRAGMENT)
        {
        }

        public override bool PreExecutionIncludeFieldIntoResult(
            GraphQLDirective directive,
            ISchemaRepository schemaRepository)
        {
             var argument = directive.Arguments.Single(e => e.Name.Value == "if");
            var booleanType = new GraphQLBoolean();

            var value = booleanType.GetFromAst(argument.Value, schemaRepository);

            return (bool)value;
        }

        public override LambdaExpression GetResolver(object value, object parentValue)
        {
            Expression<Func<bool, object>> resolver = (@if) => @value;

            return resolver;
        }
    }
}