using GraphQLCore.Language.AST;

namespace GraphQLCore.Validation.Rules
{
    public class Conflict
    {
        public string ResponseName { get; set; }
        public string Reason { get; set; }
        public Conflict[] Subreasons { get; set; }
        public GraphQLName[] Field1 { get; internal set; }
        public GraphQLName[] Field2 { get; internal set; }
    }
}