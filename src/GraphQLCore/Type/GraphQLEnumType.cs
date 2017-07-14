namespace GraphQLCore.Type
{
    using Introspection;
    using Language.AST;
    using Scalar;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Translation;
    using Utils;

    public class GraphQLEnumType : GraphQLScalarType, ISystemTypeBound
    {
        public Dictionary<string, GraphQLEnumValueInfo> Values { get; }

        protected GraphQLEnumType(string name, string description, Type enumType) : base(name, description)
        {
            this.SystemType = enumType;
            this.Values = this.GetEnumValues(enumType).ToDictionary(e => e.Name, e => e);
        }

        public Type SystemType { get; protected set; }

        public override Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind != ASTNodeKind.EnumValue)
                return Result.Invalid;

            string value = ((GraphQLScalarValue)astValue).Value;

            if (!Enum.IsDefined(this.SystemType, value))
                return Result.Invalid;

            return new Result(Enum.Parse(this.SystemType, value));
        }

        public override NonNullable<IntrospectedType> Introspect(ISchemaRepository schemaRepository)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Name = this.Name;
            introspectedType.Description = this.Description;
            introspectedType.Kind = TypeKind.ENUM;
            introspectedType.EnumValues = this.Values.Select(e => e.Value.Introspect()).ToArray();

            return introspectedType;
        }

        protected override GraphQLValue GetAst(object value, ISchemaRepository schemaRepository)
        {
            if (!Enum.IsDefined(this.SystemType, value))
                return null;

            return new GraphQLScalarValue(ASTNodeKind.EnumValue)
            {
                Value = value.ToString()
            };
        }

        private GraphQLEnumValueInfo[] GetEnumValues(Type type)
        {
            if (!ReflectionUtilities.IsEnum(type))
                throw new ArgumentException("T must be an enum type");

            return Enum.GetNames(type).Select(e => new GraphQLEnumValueInfo()
            {
                Name = e
            }).ToArray();
        }
    }
}