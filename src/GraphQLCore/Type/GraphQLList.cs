namespace GraphQLCore.Type
{
    using Introspection;
    using System;
    using Utils;

    public class GraphQLList : GraphQLObjectType
    {
        private Type memberType;

        public GraphQLList(Type collectionType) : base("", "", null)
        {
            this.memberType = ReflectionUtilities.GetCollectionMemberType(collectionType);

            this.Name = "ListOf" + memberType.Name;
        }

        internal __Type GetMemberType()
        {
            return TypeUtilities.ResolveObjectFieldType(this.memberType);
        }
    }
}