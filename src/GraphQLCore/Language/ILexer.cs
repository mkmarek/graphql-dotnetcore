namespace GraphQLCore.Language
{
    public interface ILexer
    {
        Token Lex(ISource source);

        Token Lex(ISource source, int start);
    }
}