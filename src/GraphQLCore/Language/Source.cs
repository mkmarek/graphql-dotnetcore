namespace GraphQLCore.Language
{
    public class Source : ISource
    {
        public Source(string body) : this(body, "GraphQL")
        {
        }

        public Source(string body, string name)
        {
            this.Name = name;
            this.Body = body;
        }

        public string Body { get; set; }
        public string Name { get; set; }
    }
}