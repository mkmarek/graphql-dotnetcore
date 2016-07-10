namespace GraphQLCore.Type.Translation
{
    using Exceptions;
    using Language.AST;
    using System;

    public interface ITypeTranslator
    {
        IObjectTypeTranslator GetObjectTypeTranslatorFor(System.Type type);

        IObjectTypeTranslator GetObjectTypeTranslatorFor(GraphQLNullableType type);

        GraphQLScalarType GetType(Type type);

        GraphQLException[] IsValidLiteralValue(GraphQLScalarType inputType, GraphQLValue astValue);

        object GetLiteralValue(GraphQLValue value);

        GraphQLScalarType GetType(GraphQLNamedType type);

        System.Type GetType(GraphQLScalarType type);
        GraphQLScalarType GetInputType(Type type);
    }
}