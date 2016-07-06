namespace GraphQLCore.Type
{
    using System;
    using System.Linq;
    using Utils;

    public class GraphQLEnumValue : GraphQLObjectType
    {
        public GraphQLEnumValue(string name, string description) :
            base("__EnumValue", "")
        {
            this.Field("name", () => name);
            this.Field("description", () => description);
            this.Field("isDeprecated", () => null as bool?);
            this.Field("deprecationReason", () => null as string);
        }

        public static GraphQLEnumValue[] GetEnumValuesFor(Type type)
        {
            if (!ReflectionUtilities.IsEnum(type))
            {
                throw new ArgumentException("T must be an enum type");
            }

            return Enum.GetNames(type).Select(e => new GraphQLEnumValue(e, "")).ToArray();
        }
    }
}