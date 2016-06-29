namespace GraphQLCore.Type.Introspection
{
    public enum TypeKind
    {
        OBJECT,
        SCALAR,
        ENUM,
        INPUT_OBJECT,
        INTERFACE,
        LIST,
        NON_NULL,
        UNION
    }

    public class __TypeKind : GraphQLEnumType
    {
        public __TypeKind(GraphQLSchema schema) : base("__TypeKind", "An enum describing what kind of type a given `__Type` is.", typeof(TypeKind), null)
        {
            this.schema = schema;
        }
    }
}