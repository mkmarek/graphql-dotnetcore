namespace GraphQLCore.Type.Complex
{
    using Exceptions;

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

        public FieldDefinitionBuilder WithDefaultValue(string parameterName, object defaultValue)
        {
            if (!this.FieldInfo.Arguments.ContainsKey(parameterName))
                throw new GraphQLException($"Argument {parameterName} does not exist.");

            var argument = this.FieldInfo.Arguments[parameterName];
            argument.DefaultValue = new DefaultValue(defaultValue, argument.SystemType);

            return this;
        }
    }
}
