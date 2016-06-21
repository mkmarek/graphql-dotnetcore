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
        public List<__Type> SchemaTypes { get; private set; }
        public GraphQLObjectType RootType { get; private set; }


        public GraphQLSchema()
        {
            this.SchemaTypes = new List<__Type>();
            this.__Schema = new __Schema(this);
        }

        internal void RegisterType(GraphQLScalarType value)
        {
            this.SchemaTypes.Add(new __Type(value, null));

            if (value is GraphQLObjectType)
                this.AddObjectFieldTypes((GraphQLObjectType)value);
        }

        private void AddObjectFieldTypes(GraphQLObjectType value)
        {
            var types = value.GetFieldTypes();

            this.SchemaTypes.AddRange(
                types.Select(e => this.ResolveObjectFieldType(e)).Where(e => e != null));
        }

        private __Type ResolveObjectFieldType(Type type)
        {
            if (typeof(int) == type)
                return new __Type(new GraphQLInt(null), null);

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
