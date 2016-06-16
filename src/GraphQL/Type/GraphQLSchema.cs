namespace GraphQL.Type
{
    using Execution;
    using Language;
    using Language.AST;
    using System;

    public class GraphQLSchema
    {
        public  GraphQLObjectType RootType { get; private set; }

        public GraphQLSchema(GraphQLObjectType rootType)
        {
            this.RootType = rootType;
        }

        public dynamic Execute(string expression)
        {
            using (var context = new ExecutionContext(this, this.GetAst(expression)))
            {
                return context.Execute();
            }
        }

        private GraphQLDocument GetAst(string expression)
        {
            return new Parser(new Lexer()).Parse(new Source(expression));
        }
    }
}
