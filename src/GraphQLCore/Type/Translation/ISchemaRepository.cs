using GraphQLCore.Execution;

namespace GraphQLCore.Type.Translation
{
    using System;
    using System.Collections.Generic;

    public interface ISchemaRepository
    {
        IVariableResolver VariableResolver { get; set; }

        void AddKnownType(GraphQLBaseType type);

        GraphQLComplexType[] GetImplementingInterfaces(GraphQLComplexType type);

        IEnumerable<GraphQLInputType> GetInputKnownTypes();

        IEnumerable<GraphQLBaseType> GetOutputKnownTypes();

        GraphQLInputType GetSchemaInputTypeFor(Type type);

        GraphQLBaseType GetSchemaTypeFor(Type type);

        Type GetInputSystemTypeFor(GraphQLBaseType type);

        GraphQLComplexType[] GetTypesImplementing(GraphQLInterfaceType objectType);

        GraphQLInputType GetSchemaInputTypeByName(string value);

        GraphQLBaseType GetSchemaOutputTypeByName(string value);
    }
}