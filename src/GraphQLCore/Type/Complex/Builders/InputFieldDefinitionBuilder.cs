namespace GraphQLCore.Type.Complex
{
    public class InputFieldDefinitionBuilder : FieldDefinitionBuilder<InputFieldDefinitionBuilder, GraphQLInputObjectTypeFieldInfo>
    {
        public InputFieldDefinitionBuilder(GraphQLInputObjectTypeFieldInfo fieldInfo)
            : base(fieldInfo)
        {
        }
    }
}
