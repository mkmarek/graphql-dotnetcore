namespace GraphQLCore.Type
{
    using Introspection;
    using Language.AST;
    using System;
    using Translation;

    public class GraphQLEnumType : GraphQLInputType
    {
        public GraphQLEnumType(string name, string description, Type enumType) : base(name, description)
        {
            this.EnumType = enumType;
        }

        public Type EnumType { get; set; }

        public override object GetFromAst(GraphQLValue astValue)
        {
            if (astValue.Kind != ASTNodeKind.EnumValue)
                return null;

            string value = ((GraphQLScalarValue)astValue).Value;

            if (!Enum.IsDefined(this.EnumType, value))
                return null;

            return Enum.Parse(this.EnumType, value);
        }

        public override IntrospectedType Introspect(ISchemaObserver schemaObserver)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Name = this.Name;
            introspectedType.Description = this.Description;
            introspectedType.Kind = TypeKind.ENUM;
            introspectedType.EnumValues = GraphQLEnumValue.GetEnumValuesFor(this.EnumType);

            return introspectedType;
        }
    }
}