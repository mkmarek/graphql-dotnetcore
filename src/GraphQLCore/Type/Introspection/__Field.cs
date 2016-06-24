namespace GraphQLCore.Type.Introspection
{
    using Utils;

    public class __Field : GraphQLObjectType
    {

        public __Field(string fieldName, string fieldDescription, System.Type sourceType) : base("__Field", 
            "Object and Interface types are described by a list of Fields, each of " +
            "which has a name, potentially a list of arguments, and a return type."
            , null)
        {

            this.Field("name", () => fieldName);
            this.Field("description", () => fieldDescription);
            this.Field("isDeprecated", () => null as bool?);
            this.Field("deprecationReason", () => null as string);
            this.Field("type", () => TypeUtilities.ResolveObjectFieldType(sourceType));
        }
    }
}
