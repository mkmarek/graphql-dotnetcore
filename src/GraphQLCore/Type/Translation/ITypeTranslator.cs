namespace GraphQLCore.Type.Translation
{
    using Exceptions;

    public interface ITypeTranslator
    {
        IObjectTypeTranslator GetObjectTypeTranslatorFor(System.Type type);

        IObjectTypeTranslator GetObjectTypeTranslatorFor(GraphQLNullableType type);

        GraphQLScalarType GetType(System.Type type);

        GraphQLException[] IsValidLiteralValue(GraphQLScalarType inputType, Language.AST.GraphQLValue astValue);
    }
}