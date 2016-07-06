namespace GraphQLCore.Type.Translation
{
    using Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Utils;

    public class SchemaObserver : ISchemaObserver
    {
        public SchemaObserver()
        {
            this.bindings = new Dictionary<string, GraphQLNullableType>();
        }

        private Dictionary<string, GraphQLNullableType> bindings { get; set; }

        public void AddKnownType(GraphQLNullableType type)
        {
            var reflectedType = type.GetType();
            var argument = ReflectionUtilities.GetGenericArgumentsEagerly(reflectedType);

            if (argument == null)
                this.bindings.Add(reflectedType.FullName, type);
            else
                this.bindings.Add(argument.FullName, type);
        }

        public IEnumerable<GraphQLNullableType> GetKnownTypes()
        {
            return this.bindings.Select(e => e.Value).ToArray();
        }

        public GraphQLNullableType GetSchemaTypeFor(Type type)
        {
            return this.GetSchemaTypeFor(type, type);
        }

        public Type GetTypeFor(GraphQLScalarType type)
        {
            return ReflectionUtilities.GetGenericArgumentsEagerly(type.GetType());
        }

        public GraphQLComplexType[] GetTypesImplementing(GraphQLNullableType objectType)
        {
            if (objectType is GraphQLInterfaceType)
            {
                var type = ReflectionUtilities.GetGenericArgumentsEagerly(objectType.GetType());
                return this.GetKnownTypes()
                    .Where(e => ReflectionUtilities.GetAllImplementingInterfaces(
                        ReflectionUtilities.GetGenericArgumentsEagerly(e.GetType())).Contains(type))
                    .Select(e => e as GraphQLComplexType)
                    .ToArray();
            }

            return new GraphQLComplexType[] { };
        }

        private GraphQLNullableType GetSchemaTypeFor(Type originalType, Type presumedSchemaType)
        {
            if (this.bindings.ContainsKey(presumedSchemaType.FullName))
                return this.bindings[presumedSchemaType.FullName];

            presumedSchemaType = presumedSchemaType.GetTypeInfo().BaseType;
            if (presumedSchemaType != null)
                return this.GetSchemaTypeFor(originalType, presumedSchemaType);

            throw new GraphQLException($"Unknown type {originalType.FullName} have you added it to known types?");
        }
    }
}