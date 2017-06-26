namespace GraphQLCore.Type.Complex
{
    using System;
    using System.Linq.Expressions;

    public class SubscriptionFieldDefinitionBuilder<TEntityType>
        : FieldDefinitionBuilder<SubscriptionFieldDefinitionBuilder<TEntityType>, GraphQLSubscriptionTypeFieldInfo>
    {
        public SubscriptionFieldDefinitionBuilder(GraphQLSubscriptionTypeFieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        public SubscriptionFieldDefinitionBuilder<TEntityType> WithSubscriptionFilter(Expression<Func<TEntityType, bool>> filter)
        {
            this.FieldInfo.Filter = filter;

            return this;
        }

        public SubscriptionFieldDefinitionBuilder<TEntityType> WithSubscriptionFilter(LambdaExpression filter)
        {
            this.FieldInfo.Filter = filter;

            return this;
        }

        public SubscriptionFieldDefinitionBuilder<TEntityType> OnChannel(string channelName)
        {
            this.FieldInfo.Channel = channelName;

            return this;
        }
    }
}
