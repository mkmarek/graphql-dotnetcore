namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class ScalarLeafsVisitor : ValidationASTVisitor
    {
        public ScalarLeafsVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLFieldSelection EndVisitFieldSelection(GraphQLFieldSelection selection)
        {
            var type = this.GetLastType();
            var field = this.GetLastField();

            if (type.IsLeafType && selection?.SelectionSet != null)
            {
                this.Errors.Add(new GraphQLException(
                    this.NoScalarSubselection(field.Name, type)));
            }
            else if (!type.IsLeafType && selection?.SelectionSet == null)
            {
                this.Errors.Add(new GraphQLException(
                    this.RequiredSubselectionMessage(field.Name, type)));
            }

            return base.EndVisitFieldSelection(selection);
        }

        private string NoScalarSubselection(string fieldName, GraphQLBaseType type)
        {
            return $"Field \"{fieldName}\" must not have a selection since " +
                   $"type \"{type}\" has no subfields.";
        }

        private string RequiredSubselectionMessage(string fieldName, GraphQLBaseType type)
        {
            return $"Field \"{fieldName}\" of type \"{type}\" must have a " +
                   $"selection of subfields. Did you mean \"{fieldName} " + "{ ... }\"?";
        }
    }
}