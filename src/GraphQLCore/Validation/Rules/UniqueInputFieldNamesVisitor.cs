namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class UniqueInputFieldNamesVisitor : ValidationASTVisitor
    {
        private Stack<Dictionary<string, GraphQLName>> knownNameStack = new Stack<Dictionary<string, GraphQLName>>();
        private Dictionary<string, GraphQLName> knownNames = new Dictionary<string, GraphQLName>();

        public UniqueInputFieldNamesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLObjectValue BeginVisitObjectValue(GraphQLObjectValue node)
        {
            this.knownNameStack.Push(this.knownNames);
            this.knownNames = new Dictionary<string, GraphQLName>();

            return base.BeginVisitObjectValue(node);
        }

        public override GraphQLObjectValue EndVisitObjectValue(GraphQLObjectValue node)
        {
            this.knownNames = this.knownNameStack.Pop();

            return base.EndVisitObjectValue(node);
        }

        public override GraphQLObjectField BeginVisitObjectField(GraphQLObjectField node)
        {
            var fieldName = node.Name.Value;

            if (this.knownNames.ContainsKey(fieldName))
                this.Errors.Add(new GraphQLException(this.DuplicateInputFieldMessage(fieldName),
                    new[] { this.knownNames[fieldName], node.Name }));
            else
                this.knownNames.Add(fieldName, node.Name);

            return base.BeginVisitObjectField(node);
        }

        private string DuplicateInputFieldMessage(string fieldName)
        {
            return $"There can be only one input field named \"{fieldName}\".";
        }
    }
}