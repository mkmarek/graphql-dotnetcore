namespace GraphQLCore.Type.Directives
{
    using Complex;
    using Execution;
    using Introspection;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Translation;

    public abstract class GraphQLDirectiveType
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public DirectiveLocation[] Locations { get; private set; }
        public Dictionary<string, GraphQLObjectTypeArgumentInfo> Arguments { get; private set; }

        protected GraphQLDirectiveType(
            string name,
            string description,
            params DirectiveLocation[] locations)
        {
            this.Name = name;
            this.Description = description;
            this.Locations = locations;
            this.Arguments = this.GetArgumentsFromResolver(this.GetResolverInfo())?.ToDictionary(e => e.Name, e => e)
                ?? new Dictionary<string, GraphQLObjectTypeArgumentInfo>();
        }

        public virtual bool PostponeNodeResolve()
        {
            return false;
        }

        public virtual bool PreExecutionIncludeFieldIntoResult(
            GraphQLDirective directive, ISchemaRepository schemaRepository)
        {
            return true;
        }

        public virtual bool PostExecutionIncludeFieldIntoResult(
            GraphQLDirective directive,
            ISchemaRepository schemaRepository,
            object value,
            object parentValue)
        {
            return true;
        }

        public virtual LambdaExpression GetResolver(Func<Task<object>> valueGetter, object parentValue)
        {
            Expression<Func<Task<object>>> resolver = () => valueGetter();

            return resolver;
        }

        public IntrospectedDirective Introspect(ISchemaRepository schemaRepository)
        {
            return new IntrospectedDirective()
            {
                Name = this.Name,
                Description = this.Description,
                Locations = this.Locations,
                Arguments = this.Arguments.Select(e =>
                    e.Value.Introspect(schemaRepository))
                    .ToArray()
            };
        }

        public IEnumerable<GraphQLObjectTypeArgumentInfo> GetArguments()
        {
            return this.Arguments.Values;
        }

        public GraphQLObjectTypeArgumentInfo GetArgument(string name)
        {
            if (this.Arguments.ContainsKey(name))
                return this.Arguments[name];

            return null;
        }

        protected ArgumentDefinitionBuilder Argument(string name)
        {
            if (this.Arguments.ContainsKey(name))
                return new ArgumentDefinitionBuilder(this.Arguments[name]);

            return null;
        }

        private LambdaExpression GetResolverInfo() => this.GetResolver(null, null);

        private IEnumerable<GraphQLObjectTypeArgumentInfo> GetArgumentsFromResolver(LambdaExpression resolver)
        {
            return resolver?.Parameters?.Select(e => new GraphQLObjectTypeArgumentInfo()
            {
                Name = e.Name,
                SystemType = e.Type
            });
        }
    }
}