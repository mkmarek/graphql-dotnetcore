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
        public List<GraphQLScalarType> SchemaTypes { get; private set; }

        public GraphQLSchema()
        {
            this.SchemaTypes = new List<GraphQLScalarType>();
            this.SchemaTypes.Add(new __Type(this));
            this.SchemaTypes.Add(new __TypeKind(this));
            this.SchemaTypes.Add(new __InputValue(null, this));
            this.__Schema = new __Schema(this);
        }

        public __Schema __Schema { get; private set; }
        public GraphQLObjectType RootType { get; private set; }

        public dynamic Execute(string expression)
        {
            using (var context = new ExecutionContext(this, this.GetAst(expression)))
            {
                return context.Execute();
            }
        }

        public IEnumerable<__Type> Introspect()
        {
            var result = new List<__Type>();

            foreach (var type in this.SchemaTypes)
            {
                if (!TypeUtilities.GetTypeNames(result).Contains(type.Name))
                {
                    result.Add(new __Type(type, this));
                    if (type is GraphQLObjectType)
                        AppendObjectTypes(result, (GraphQLObjectType)type);
                }
            }

            return result.Where(e => TypeUtilities.GetTypeName(e) != null)
                .ToList();
        }

        public void Query(GraphQLObjectType root)
        {
            this.RootType = root;
            ;
        }

        internal __Type GetGraphQLType(string name)
        {
            return this.Introspect().Where(e => TypeUtilities.GetTypeName(e) == name).FirstOrDefault();
        }

        internal void RegisterType(GraphQLScalarType value)
        {
            this.SchemaTypes.Add(value);
        }

        private void AppendObjectTypes(List<__Type> result, GraphQLObjectType objectType)
        {
            var types = TypeResolver.IntrospectObjectFieldTypes(objectType, this);

            foreach (var type in types)
            {
                if (!TypeUtilities.GetTypeNames(result).Contains(TypeUtilities.GetTypeName(type)))
                    result.Add(type);
            }
        }

        private GraphQLDocument GetAst(string expression)
        {
            return new Parser(new Lexer()).Parse(new Source(expression));
        }
    }
}