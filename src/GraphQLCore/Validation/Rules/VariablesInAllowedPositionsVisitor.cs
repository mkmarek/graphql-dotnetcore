namespace GraphQLCore.Validation.Rules
{
    using Abstract;
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;
    using Utils;

    public class VariablesInAllowedPositionsVisitor : VariableValidationVisitor
    {
        private GraphQLDocument document;
        private Dictionary<string, GraphQLVariableDefinition> variableDefinitions;

        public VariablesInAllowedPositionsVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            this.variableDefinitions = new Dictionary<string, GraphQLVariableDefinition>();

            return base.BeginVisitOperationDefinition(definition);
        }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(GraphQLVariableDefinition node)
        {
            var name = node.Variable.Name.Value;

            if (!this.variableDefinitions.ContainsKey(name))
                this.variableDefinitions.Add(node.Variable.Name.Value, node);

            return base.BeginVisitVariableDefinition(node);
        }

        public override GraphQLOperationDefinition EndVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            var variableUsages = VariableUsagesProvider.Get(definition, this.document, this.Schema);

            foreach (var usage in variableUsages)
                this.VerifyUsage(usage);

            return base.EndVisitOperationDefinition(definition);
        }

        public override void Visit(GraphQLDocument ast)
        {
            this.document = ast;

            base.Visit(ast);
        }

        private GraphQLBaseType GetEffectiveType(GraphQLBaseType type, GraphQLVariableDefinition definition)
        {
            return definition.DefaultValue == null || type is Type.GraphQLNonNullType
                ? type
                : new Type.GraphQLNonNullType(type);
        }

        private void VerifyUsage(VariableUsage usage)
        {
            var variableName = usage.Variable.Name.Value;

            if (this.variableDefinitions.ContainsKey(variableName))
            {
                var variableDefinition = this.variableDefinitions[variableName];
                var variableType = this.GetInputType(this.variableDefinitions[variableName].Type);

                if (!TypeComparators.IsSubtypeOf(
                    this.GetEffectiveType(variableType, variableDefinition),
                    usage.ArgumentType,
                    this.SchemaRepository))
                {
                    this.Errors.Add(new GraphQLException(
                        $"Variable \"${variableName}\" of type \"{variableType}\" used in " +
                        $"position expecting type \"{usage.ArgumentType}\"."));
                }
            }
        }
    }
}