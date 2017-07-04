namespace GraphQLCore.Type.Complex
{
    using System;
    using Translation;

    public class GraphQLObjectTypeArgumentInfo : GraphQLFieldInfo
    {
        public override Type SystemType { get; set; }
        public DefaultValue DefaultValue { get; set; }

        protected override GraphQLBaseType GetSchemaType(Type type, ISchemaRepository schemaRepository)
        {
            return schemaRepository.GetSchemaInputTypeFor(type);
        }
    }
}