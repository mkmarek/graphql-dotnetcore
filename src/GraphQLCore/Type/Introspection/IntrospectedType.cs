namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedType
    {
        internal IntrospectedType()
        {
        }

        public string Description { get; protected set; }

        public virtual GraphQLEnumValue[] EnumValues { get; protected set; }

        public virtual IntrospectedField[] Fields { get { return null; } }

        public virtual IntrospectedType[] Interfaces { get { return null; } }

        public TypeKind Kind { get; protected set; }

        public string Name { get; protected set; }

        public IntrospectedType OfType { get; protected set; }

        public virtual IntrospectedType[] PossibleTypes { get { return null; } }

        public static IntrospectedType CreateForInterface(GraphQLInterfaceType type, IIntrospector introspector, IObjectTypeTranslator typeObserver)
        {
            var introspectedType = new ComplexIntrospectedType(introspector, typeObserver);
            introspectedType.Name = type.Name;
            introspectedType.Description = type.Description;
            introspectedType.Kind = TypeKind.INTERFACE;

            return introspectedType;
        }

        public static IntrospectedType CreateForObject(GraphQLObjectType type, IIntrospector introspector, IObjectTypeTranslator typeObserver)
        {
            var introspectedType = new ComplexIntrospectedType(introspector, typeObserver);
            introspectedType.Name = type.Name;
            introspectedType.Description = type.Description;
            introspectedType.Kind = TypeKind.OBJECT;

            return introspectedType;
        }

        public static IntrospectedType CreateForScalar(GraphQLScalarType type)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Name = type.Name;
            introspectedType.Description = type.Description;
            introspectedType.Kind = TypeKind.SCALAR;

            return introspectedType;
        }

        internal static IntrospectedType CreateForEnum(GraphQLEnumType type, GraphQLEnumValue[] enumValues)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Name = type.Name;
            introspectedType.Description = type.Description;
            introspectedType.Kind = TypeKind.ENUM;
            introspectedType.EnumValues = enumValues;

            return introspectedType;
        }

        internal static IntrospectedType CreateForList(GraphQLList type, IntrospectedType listItemType)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Name = type.Name;
            introspectedType.Description = type.Description;
            introspectedType.Kind = TypeKind.LIST;
            introspectedType.OfType = listItemType;

            return introspectedType;
        }

        internal static IntrospectedType CreateForNonNull(IntrospectedType underlyingType)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Kind = TypeKind.NON_NULL;
            introspectedType.OfType = underlyingType;

            return introspectedType;
        }
    }
}