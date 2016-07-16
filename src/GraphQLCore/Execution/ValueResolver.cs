namespace GraphQLCore.Execution
{
    using Language.AST;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Type.Translation;
    using Utils;

    public class ValueResolver : IValueResolver
    {
        private IVariableResolver variableResolver;
        private ITypeTranslator typeTranslator;

        public ValueResolver(IVariableResolver variableResolver, ITypeTranslator typeTranslator)
        {
            this.variableResolver = variableResolver;
            this.typeTranslator = typeTranslator;
        }

        public object GetArgumentValue(IEnumerable<GraphQLArgument> arguments, string argumentName)
        {
            var argument = arguments.SingleOrDefault(e => e.Name.Value == argumentName);

            if (argument == null)
                return null;

            return this.GetValue(argument.Value);
        }

        public object GetValue(GraphQLValue value)
        {
            var literalValue = this.typeTranslator.GetLiteralValue(value);

            if (literalValue != null)
                return literalValue;

            switch (value.Kind)
            {
                case ASTNodeKind.ListValue: return this.GetListValue(value);
                case ASTNodeKind.Variable: return this.variableResolver.GetValue((GraphQLVariable)value);
                case ASTNodeKind.ObjectValue: return this.CreateObjectFromObjectValue((GraphQLObjectValue)value);
                default: throw new NotImplementedException($"Unknown kind {value.Kind}");
            }
        }

        public object[] FetchArgumentValues(LambdaExpression expression, IList<GraphQLArgument> arguments)
        {
            return ReflectionUtilities.GetParameters(expression)
                .Select(e => this.typeTranslator.TranslatePerDefinition(this.GetArgumentValue(arguments, e.Name), e.Type))
                .ToArray();
        }

        private IEnumerable GetListValue(GraphQLValue value)
        {
            IList output = new List<object>();
            var list = ((GraphQLValue<IEnumerable<GraphQLValue>>)value).Value;

            foreach (var item in list)
                output.Add(this.GetValue(item));

            return output;
        }

        private object CreateObjectFromObjectValue(GraphQLObjectValue value)
        {
            var result = new ExpandoObject();
            var resultDictionary = (IDictionary<string, object>)result;

            if (value.Fields != null)
                this.AssignFields(value, resultDictionary);

            return result;
        }

        private void AssignFields(GraphQLObjectValue value, IDictionary<string, object> resultDictionary)
        {
            foreach (var field in value.Fields)
                resultDictionary[field.Name.Value] = this.GetValue(field.Value);
        }
    }
}
