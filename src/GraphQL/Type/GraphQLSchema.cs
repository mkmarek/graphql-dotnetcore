namespace GraphQL.Type
{
    using Execution;
    using Introspection;
    using Language;
    using Language.AST;
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using Scalars;
    using System.Reflection;
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
            var types = this.IntrospectObjectFieldTypes(objectType);

            foreach (var type in types)
            {
                if (!GetTypeNames(result).Contains(GetTypeName(type)))
                    result.Add(type);
            }
        }

        private static string[] GetTypeNames(List<__Type> typeList)
        {
            return typeList.Select(e => GetTypeName(e)).ToArray();
        }

        private static string GetTypeName(__Type e)
        {
            return (string)e.ResolveField("name");
        }

        internal void RegisterType(GraphQLScalarType value)
        {
            this.schemaTypes.Add(value);
        }

        private IEnumerable<__Type> IntrospectObjectFieldTypes(GraphQLObjectType value)
        {
            var types = value.GetFieldTypes();

            return types.Select(e => this.ResolveObjectFieldType(e))
                .Where(e => e != null);
        }

        private __Type ResolveObjectFieldType(Type type)
        {
            if (typeof(int) == type)
                return new __Type(new GraphQLInt(null), null);

            if (typeof(bool) == type)
                return new __Type(new GraphQLBoolean(null), null);

            if (typeof(float) == type || typeof(double) == type)
                return new __Type(new GraphQLFloat(null), null);

            if (typeof(string) == type)
                return new __Type(new GraphQLString(null), null);

            return null;
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
