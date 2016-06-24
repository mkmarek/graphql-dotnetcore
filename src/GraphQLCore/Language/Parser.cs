namespace GraphQLCore.Language
{
    using GraphQLCore.Language.AST;

    public class Parser
    {
        private ILexer Lexer;

        public Parser(ILexer lexer)
        {
            this.Lexer = lexer;
        }

        public GraphQLDocument Parse(ISource source)
        {
            using (var context = new ParserContext(source, this.Lexer))
            {
                return context.Parse();
            }
        }
    }
}