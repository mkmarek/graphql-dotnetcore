namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class UniqueInputFieldNamesVisitor : ValidationASTVisitor
    {
        private Stack<Dictionary<string, bool>> knownNameStack = new Stack<Dictionary<string, bool>>();
        private Dictionary<string, bool> knownNames = new Dictionary<string, bool>();

        public UniqueInputFieldNamesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLObjectValue BeginVisitObjectValue(GraphQLObjectValue node)
        {
            this.knownNameStack.Push(this.knownNames);
            this.knownNames = new Dictionary<string, bool>();

            return base.BeginVisitObjectValue(node);
        }

        public override GraphQLObjectValue EndVisitObjectValue(GraphQLObjectValue node)
        {
            this.knownNames = this.knownNameStack.Pop();

            return base.EndVisitObjectValue(node);
        }

        public override GraphQLObjectField BeginVisitObjectField(GraphQLObjectField node)
        {
            string fieldName = node.Name.Value;

            if (this.knownNames.ContainsKey(fieldName))
                this.Errors.Add(new GraphQLException(this.DuplicateInputFieldMessage(fieldName)));
            else
                this.knownNames.Add(fieldName, true);

            return base.BeginVisitObjectField(node);
        }

        private string DuplicateInputFieldMessage(string fieldName)
        {
            return $"There can be only one input field named \"{fieldName}\".";
        }
    }
}