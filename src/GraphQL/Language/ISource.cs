namespace GraphQL.Parser.Language
{
    public interface ISource
    {
        string Body { get; set; }
        string Name { get; set; }
    }
}