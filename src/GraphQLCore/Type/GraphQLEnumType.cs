namespace GraphQLCore.Type
{
    using Introspection;
    using Language.AST;
    using Scalar;
    using System;
    using Translation;

    public class GraphQLEnumType : GraphQLScalarType, ISystemTypeBound
    {
        public GraphQLEnumType(string name, string description, Type enumType) : base(name, description)
        {
            this.SystemType = enumType;
        }

        public Type SystemType { get; protected set; }

        public override object GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind != ASTNodeKind.EnumValue)
                return null;

            string value = ((GraphQLScalarValue)astValue).Value;

            if (!Enum.IsDefined(this.SystemType, value))
                return null;

            return Enum.Parse(this.SystemType, value);
        }

        public override IntrospectedType Introspect(ISchemaRepository schemaRepository)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Name = this.Name;
            introspectedType.Description = this.Description;
            introspectedType.Kind = TypeKind.ENUM;
            introspectedType.EnumValues = IntrospectedEnumValue.GetEnumValuesFor(this.SystemType);

            return introspectedType;
        }
    }
}