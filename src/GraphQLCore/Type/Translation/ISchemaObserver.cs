namespace GraphQLCore.Type.Translation
{
    using System;
    using System.Collections.Generic;

    public interface ISchemaObserver
    {
        void AddKnownType(GraphQLNullableType type);

        IEnumerable<GraphQLNullableType> GetInputKnownTypes();

        IEnumerable<GraphQLNullableType> GetOutputKnownTypes();

        GraphQLNullableType GetSchemaInputTypeFor(Type type);

        GraphQLNullableType GetSchemaTypeFor(Type type);

        Type GetTypeFor(GraphQLScalarType type);

        GraphQLComplexType[] GetTypesImplementing(GraphQLNullableType objectType);
    }
}