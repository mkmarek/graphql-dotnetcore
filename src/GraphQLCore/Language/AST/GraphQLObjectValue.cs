namespace GraphQLCore.Language.AST
{
    using System.Collections.Generic;
    using System.Linq;

    public class GraphQLObjectValue : GraphQLValue
    {
        public IEnumerable<GraphQLObjectField> Fields { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.ObjectValue;
            }
        }

        public override string ToString()
        {
            var serializedFields = this.Fields.Select(e => $"{e.Name.Value}: {e.Value}");
            var serializedObject = string.Join(", ", serializedFields);

            return $"{{{serializedObject}}}";
        }
    }
}
