using System;
using System.Linq.Expressions;

namespace GraphQLCore.Type.Complex
{
    public static class GraphQLSubscriptionTypeExtensions
    {
        public static SubscriptionFieldDefinitionBuilder<T1> Field<T1>(this GraphQLSubscriptionType @this, string name, Expression<Func<T1>> resolver) => @this.Field<T1>(name, resolver);

        public static SubscriptionFieldDefinitionBuilder<T2> Field<T1, T2>(this GraphQLSubscriptionType @this, string name, Expression<Func<T1, T2>> resolver) => @this.Field<T2>(name, resolver);

        public static SubscriptionFieldDefinitionBuilder<T3> Field<T1, T2, T3>(this GraphQLSubscriptionType @this, string name, Expression<Func<T1, T2, T3>> resolver) => @this.Field<T3>(name, resolver);

        public static SubscriptionFieldDefinitionBuilder<T4> Field<T1, T2, T3, T4>(this GraphQLSubscriptionType @this, string name, Expression<Func<T1, T2, T3, T4>> resolver) => @this.Field<T4>(name, resolver);

        public static SubscriptionFieldDefinitionBuilder<T5> Field<T1, T2, T3, T4, T5>(this GraphQLSubscriptionType @this, string name, Expression<Func<T1, T2, T3, T4, T5>> resolver) => @this.Field<T5>(name, resolver);

        public static SubscriptionFieldDefinitionBuilder<T6> Field<T1, T2, T3, T4, T5, T6>(this GraphQLSubscriptionType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6>> resolver) => @this.Field<T6>(name, resolver);

        public static SubscriptionFieldDefinitionBuilder<T7> Field<T1, T2, T3, T4, T5, T6, T7>(this GraphQLSubscriptionType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7>> resolver) => @this.Field<T7>(name, resolver);

        public static SubscriptionFieldDefinitionBuilder<T8> Field<T1, T2, T3, T4, T5, T6, T7, T8>(this GraphQLSubscriptionType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8>> resolver) => @this.Field<T8>(name, resolver);

        public static SubscriptionFieldDefinitionBuilder<T9> Field<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this GraphQLSubscriptionType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9>> resolver) => @this.Field<T9>(name, resolver);

        public static SubscriptionFieldDefinitionBuilder<T10> Field<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this GraphQLSubscriptionType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> resolver) => @this.Field<T10>(name, resolver);
    }
}