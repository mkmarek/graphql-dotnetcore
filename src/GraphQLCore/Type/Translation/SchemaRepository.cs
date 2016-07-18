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
        private Dictionary<string, GraphQLInputType> inputBindings;

        private Dictionary<string, GraphQLBaseType> outputBindings;

        public SchemaRepository()
        {
            this.outputBindings = new Dictionary<string, GraphQLBaseType>();
            this.inputBindings = new Dictionary<string, GraphQLInputType>();

            var graphQLInt = new GraphQLInt();
            var graphQLFloat = new GraphQLFloat();
            var graphQLBoolean = new GraphQLBoolean();
            var graphQLString = new GraphQLString();

            this.outputBindings.Add(typeof(string).FullName, graphQLString);
            this.inputBindings.Add(typeof(string).FullName, graphQLString);

            this.outputBindings.Add(typeof(int).FullName, graphQLInt);
            this.outputBindings.Add(typeof(float).FullName, graphQLFloat);
            this.outputBindings.Add(typeof(bool).FullName, graphQLBoolean);

            this.inputBindings.Add(typeof(int).FullName, graphQLInt);
            this.inputBindings.Add(typeof(float).FullName, graphQLFloat);
            this.inputBindings.Add(typeof(bool).FullName, graphQLBoolean);

            this.outputBindings.Add(typeof(int?).FullName, graphQLInt);
            this.outputBindings.Add(typeof(float?).FullName, graphQLFloat);
            this.outputBindings.Add(typeof(bool?).FullName, graphQLBoolean);

            this.inputBindings.Add(typeof(int?).FullName, graphQLInt);
            this.inputBindings.Add(typeof(float?).FullName, graphQLFloat);
            this.inputBindings.Add(typeof(bool?).FullName, graphQLBoolean);
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

            if (this.inputBindings.ContainsKey(type.FullName))
                return this.inputBindings[type.FullName];

            throw new GraphQLException($"Unknown input type {type.FullName} have you added it to known types?");
        }

        public GraphQLInputType GetSchemaInputTypeByName(string name)
        {
            return this.GetInputKnownTypes()
                .Where(e => e.Name == name)
                .SingleOrDefault();
        }

        public GraphQLBaseType GetSchemaTypeFor(Type type)
        {
            if (ReflectionUtilities.IsCollection(type))
                return new GraphQLList(this.GetSchemaTypeFor(ReflectionUtilities.GetCollectionMemberType(type)));

            return this.GetSchemaTypeFor(type, type);
        }

        public Type GetSystemTypeFor(GraphQLBaseType type)
        {
            return ReflectionUtilities.GetGenericArgumentsEagerly(type.GetType());
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
                this.inputBindings.Add(reflectedType.FullName, type);
            else
                this.inputBindings.Add(argument.FullName, type);
        }

        private void AddOutputObjectKnownType(GraphQLBaseType type)
        {
            var reflectedType = type.GetType();
            var argument = ReflectionUtilities.GetGenericArgumentsEagerly(reflectedType);

            if (argument == null)
                this.outputBindings.Add(reflectedType.FullName, type);
            else
                this.outputBindings.Add(argument.FullName, type);
        }

        private GraphQLBaseType GetSchemaTypeFor(Type originalType, Type presumedSchemaType)
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