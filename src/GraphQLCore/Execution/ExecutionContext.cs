namespace GraphQLCore.Execution
{
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type;
    using GraphQLCore.Type.Translation;

    public class ExecutionContext
    {
        public ISchemaRepository SchemaRepository { get; set; }
        public IVariableResolver VariableResolver {get; set; }
        public IFieldCollector FieldCollector { get; set; }
        public IGraphQLSchema Schema { get; set; }
        public OperationType OperationType { get; set; }
    }
}
