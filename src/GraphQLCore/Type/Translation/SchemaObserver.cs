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
            this.outputBindings = new Dictionary<string, GraphQLNullableType>();
            this.inputBindings = new Dictionary<string, GraphQLNullableType>();
        }

        private Dictionary<string, GraphQLNullableType> inputBindings;
        private Dictionary<string, GraphQLNullableType> outputBindings;

        public void AddKnownType(GraphQLNullableType type)
        {
            if (type is GraphQLInputObjectType)
            {
                this.AddInputObjectKnownType(type);
            }
            else if (type is GraphQLComplexType)
            {
                this.AddOutputObjectKnownType(type);
            }
            else
            {
                this.AddInputObjectKnownType(type);
                this.AddOutputObjectKnownType(type);
            }
        }

        public IEnumerable<GraphQLNullableType> GetInputKnownTypes()
        {
            return this.inputBindings.Select(e => e.Value).ToList();
        }

        public IEnumerable<GraphQLNullableType> GetOutputKnownTypes()
        {
            return this.outputBindings.Select(e => e.Value).ToList();
        }

        public GraphQLNullableType GetSchemaInputTypeFor(Type type)
        {
            if (this.inputBindings.ContainsKey(type.FullName))
                return this.inputBindings[type.FullName];

            throw new GraphQLException($"Unknown input type {type.FullName} have you added it to known types?");
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
                return this.GetOutputKnownTypes()
                    .Where(e => ReflectionUtilities.GetAllImplementingInterfaces(
                        ReflectionUtilities.GetGenericArgumentsEagerly(e.GetType())).Contains(type))
                    .Select(e => e as GraphQLComplexType)
                    .ToArray();
            }

            return new GraphQLComplexType[] { };
        }

        private void AddInputObjectKnownType(GraphQLNullableType type)
        {
            var reflectedType = type.GetType();
            var argument = ReflectionUtilities.GetGenericArgumentsEagerly(reflectedType);

            if (argument == null)
                this.inputBindings.Add(reflectedType.FullName, type);
            else
                this.inputBindings.Add(argument.FullName, type);
        }

        private void AddOutputObjectKnownType(GraphQLNullableType type)
        {
            var reflectedType = type.GetType();
            var argument = ReflectionUtilities.GetGenericArgumentsEagerly(reflectedType);

            if (argument == null)
                this.outputBindings.Add(reflectedType.FullName, type);
            else
                this.outputBindings.Add(argument.FullName, type);
        }

        private GraphQLNullableType GetSchemaTypeFor(Type originalType, Type presumedSchemaType)
        {
            if (this.outputBindings.ContainsKey(presumedSchemaType.FullName))
                return this.outputBindings[presumedSchemaType.FullName];

            presumedSchemaType = presumedSchemaType.GetTypeInfo().BaseType;
            if (presumedSchemaType != null)
                return this.GetSchemaTypeFor(originalType, presumedSchemaType);

            throw new GraphQLException($"Unknown type {originalType.FullName} have you added it to known types?");
        }
    }
}