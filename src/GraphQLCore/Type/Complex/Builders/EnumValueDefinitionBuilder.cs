namespace GraphQLCore.Type.Complex
{
    public class EnumValueDefinitionBuilder : FieldDefinitionBuilder<EnumValueDefinitionBuilder, GraphQLEnumValueInfo>
    {
        public EnumValueDefinitionBuilder(GraphQLEnumValueInfo valueInfo) : base(valueInfo)
        {
        }
    }
}
