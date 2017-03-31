using System;
using System.Linq.Expressions;

namespace GraphQLCore.Type.Complex
{
    public static class SubscriptionFieldDefinitionBuilderExtensions
    {
        public static SubscriptionFieldDefinitionBuilder<TEntity> WithSubscriptionFilter<TEntity, T1>(this SubscriptionFieldDefinitionBuilder<TEntity> @this, Expression<Func<T1, bool>> filter) => @this.WithSubscriptionFilter(filter);
        
        public static SubscriptionFieldDefinitionBuilder<TEntity> WithSubscriptionFilter<TEntity, T1, T2>(this SubscriptionFieldDefinitionBuilder<TEntity> @this, Expression<Func<T1, T2, bool>> filter) => @this.WithSubscriptionFilter(filter);
        /*
        public static SubscriptionFieldDefinitionBuilder<T3> WithFilter<T1, T2, T3>(this SubscriptionFieldDefinitionBuilder @this, string name, Expression<Func<T1, T2, T3>> filter) => @this.WithFilter<T3>(name, filter);

        public static SubscriptionFieldDefinitionBuilder<T4> WithFilter<T1, T2, T3, T4>(this SubscriptionFieldDefinitionBuilder @this, string name, Expression<Func<T1, T2, T3, T4>> filter) => @this.WithFilter<T4>(name, filter);

        public static SubscriptionFieldDefinitionBuilder<T5> WithFilter<T1, T2, T3, T4, T5>(this SubscriptionFieldDefinitionBuilder @this, string name, Expression<Func<T1, T2, T3, T4, T5>> filter) => @this.WithFilter<T5>(name, filter);

        public static SubscriptionFieldDefinitionBuilder<T6> WithFilter<T1, T2, T3, T4, T5, T6>(this SubscriptionFieldDefinitionBuilder @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6>> filter) => @this.WithFilter<T6>(name, filter);

        public static SubscriptionFieldDefinitionBuilder<T7> WithFilter<T1, T2, T3, T4, T5, T6, T7>(this SubscriptionFieldDefinitionBuilder @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7>> filter) => @this.WithFilter<T7>(name, filter);

        public static SubscriptionFieldDefinitionBuilder<T8> WithFilter<T1, T2, T3, T4, T5, T6, T7, T8>(this SubscriptionFieldDefinitionBuilder @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8>> filter) => @this.WithFilter<T8>(name, filter);

        public static SubscriptionFieldDefinitionBuilder<T9> WithFilter<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this SubscriptionFieldDefinitionBuilder @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9>> filter) => @this.WithFilter<T9>(name, filter);

        public static SubscriptionFieldDefinitionBuilder<T10> WithFilter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this SubscriptionFieldDefinitionBuilder @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> filter) => @this.WithFilter<T10>(name, filter);*/
    }
}