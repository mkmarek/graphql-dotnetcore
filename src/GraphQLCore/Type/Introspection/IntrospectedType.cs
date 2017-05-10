namespace GraphQLCore.Type.Introspection
{
    using System.Collections.Generic;
    using System.Linq;

    public class IntrospectedType
    {
        public string Description { get; set; }

        public virtual GraphQLEnumValue[] EnumValues { get; set; }

        public virtual IntrospectedField[] Fields { get { return null; } }

        public virtual IntrospectedInputValue[] InputFields { get { return null; } }

        public virtual IntrospectedType[] Interfaces { get { return null; } }

        public TypeKind Kind { get; set; }

        public string Name { get; set; }

        public IntrospectedType OfType { get; set; }

        public virtual IntrospectedType[] PossibleTypes { get { return null; } }

        public IEnumerable<IntrospectedField> GetFields(bool includeDeprecated)
        {
            return this.Fields?.Where(e => includeDeprecated || !e.IsDeprecated);
        }

        public IEnumerable<GraphQLEnumValue> GetEnumValues(bool includeDeprecated)
        {
            // TODO filter by includeDeprecated

            return this.EnumValues;
        }
    }
}
