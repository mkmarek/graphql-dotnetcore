using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace GraphQLCore.Type.Complex
{
    public class SubscriptionFieldDefinitionBuilder<TEntityType> : FieldDefinitionBuilder
    {
        private GraphQLSubscriptionTypeFieldInfo fieldInfo;

        public SubscriptionFieldDefinitionBuilder(GraphQLSubscriptionTypeFieldInfo fieldInfo) : base(fieldInfo)
        {
            this.fieldInfo = fieldInfo;
        }

        public SubscriptionFieldDefinitionBuilder<TEntityType> WithSubscriptionFilter(Expression<Func<TEntityType, bool>> filter)
        {
            this.fieldInfo.Filter = filter;

            return this;
        }

        public SubscriptionFieldDefinitionBuilder<TEntityType> WithSubscriptionFilter(LambdaExpression filter)
        {
            this.fieldInfo.Filter = filter;

            return this;
        }

        public new SubscriptionFieldDefinitionBuilder<TEntityType> OnChannel(string channelName)
        {
            this.fieldInfo.Channel = channelName;

            return this;
        }
    }
}
