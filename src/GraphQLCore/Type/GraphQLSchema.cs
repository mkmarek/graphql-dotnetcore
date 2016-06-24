namespace GraphQLCore.Type
{
    using Execution;
    using Introspection;
    using Language;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Utils;

    public class GraphQLSchema
    {
        public __Schema __Schema { get; private set; }
        public GraphQLObjectType RootType { get; private set; }

        private List<GraphQLScalarType> schemaTypes;

        public GraphQLSchema()
        {
            this.schemaTypes = new List<GraphQLScalarType>();
            this.schemaTypes.Add(new __Type("", "", null));
            this.__Schema = new __Schema(this);
        }

        public IEnumerable<__Type> Introspect()
        {
            var result = new List<__Type>();

            foreach (var type in this.schemaTypes)
            {
                result.Add(new __Type(type, null));
                if (type is GraphQLObjectType)
                    AppendObjectTypes(result, (GraphQLObjectType)type);
            }

            return result;
        }

        private void AppendObjectTypes(List<__Type> result, GraphQLObjectType objectType)
        {
            var types = TypeUtilities.IntrospectObjectFieldTypes(objectType);

            foreach (var type in types)
            {
                if (!TypeUtilities.GetTypeNames(result).Contains(TypeUtilities.GetTypeName(type)))
                    result.Add(type);
            }
        }

        internal void RegisterType(GraphQLScalarType value)
        {
            this.schemaTypes.Add(value);
        }

        public void SetRoot(GraphQLObjectType root)
        {
            this.RootType = root;
;        }

        public dynamic Execute(string expression)
        {
            using (var context = new ExecutionContext(this, this.GetAst(expression)))
            {
                return context.Execute();
            }
        }

        private GraphQLDocument GetAst(string expression)
        {
            return new Parser(new Lexer()).Parse(new Source(expression));
        }
    }
}
