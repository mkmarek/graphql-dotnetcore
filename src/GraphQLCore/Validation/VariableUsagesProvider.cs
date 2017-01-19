namespace GraphQLCore.Validation
{
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class VariableUsagesProvider : ValidationASTVisitor
    {
        private IDictionary<string, GraphQLFragmentDefinition> fragments;
        private List<VariableUsage> variableUsages;

        private VariableUsagesProvider(IDictionary<string, GraphQLFragmentDefinition> fragments, IGraphQLSchema schema)
            : base(schema)
        {
            this.variableUsages = new List<VariableUsage>();
            this.fragments = fragments;
        }

        public static IEnumerable<VariableUsage> Get(
            GraphQLOperationDefinition operation, GraphQLDocument document, IGraphQLSchema schema)
        {
            var visitor = new VariableUsagesProvider(GetFragmentsFromDocument(document), schema);

            visitor.BeginVisitOperationDefinition(operation);

            return visitor.variableUsages;
        }

        public override GraphQLFragmentSpread BeginVisitFragmentSpread(GraphQLFragmentSpread fragmentSpread)
        {
            if (this.fragments.ContainsKey(fragmentSpread.Name.Value))
            {
                var fragment = this.fragments[fragmentSpread.Name.Value];
                this.fragments.Remove(fragmentSpread.Name.Value);

                this.BeginVisitFragmentDefinition(fragment);
            }

            return base.BeginVisitFragmentSpread(fragmentSpread);
        }

        public override GraphQLArgument EndVisitArgument(GraphQLArgument argument)
        {
            if (argument.Value is GraphQLVariable)
            {
                var usage = this.CreateUsage(argument);

                if (usage != null)
                    this.variableUsages.Add(usage);
            }

            return base.EndVisitArgument(argument);
        }

        public override GraphQLObjectField BeginVisitObjectField(GraphQLObjectField node)
        {
            if (node.Value is GraphQLVariable)
            {
                var usage = this.CreateUsage(node);

                if (usage != null)
                    this.variableUsages.Add(usage);
            }

            return base.BeginVisitObjectField(node);
        }

        private static IDictionary<string, GraphQLFragmentDefinition> GetFragmentsFromDocument(GraphQLDocument document)
        {
            var fragments = new Dictionary<string, GraphQLFragmentDefinition>();

            foreach (var definition in document.Definitions)
            {
                if (definition.Kind == ASTNodeKind.FragmentDefinition)
                {
                    var fragment = (GraphQLFragmentDefinition)definition;
                    if (!fragments.ContainsKey(fragment.Name.Value))
                        fragments.Add(fragment.Name.Value, fragment);
                }
            }

            return fragments;
        }

        private VariableUsage CreateUsage(GraphQLObjectField node)
        {
            var field = (this.GetLastField()
                ?.GetGraphQLType(this.SchemaRepository) as GraphQLComplexType)
                ?.GetFieldInfo(node.Name.Value);

            if (field == null)
                return null;

            return new VariableUsage()
            {
                ArgumentType = field.GetGraphQLType(this.SchemaRepository),
                Variable = node.Value as GraphQLVariable
            };
        }

        private VariableUsage CreateUsage(GraphQLArgument argument)
        {
            var arguments = this.GetLastField().Arguments;

            if (!arguments.ContainsKey(argument.Name.Value))
                return null;

            return new VariableUsage()
            {
                ArgumentType = arguments[argument.Name.Value].GetGraphQLType(this.SchemaRepository),
                Variable = argument.Value as GraphQLVariable
            };
        }
    }
}