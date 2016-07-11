namespace GraphQLCore.Type.Translation
{
    using Exceptions;
    using Language.AST;
    using System;

    public interface ITypeTranslator
    {
        GraphQLScalarType GetInputType(Type type);

        object GetLiteralValue(GraphQLValue value);

        IObjectTypeTranslator GetObjectTypeTranslatorFor(System.Type type);

        IObjectTypeTranslator GetObjectTypeTranslatorFor(GraphQLNullableType type);

        GraphQLScalarType GetType(Type type);

        GraphQLScalarType GetType(GraphQLNamedType type);

        System.Type GetType(GraphQLScalarType type);

        GraphQLException[] IsValidLiteralValue(GraphQLScalarType inputType, GraphQLValue astValue);
    }
}