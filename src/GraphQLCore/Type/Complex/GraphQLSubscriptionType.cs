using GraphQLCore.Events;
using GraphQLCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace GraphQLCore.Type.Complex
{
    public class GraphQLSubscriptionType : GraphQLObjectType
    {
        public IEventBus EventBus { get; private set; }

        public override System.Type SystemType { get; protected set; }

        public GraphQLSubscriptionType(
            string name,
            string description,
            IEventBus eventBus) : base(name, description)
        {
            this.EventBus = eventBus;
            this.SystemType = this.GetType();
        }

        public SubscriptionFieldDefinitionBuilder<TFieldType> Field<TFieldType>(string fieldName, LambdaExpression fieldLambda)
        {
            return this.AddField<TFieldType>(fieldName, fieldLambda);
        }

        protected virtual SubscriptionFieldDefinitionBuilder<TFieldType> AddField<TFieldType>(
            string fieldName, LambdaExpression resolver)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            var fieldInfo = this.CreateResolverFieldInfo(fieldName, resolver);

            this.Fields.Add(fieldName, fieldInfo);

            return new SubscriptionFieldDefinitionBuilder<TFieldType>(fieldInfo);
        }

        private GraphQLSubscriptionTypeFieldInfo CreateResolverFieldInfo(string fieldName, LambdaExpression resolver)
        {
            return GraphQLSubscriptionTypeFieldInfo.CreateResolverFieldInfo(fieldName, resolver);
        }
    }
}
