namespace GraphQLCore.Validation.Rules
{
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type;
    using GraphQLCore.Type.Complex;

    public class NodeAndDefinitions
    {
        public string PresumedParentName { get; set; }
        public GraphQLObjectTypeFieldInfo FieldDefinition { get; set; }
        public GraphQLBaseType ParentType { get; set; }
        public GraphQLFieldSelection Selection { get; set; }
    }
}