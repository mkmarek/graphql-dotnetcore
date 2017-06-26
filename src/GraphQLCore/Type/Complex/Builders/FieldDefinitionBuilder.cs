namespace GraphQLCore.Type.Complex
{
    public class FieldDefinitionBuilder : FieldDefinitionBuilder<FieldDefinitionBuilder, GraphQLObjectTypeFieldInfo>
    {
        public FieldDefinitionBuilder(GraphQLObjectTypeFieldInfo fieldInfo)
            : base(fieldInfo)
        {
        }

        public FieldDefinitionBuilder OnChannel(string channelName)
        {
            this.FieldInfo.Channel = channelName;

            return this;
        }
    }
}
