namespace GraphQLCore.Validation.Rules
{
    using Abstract;
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class DefaultValuesOfCorrectTypeVisitor : VariableValidationVisitor
    {
        public DefaultValuesOfCorrectTypeVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(GraphQLVariableDefinition node)
        {
            var name = node.Variable.Name.Value;
            var defaultValue = node.DefaultValue;
            var type = this.GetInputType(node.Type);

            if (type is GraphQLNonNull && defaultValue != null)
            {
                var guessType = ((GraphQLNonNull)type).UnderlyingNullableType;
                this.Errors.Add(new GraphQLException(this.ComposeDefaultForNonNullArgMessage(name, type, guessType)));
            }

            if (type != null && defaultValue != null)
            {
                var errors = this.LiteralValueValidator.IsValid(type, defaultValue);

                if (errors != null && errors.Count() > 0)
                    this.Errors.Add(new GraphQLException(this.ComposeBadValueForDefaultArgMessage(name, type, defaultValue.ToString(), errors.Select(e => e.Message).ToArray())));
            }

            return base.BeginVisitVariableDefinition(node);
        }

        public string ComposeDefaultForNonNullArgMessage(string varName, GraphQLBaseType type, GraphQLBaseType guessType)
        {
            return $"Variable \"${varName}\" of type \"{type.ToString()}\" is required and " +
            "will not use the default value. " +
            $"Perhaps you meant to use type \"{guessType.ToString()}\".";
        }

        public string ComposeBadValueForDefaultArgMessage(string varName, GraphQLBaseType type, string value, string[] verboseErrors = null)
        {
            var message = verboseErrors.Length > 0 ? " \n" + string.Join("\n", verboseErrors) : string.Empty;

            return $"Variable \"${varName}\" of type \"{type.ToString()}\" has invalid " +
            $"default value {value}.{message}";
        }
    }
}
