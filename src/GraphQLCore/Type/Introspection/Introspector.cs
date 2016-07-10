namespace GraphQLCore.Type.Introspection
{
    using System;
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
                return this.IntrospectListType((GraphQLList)type, () => this.Introspect(((GraphQLList)type).MemberType));

            if (type is GraphQLNonNullType)
                return this.IntrospectNonNullType((GraphQLNonNullType)type, () => this.Introspect(((GraphQLNonNullType)type).UnderlyingNullableType));

            if (type is GraphQLEnumType)
                return this.IntrospectEnumType((GraphQLEnumType)type);

            if (type is GraphQLObjectType)
                return this.IntrospectObjectType((GraphQLObjectType)type);

            if (type is GraphQLInterfaceType)
                return this.IntrospectInterfaceType((GraphQLInterfaceType)type);

            if (type is GraphQLInputObjectType)
                return this.IntrospectInputObjectType((GraphQLInputObjectType)type);

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

        public IntrospectedInputValue IntrospectInputValue(GraphQLFieldConfig fieldConfig)
        {
            return new IntrospectedInputValue()
            {
                Name = fieldConfig.Name,
                Description = fieldConfig.Description,
                Type = this.Introspect(fieldConfig.Type)
            };
        }

        private IntrospectedInputValue IntrospectArgument(KeyValuePair<string, GraphQLScalarType> argument)
        {
            return new IntrospectedInputValue()
            {
                Name = argument.Key,
                Type = this.Introspect(argument.Value)
            };
        }

        private IntrospectedInputValue[] IntrospectArguments(GraphQLFieldConfig fieldConfig)
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

        private IntrospectedType IntrospectListType(GraphQLList type, Func<IntrospectedType> innerType)
        {
            return IntrospectedType.CreateForList(type, innerType());
        }

        private IntrospectedType IntrospectNonNullType(GraphQLNonNullType type, Func<IntrospectedType> innerType)
        {
            return IntrospectedType.CreateForNonNull(innerType());
        }

        private IntrospectedType IntrospectObjectType(GraphQLObjectType type)
        {
            return IntrospectedType.CreateForObject(type, this, this.typeTranslator.GetObjectTypeTranslatorFor(type));
        }

        private IntrospectedType IntrospectInputObjectType(GraphQLInputObjectType type)
        {
            return IntrospectedType.CreateForInputObject(type, this, this.typeTranslator.GetObjectTypeTranslatorFor(type));
        }

        private IntrospectedType IntrospectScalarType(GraphQLScalarType scalarType)
        {
            return IntrospectedType.CreateForScalar(scalarType);
        }
    }
}