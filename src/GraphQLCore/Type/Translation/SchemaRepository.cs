namespace GraphQLCore.Type.Translation
{
    using Exceptions;
    using Scalar;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Utils;

    public class SchemaRepository : ISchemaRepository
    {
        private Dictionary<Type, GraphQLInputType> inputBindings;

        private Dictionary<Type, GraphQLBaseType> outputBindings;

        public SchemaRepository()
        {
            this.outputBindings = new Dictionary<Type, GraphQLBaseType>();
            this.inputBindings = new Dictionary<Type, GraphQLInputType>();

            var graphQLInt = new GraphQLInt();
            var graphQLFloat = new GraphQLFloat();
            var graphQLBoolean = new GraphQLBoolean();
            var graphQLString = new GraphQLString();

            this.outputBindings.Add(typeof(string), graphQLString);
            this.inputBindings.Add(typeof(string), graphQLString);

            this.outputBindings.Add(typeof(int), graphQLInt);
            this.outputBindings.Add(typeof(float), graphQLFloat);
            this.outputBindings.Add(typeof(bool), graphQLBoolean);

            this.inputBindings.Add(typeof(int), graphQLInt);
            this.inputBindings.Add(typeof(float), graphQLFloat);
            this.inputBindings.Add(typeof(bool), graphQLBoolean);

            this.outputBindings.Add(typeof(int?), graphQLInt);
            this.outputBindings.Add(typeof(float?), graphQLFloat);
            this.outputBindings.Add(typeof(bool?), graphQLBoolean);

            this.inputBindings.Add(typeof(int?), graphQLInt);
            this.inputBindings.Add(typeof(float?), graphQLFloat);
            this.inputBindings.Add(typeof(bool?), graphQLBoolean);
        }

        public void AddKnownType(GraphQLBaseType type)
        {
            if (type is GraphQLInputType)
            {
                this.AddInputObjectKnownType((GraphQLInputType)type);
            }

            if (type is GraphQLComplexType || type is GraphQLScalarType)
            {
                this.AddOutputObjectKnownType(type);
            }
        }

        public GraphQLComplexType[] GetImplementingInterfaces(GraphQLComplexType type)
        {
            var systemType = ReflectionUtilities.GetGenericArgumentsEagerly(type.GetType());
            var interfacesTypes = ReflectionUtilities.GetAllImplementingInterfaces(systemType);

            return interfacesTypes.Select(e => this.GetSchemaTypeFor(e))
                .Select(e => e as GraphQLComplexType)
                .Where(e => e != null)
                .ToArray();
        }

        public IEnumerable<GraphQLInputType> GetInputKnownTypes()
        {
            return this.inputBindings.Select(e => e.Value).Distinct().ToList();
        }

        public IEnumerable<GraphQLBaseType> GetOutputKnownTypes()
        {
            return this.outputBindings.Select(e => e.Value).Distinct().ToList();
        }

        public GraphQLInputType GetSchemaInputTypeFor(Type type)
        {
            if (ReflectionUtilities.IsCollection(type))
                return new GraphQLList(this.GetSchemaInputTypeFor(ReflectionUtilities.GetCollectionMemberType(type)));

            if (this.inputBindings.ContainsKey(type))
                return this.inputBindings[type];

            throw new GraphQLException($"Unknown input type {type} have you added it to known types?");
        }

        public GraphQLInputType GetSchemaInputTypeByName(string name)
        {
            return this.GetInputKnownTypes()
                .Where(e => e.Name == name)
                .SingleOrDefault();
        }

        public GraphQLBaseType GetSchemaOutputTypeByName(string name)
        {
            return this.GetOutputKnownTypes()
                .Where(e => e.Name == name)
                .SingleOrDefault();
        }

        public GraphQLBaseType GetSchemaTypeFor(Type type)
        {
            if (ReflectionUtilities.IsCollection(type))
                return new GraphQLList(this.GetSchemaTypeFor(ReflectionUtilities.GetCollectionMemberType(type)));

            return this.GetSchemaTypeFor(type, type);
        }

        public Type GetInputSystemTypeFor(GraphQLBaseType type)
        {
            var reflectedType = this.inputBindings
                .Where(e => e.Value == type)
                .Select(e => e.Key)
                .FirstOrDefault();

            return reflectedType;
        }

        public GraphQLComplexType[] GetTypesImplementing(GraphQLInterfaceType objectType)
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

        private void AddInputObjectKnownType(GraphQLInputType type)
        {
            var reflectedType = type.GetType();
            var argument = ReflectionUtilities.GetGenericArgumentsEagerly(reflectedType);

            if (argument == null)
                this.inputBindings.Add(reflectedType, type);
            else
                this.inputBindings.Add(argument, type);
        }

        private void AddOutputObjectKnownType(GraphQLBaseType type)
        {
            var reflectedType = type.GetType();
            var argument = ReflectionUtilities.GetGenericArgumentsEagerly(reflectedType);

            if (argument == null)
                this.outputBindings.Add(reflectedType, type);
            else
                this.outputBindings.Add(argument, type);
        }

        private GraphQLBaseType GetSchemaTypeFor(Type originalType, Type presumedSchemaType)
        {
            if (this.outputBindings.ContainsKey(presumedSchemaType))
                return this.outputBindings[presumedSchemaType];

            presumedSchemaType = presumedSchemaType.GetTypeInfo().BaseType;
            if (presumedSchemaType != null)
                return this.GetSchemaTypeFor(originalType, presumedSchemaType);

            throw new GraphQLException($"Unknown type {originalType} have you added it to known types?");
        }
    }
}