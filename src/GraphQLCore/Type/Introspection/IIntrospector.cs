namespace GraphQLCore.Type.Introspection
{
    public interface IIntrospector
    {
        IntrospectedType Introspect(GraphQLScalarType type);

        IntrospectedField IntrospectField(GraphQLFieldConfig field);

        IntrospectedInputValue IntrospectInputValue(GraphQLFieldConfig field);
    }
}