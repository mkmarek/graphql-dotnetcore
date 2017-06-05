using GraphQLCore.Type;
using GraphQLCore.Type.Translation;
using System.Linq;

namespace GraphQLCore.Utils
{
    public static class TypeComparators
    {
        public static bool IsSubtypeOf(
            GraphQLBaseType possibleSubtype,
            GraphQLBaseType superType,
            ISchemaRepository schemaRepository)
        {
            if (possibleSubtype == superType)
            {
                return true;
            }

            // If superType is non-null, maybeSubType must also be nullable.
            if (superType is GraphQLNonNull)
            {
                if (possibleSubtype is GraphQLNonNull)
                {
                    return IsSubtypeOf(
                        ((GraphQLNonNull)possibleSubtype).UnderlyingNullableType,
                        ((GraphQLNonNull)superType).UnderlyingNullableType,
                        schemaRepository);
                }

                return false;
            }
            else if (possibleSubtype is GraphQLNonNull)
            {
                // If superType is nullable, maybeSubType may be non-null.
                return IsSubtypeOf(
                        ((GraphQLNonNull)possibleSubtype).UnderlyingNullableType,
                        superType,
                        schemaRepository);
            }

            // If superType type is a list, maybeSubType type must also be a list.
            if (superType is GraphQLList)
            {
                if (possibleSubtype is GraphQLList)
                {
                    return IsSubtypeOf(
                        ((GraphQLList)possibleSubtype).MemberType,
                        ((GraphQLList)superType).MemberType,
                        schemaRepository);
                }

                return false;
            }
            else if (possibleSubtype is GraphQLList)
            {
                // If superType is not a list, maybeSubType must also be not a list.
                return false;
            }

            // If superType type is an abstract type, maybeSubType type may be a currently
            // possible object type.
            if (IsAbstractType(superType) &&
                possibleSubtype is GraphQLObjectType &&
                IsPossibleType(superType, possibleSubtype, schemaRepository))
            {
                return true;
            }

            // Otherwise, the child type is not a valid subtype of the parent type.
            return false;
        }

        public static bool IsAbstractType(GraphQLBaseType type)
        {
            return type is GraphQLInputObjectType;
        }

        public static bool IsPossibleType(
            GraphQLBaseType parent, GraphQLBaseType child, ISchemaRepository schemaRepository)
        {
            return parent
                .Introspect(schemaRepository)
                .Interfaces.Any(e => e.Name == child.Name);
        }
    }
}
