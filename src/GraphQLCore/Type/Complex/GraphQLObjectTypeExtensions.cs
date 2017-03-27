using System;
using System.Linq.Expressions;
using GraphQLCore.Type.Complex;

namespace GraphQLCore.Type
{
    public static class GraphQLObjectTypeExtensions
    {
        public static FieldDefinitionBuilder Field<T1>(this GraphQLObjectType @this, string name, Expression<Func<T1>> resolver) => @this.Field(name, resolver);

        public static FieldDefinitionBuilder Field<T1, T2>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2>> resolver) => @this.Field(name, resolver);

        public static FieldDefinitionBuilder Field<T1, T2, T3>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3>> resolver) => @this.Field(name, resolver);

        public static FieldDefinitionBuilder Field<T1, T2, T3, T4>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4>> resolver) => @this.Field(name, resolver);

        public static FieldDefinitionBuilder Field<T1, T2, T3, T4, T5>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5>> resolver) => @this.Field(name, resolver);

        public static FieldDefinitionBuilder Field<T1, T2, T3, T4, T5, T6>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6>> resolver) => @this.Field(name, resolver);

        public static FieldDefinitionBuilder Field<T1, T2, T3, T4, T5, T6, T7>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7>> resolver) => @this.Field(name, resolver);

        public static FieldDefinitionBuilder Field<T1, T2, T3, T4, T5, T6, T7, T8>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8>> resolver) => @this.Field(name, resolver);

        public static FieldDefinitionBuilder Field<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9>> resolver) => @this.Field(name, resolver);

        public static FieldDefinitionBuilder Field<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this GraphQLObjectType @this, string name, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> resolver) => @this.Field(name, resolver);
    }
}