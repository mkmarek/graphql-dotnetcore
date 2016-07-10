namespace GraphQLCore.Type.Translation
{
    using System;
    using System.Collections.Generic;

    public interface ISchemaObserver
    {
        void AddKnownType(GraphQLNullableType type);

        IEnumerable<GraphQLNullableType> GetOutputKnownTypes();

        GraphQLNullableType GetSchemaTypeFor(Type type);

        Type GetTypeFor(GraphQLScalarType type);

        GraphQLComplexType[] GetTypesImplementing(GraphQLNullableType objectType);
        GraphQLNullableType GetSchemaInputTypeFor(Type type);
        IEnumerable<GraphQLNullableType> GetInputKnownTypes();
    }
}