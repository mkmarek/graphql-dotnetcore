namespace GraphQLCore.Type.Translation
{
    using System;
    using System.Collections.Generic;

    public interface ISchemaObserver
    {
        void AddKnownType(GraphQLBaseType type);

        GraphQLComplexType[] GetImplementingInterfaces(GraphQLComplexType type);

        IEnumerable<GraphQLBaseType> GetInputKnownTypes();

        IEnumerable<GraphQLBaseType> GetOutputKnownTypes();

        GraphQLBaseType GetSchemaInputTypeFor(Type type);

        GraphQLBaseType GetSchemaTypeFor(Type type);

        Type GetTypeFor(GraphQLBaseType type);

        GraphQLComplexType[] GetTypesImplementing(GraphQLBaseType objectType);
    }
}