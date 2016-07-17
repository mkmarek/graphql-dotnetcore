namespace GraphQLCore.Type.Translation
{
    using Language.AST;
    using System;

    public interface ITypeTranslator
    {
        GraphQLBaseType GetType(Type type);

        GraphQLBaseType GetType(GraphQLNamedType type);

        Type GetType(GraphQLBaseType type);

        object TranslatePerDefinition(object inputObject, GraphQLBaseType typeDefinition);

        object TranslatePerDefinition(object inputObject, Type type);
    }
}