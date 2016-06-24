using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GraphQLCore.Type
{
    public static class GraphQLObjectTypeExtensions
    {
        public static void SetResolver<T>(this GraphQLObjectType<T> @this, Expression<Func<T>> resolver) 
            where T : class => @this.SetResolver(resolver);
        public static void SetResolver<T, T1>(this GraphQLObjectType<T> @this, Expression<Func<T1, T>> resolver)
            where T : class => @this.SetResolver(resolver);
        public static void SetResolver<T, T1, T2>(this GraphQLObjectType<T> @this, Expression<Func<T1, T2, T>> resolver)
            where T : class => @this.SetResolver(resolver);
        public static void SetResolver<T, T1, T2, T3>(this GraphQLObjectType<T> @this, Expression<Func<T1, T2, T3, T>> resolver)
            where T : class => @this.SetResolver(resolver);
        public static void SetResolver<T, T1, T2, T3, T4>(this GraphQLObjectType<T> @this, Expression<Func<T1, T2, T3, T4, T>> resolver)
            where T : class => @this.SetResolver(resolver);
        public static void SetResolver<T, T1, T2, T3, T4, T5>(this GraphQLObjectType<T> @this, Expression<Func<T1, T2, T3, T4, T5, T>> resolver)
            where T : class => @this.SetResolver(resolver);
        public static void SetResolver<T, T1, T2, T3, T4, T5, T6>(this GraphQLObjectType<T> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T>> resolver)
            where T : class => @this.SetResolver(resolver);
        public static void SetResolver<T, T1, T2, T3, T4, T5, T6, T7>(this GraphQLObjectType<T> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T>> resolver)
            where T : class => @this.SetResolver(resolver);
        public static void SetResolver<T, T1, T2, T3, T4, T5, T6, T7, T8>(this GraphQLObjectType<T> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T>> resolver)
            where T : class => @this.SetResolver(resolver);
        public static void SetResolver<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this GraphQLObjectType<T> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T>> resolver)
            where T : class => @this.SetResolver(resolver);
        public static void SetResolver<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this GraphQLObjectType<T> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T>> resolver)
            where T : class => @this.SetResolver(resolver);

        public static void Field<T1>(this GraphQLObjectType @this, string name, Expression<Func<T1>> resolver) => @this.Field<T1>(name, resolver);
        public static void Field<T1, T2>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2>> resolver) => @this.Field<T2>(name, resolver);
        public static void Field<T1, T2, T3>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3>> resolver) => @this.Field<T3>(name, resolver);
        public static void Field<T1, T2, T3, T4>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4>> resolver) => @this.Field<T4>(name, resolver);
        public static void Field<T1, T2, T3, T4, T5>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5>> resolver) => @this.Field<T5>(name, resolver);
        public static void Field<T1, T2, T3, T4, T5, T6>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6>> resolver) => @this.Field<T6>(name, resolver);
        public static void Field<T1, T2, T3, T4, T5, T6, T7>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7>> resolver) => @this.Field<T7>(name, resolver);
        public static void Field<T1, T2, T3, T4, T5, T6, T7, T8>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8>> resolver) => @this.Field<T8>(name, resolver);
        public static void Field<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9>> resolver) => @this.Field<T9>(name, resolver);
        public static void Field<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> resolver) => @this.Field<T10>(name, resolver);
    }
}
