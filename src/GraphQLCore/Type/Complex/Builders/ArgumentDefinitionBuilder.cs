namespace GraphQLCore.Type.Complex
{
    public class ArgumentDefinitionBuilder : FieldDefinitionBuilder<ArgumentDefinitionBuilder, GraphQLObjectTypeArgumentInfo>
    {
        public ArgumentDefinitionBuilder(GraphQLObjectTypeArgumentInfo argumentInfo) : base(argumentInfo)
        {
        }

        public ArgumentDefinitionBuilder WithDefaultValue(object defaultValue)
        {
            this.FieldInfo.DefaultValue = new DefaultValue(defaultValue, this.FieldInfo.SystemType);

            return this;
        }
    }
}
