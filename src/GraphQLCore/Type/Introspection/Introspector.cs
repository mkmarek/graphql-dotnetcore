namespace GraphQLCore.Type.Introspection
{
    using System.Collections.Generic;
    using System.Linq;
    using Translation;

    public class Introspector : IIntrospector
    {
        private ITypeTranslator typeTranslator;

        public Introspector(ITypeTranslator typeTranslator)
        {
            this.typeTranslator = typeTranslator;
        }

        public IntrospectedType Introspect(GraphQLScalarType type)
        {
            if (type is GraphQLList)
                return this.IntrospectListType((GraphQLList)type);

            if (type is GraphQLNonNullType)
                return this.IntrospectNonNullType((GraphQLNonNullType)type);

            if (type is GraphQLEnumType)
                return this.IntrospectEnumType((GraphQLEnumType)type);

            if (type is GraphQLObjectType)
                return this.IntrospectObjectType((GraphQLObjectType)type);

            if (type is GraphQLInterfaceType)
                return this.IntrospectInterfaceType((GraphQLInterfaceType)type);

            return this.IntrospectScalarType(type);
        }

        public IntrospectedField IntrospectField(GraphQLFieldConfig fieldConfig)
        {
            return new IntrospectedField()
            {
                Name = fieldConfig.Name,
                Description = fieldConfig.Description,
                Type = this.Introspect(fieldConfig.Type),
                Arguments = this.IntrospectArguments(fieldConfig)
            };
        }

        private IntrospectedArgument IntrospectArgument(KeyValuePair<string, GraphQLScalarType> argument)
        {
            return new IntrospectedArgument()
            {
                Name = argument.Key,
                Type = this.Introspect(argument.Value)
            };
        }

        private IntrospectedArgument[] IntrospectArguments(GraphQLFieldConfig fieldConfig)
        {
            return fieldConfig.Arguments.Select(e => this.IntrospectArgument(e)).ToArray();
        }

        private IntrospectedType IntrospectEnumType(GraphQLEnumType type)
        {
            return IntrospectedType.CreateForEnum(type, GraphQLEnumValue.GetEnumValuesFor(type.EnumType));
        }

        private IntrospectedType IntrospectInterfaceType(GraphQLInterfaceType type)
        {
            return IntrospectedType.CreateForInterface(type, this, this.typeTranslator.GetObjectTypeTranslatorFor(type));
        }

        private IntrospectedType IntrospectListType(GraphQLList type)
        {
            return IntrospectedType.CreateForList(type, this.Introspect(type.MemberType));
        }

        private IntrospectedType IntrospectNonNullType(GraphQLNonNullType type)
        {
            var underlyingType = this.Introspect(type.UnderlyingNullableType);

            return IntrospectedType.CreateForNonNull(underlyingType);
        }

        private IntrospectedType IntrospectObjectType(GraphQLObjectType type)
        {
            return IntrospectedType.CreateForObject(type, this, this.typeTranslator.GetObjectTypeTranslatorFor(type));
        }

        private IntrospectedType IntrospectScalarType(GraphQLScalarType scalarType)
        {
            return IntrospectedType.CreateForScalar(scalarType);
        }
    }
}