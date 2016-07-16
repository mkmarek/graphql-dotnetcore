namespace GraphQLCore.Execution
{
    using Language.AST;
    using System;
    using System.Collections;
    using System.Collections.Generic;
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

        public IEnumerable GetListValue(Language.AST.GraphQLValue value)
        {
            IList output = new List<object>();
            var list = ((GraphQLValue<IEnumerable<GraphQLValue>>)value).Value;

            foreach (var item in list)
                output.Add(this.GetValue(item));

            return output;
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
                default: throw new NotImplementedException($"Unknown kind {value.Kind}");
            }
        }

        public object[] FetchArgumentValues(LambdaExpression expression, IList<GraphQLArgument> arguments)
        {
            return ReflectionUtilities.GetParameters(expression)
                .Select(e => ReflectionUtilities.ChangeValueType(this.GetArgumentValue(arguments, e.Name), e.Type))
                .ToArray();
        }
    }
}
