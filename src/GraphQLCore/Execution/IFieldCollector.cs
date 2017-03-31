namespace GraphQLCore.Execution
{
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public interface IFieldCollector
    {
        Dictionary<string, IList<GraphQLFieldSelection>> CollectFields(GraphQLComplexType runtimeType, GraphQLSelectionSet selectionSet);
    }
}