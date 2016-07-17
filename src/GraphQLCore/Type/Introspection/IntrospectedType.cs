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

        protected GraphQLBaseType GetInputTypeFrom(System.Type type, ISchemaObserver schemaObserver)
        {
            if (ReflectionUtilities.IsCollection(type))
                return new GraphQLList(this.GetInputTypeFrom(ReflectionUtilities.GetCollectionMemberType(type), schemaObserver));

            if (ReflectionUtilities.IsNullable(type))
                return schemaObserver.GetSchemaTypeFor(Nullable.GetUnderlyingType(type));

            if (ReflectionUtilities.IsValueType(type))
                return new GraphQLNonNullType(schemaObserver.GetSchemaInputTypeFor(type));

            return schemaObserver.GetSchemaInputTypeFor(type);
        }

        protected GraphQLBaseType GetOutputTypeFrom(System.Type type, ISchemaObserver schemaObserver)
        {
            if (ReflectionUtilities.IsCollection(type))
                return new GraphQLList(this.GetOutputTypeFrom(ReflectionUtilities.GetCollectionMemberType(type), schemaObserver));

            if (ReflectionUtilities.IsNullable(type))
                return schemaObserver.GetSchemaTypeFor(Nullable.GetUnderlyingType(type));

            if (ReflectionUtilities.IsValueType(type))
                return new GraphQLNonNullType(schemaObserver.GetSchemaTypeFor(type));

            return schemaObserver.GetSchemaTypeFor(type);
        }
    }
}