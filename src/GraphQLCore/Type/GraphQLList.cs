namespace GraphQLCore.Type
{
    using Introspection;
    using System;
    using Utils;

    public class GraphQLList : GraphQLObjectType
    {
        private __Type memberType;

        public GraphQLList(Type collectionType, GraphQLSchema schema) : base(null, null, null)
        {
            this.schema = schema;
            this.memberType = TypeUtilities.ResolveObjectArgumentType(
                ReflectionUtilities.GetCollectionMemberType(collectionType), schema);
        }

        internal __Type GetMemberType()
        {
            return this.memberType;
        }
    }
}