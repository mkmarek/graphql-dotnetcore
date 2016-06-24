namespace GraphQLCore.Utils
{
    using Type.Introspection;
    using Type.Scalars;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public static class TypeUtilities
    {
        public static __Type ResolveObjectFieldType(System.Type type)
        {
            if (typeof(int) == type)
                return new __Type(new GraphQLInt(null), null);

            if (typeof(bool) == type)
                return new __Type(new GraphQLBoolean(null), null);

            if (typeof(float) == type || typeof(double) == type)
                return new __Type(new GraphQLFloat(null), null);

            if (typeof(string) == type)
                return new __Type(new GraphQLString(null), null);

            return null;
        }

        public static IEnumerable<__Type> IntrospectObjectFieldTypes(GraphQLObjectType value)
        {
            var types = value.GetFieldTypes();

            return types.Select(e => ResolveObjectFieldType(e))
                .Where(e => e != null);
        }

        public static string[] GetTypeNames(List<__Type> typeList)
        {
            return typeList.Select(e => GetTypeName(e)).ToArray();
        }

        public static string GetTypeName(__Type e)
        {
            return (string)e.ResolveField("name");
        }
    }
}
