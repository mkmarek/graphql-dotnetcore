using GraphQLCore.Execution;

namespace GraphQLCore.Type.Translation
{
    using GraphQLCore.Type.Directives;
    using System;
    using System.Collections.Generic;

    public interface ISchemaRepository
    {
        IVariableResolver VariableResolver { get; set; }

        void AddKnownType(GraphQLBaseType type);

        void AddOrReplaceDirective(GraphQLDirectiveType directive);

        GraphQLDirectiveType GetDirective(string name);

        IEnumerable<GraphQLDirectiveType> GetDirectives();

        GraphQLComplexType[] GetImplementingInterfaces(GraphQLComplexType type);

        IEnumerable<GraphQLInputType> GetInputKnownTypes();

        IEnumerable<GraphQLBaseType> GetOutputKnownTypes();

        GraphQLInputType GetSchemaInputTypeFor(Type type);

        GraphQLBaseType GetSchemaTypeFor(Type type);

        Type GetInputSystemTypeFor(GraphQLBaseType type);

        GraphQLComplexType[] GetTypesImplementing(GraphQLInterfaceType objectType);

        GraphQLComplexType[] GetPossibleTypesForUnion(GraphQLUnionType unionType);

        GraphQLInputType GetSchemaInputTypeByName(string value);

        GraphQLBaseType GetSchemaOutputTypeByName(string value);
    }
}
