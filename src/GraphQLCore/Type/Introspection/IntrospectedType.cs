namespace GraphQLCore.Type.Introspection
{
    using System.Collections.Generic;
    using System.Linq;

    public class IntrospectedType
    {
        public string Description { get; set; }

        public virtual NonNullable<IntrospectedEnumValue>[] EnumValues { get; set; }

        public virtual NonNullable<IntrospectedField>[] Fields { get { return null; } }

        public virtual NonNullable<IntrospectedInputValue>[] InputFields { get { return null; } }

        public virtual NonNullable<IntrospectedType>[] Interfaces { get { return null; } }

        public TypeKind Kind { get; set; }

        public string Name { get; set; }

        public IntrospectedType OfType { get; set; }

        public virtual NonNullable<IntrospectedType>[] PossibleTypes { get { return null; } }

        public IEnumerable<NonNullable<IntrospectedField>> GetFields(bool includeDeprecated)
        {
            return this.Fields?.Where(e => includeDeprecated || !e.Value.IsDeprecated);
        }

        public IEnumerable<NonNullable<IntrospectedEnumValue>> GetEnumValues(bool includeDeprecated)
        {
            return this.EnumValues?.Where(e => includeDeprecated || !e.Value.IsDeprecated);
        }
    }
}
