namespace GraphQLCore.Utils
{
    using Type;

    public static class SchemaUtils
    {
        public static string PrintSchema(IGraphQLSchema schema)
        {
            var printer = new SchemaPrinter(schema);
            return printer.PrintSchema();
        }

        public static string PrintIntrospectionSchema(IGraphQLSchema schema)
        {
            var printer = new SchemaPrinter(schema);
            return printer.PrintIntrospectionSchema();
        }
    }
}
