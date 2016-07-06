namespace GraphQLCore.Type.Translation
{
    using System;
    using System.Collections.Generic;

    public interface ISchemaObserver
    {
        void AddKnownType(GraphQLNullableType type);

        IEnumerable<GraphQLNullableType> GetKnownTypes();

        GraphQLNullableType GetSchemaTypeFor(System.Type type);

        Type GetTypeFor(GraphQLScalarType type);

        GraphQLComplexType[] GetTypesImplementing(GraphQLNullableType objectType);
    }
}