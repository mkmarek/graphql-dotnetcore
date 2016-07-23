using GraphQLCore.Type.Translation;
using GraphQLCore.Utils;
using System;

namespace GraphQLCore.Type.Introspection
{
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
    }
}