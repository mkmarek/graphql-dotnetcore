namespace GraphQLCore.Type.Complex
{
    public class InputFieldDefinitionBuilder<TEntityType>
        : FieldDefinitionBuilder<InputFieldDefinitionBuilder<TEntityType>, GraphQLInputObjectTypeFieldInfo>
    {
        public InputFieldDefinitionBuilder(GraphQLInputObjectTypeFieldInfo fieldInfo)
            : base(fieldInfo)
        {
        }

        public InputFieldDefinitionBuilder<TEntityType> WithDefaultValue(TEntityType value)
        {
            this.FieldInfo.DefaultValue = new DefaultValue(value, this.FieldInfo.SystemType);

            return this;
        }
    }
}
