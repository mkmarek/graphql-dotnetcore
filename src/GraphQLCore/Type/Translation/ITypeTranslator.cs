namespace GraphQLCore.Type.Translation
{
    using Exceptions;
    using Language.AST;
    using System;
    using System.Dynamic;

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

        object TranslatePerDefinition(object inputObject, GraphQLScalarType typeDefinition);

        object TranslatePerDefinition(object inputObject, Type type);
    }
}