namespace GraphQLCore.Type.Complex
{
    using Translation;

    public class GraphQLObjectTypeArgumentInfo : GraphQLFieldInfo
    {
        public override System.Type SystemType { get; set; }

        protected override GraphQLBaseType GetSchemaType(System.Type type, ISchemaRepository schemaRepository)
        {
            return schemaRepository.GetSchemaInputTypeFor(type);
        }
    }
}