using GraphQLCore.Execution;

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

        public IVariableResolver VariableResolver { get; set; }

        public SchemaRepository()
        {
            this.outputBindings = new Dictionary<Type, GraphQLBaseType>();
            this.inputBindings = new Dictionary<Type, GraphQLInputType>();

            var graphQLInt = new GraphQLInt();
            var graphQLLong = new GraphQLLong();
            var graphQLFloat = new GraphQLFloat();
            var graphQLBoolean = new GraphQLBoolean();
            var graphQLString = new GraphQLString();

            this.outputBindings.Add(typeof(string), graphQLString);
            this.inputBindings.Add(typeof(string), graphQLString);

            this.outputBindings.Add(typeof(int), graphQLInt);
            this.outputBindings.Add(typeof(long), graphQLLong);
            this.outputBindings.Add(typeof(float), graphQLFloat);
            this.outputBindings.Add(typeof(bool), graphQLBoolean);

            this.inputBindings.Add(typeof(int), graphQLInt);
            this.inputBindings.Add(typeof(long), graphQLLong);
            this.inputBindings.Add(typeof(float), graphQLFloat);
            this.inputBindings.Add(typeof(bool), graphQLBoolean);

            this.outputBindings.Add(typeof(int?), graphQLInt);
            this.outputBindings.Add(typeof(long?), graphQLLong);
            this.outputBindings.Add(typeof(float?), graphQLFloat);
            this.outputBindings.Add(typeof(bool?), graphQLBoolean);

            this.inputBindings.Add(typeof(int?), graphQLInt);
            this.inputBindings.Add(typeof(long?), graphQLLong);
            this.inputBindings.Add(typeof(float?), graphQLFloat);
            this.inputBindings.Add(typeof(bool?), graphQLBoolean);
        }


        public void AddKnownType(GraphQLBaseType type)
        {
            if (type is GraphQLInputType)
                this.AddInputType((GraphQLInputType)type);

            if (!(type is GraphQLInputType) || type is GraphQLScalarType)
                this.AddOutputType(type);
        }

        public GraphQLComplexType[] GetImplementingInterfaces(GraphQLComplexType type)
        {
            var interfacesTypes = ReflectionUtilities.GetAllImplementingInterfaces(type.SystemType);

            return interfacesTypes.Select(e => this.GetSchemaTypeForWithNoError(e))
                .Select(e => e as GraphQLComplexType)
                .Where(e => e != null)
                .ToArray();
        }

        public IEnumerable<GraphQLInputType> GetInputKnownTypes()
        {
            return this.inputBindings.Select(e => e.Value).Distinct().ToList();
        }

        public Type GetInputSystemTypeFor(GraphQLBaseType type)
        {
            var reflectedType = this.inputBindings
                .Where(e => e.Value == type)
                .Select(e => e.Key)
                .FirstOrDefault();

            return reflectedType;
        }

        public IEnumerable<GraphQLComplexType> GetOutputKnownComplexTypes()
        {
            return this.GetOutputKnownTypes()
                .Where(e => e is GraphQLComplexType)
                .Cast<GraphQLComplexType>()
                .ToArray();
        }

        public IEnumerable<GraphQLBaseType> GetOutputKnownTypes()
        {
            return this.outputBindings.Select(e => e.Value).Distinct().ToList();
        }

        public GraphQLInputType GetSchemaInputTypeByName(string name)
        {
            return this.GetInputKnownTypes()
                .Where(e => e.Name == name)
                .SingleOrDefault();
        }

        public GraphQLInputType GetSchemaInputTypeFor(Type type)
        {
            if (ReflectionUtilities.IsCollection(type))
                return new GraphQLList(this.GetSchemaInputTypeFor(ReflectionUtilities.GetCollectionMemberType(type)));

            if (this.inputBindings.ContainsKey(type))
                return this.inputBindings[type];

            throw new GraphQLException($"Unknown input type {type} have you added it to known types?");
        }

        public GraphQLBaseType GetSchemaOutputTypeByName(string name)
        {
            return this.GetOutputKnownTypes()
                .Where(e => e.Name == name)
                .SingleOrDefault();
        }

        public GraphQLBaseType GetSchemaTypeFor(Type type)
        {
            var schemaType = this.GetSchemaTypeForWithNoError(type);

            if (schemaType == null)
                throw new GraphQLException($"Unknown type {type} have you added it to known types?");

            return schemaType;
        }

        public GraphQLComplexType[] GetTypesImplementing(GraphQLInterfaceType objectType)
        {
            return this.GetOutputKnownComplexTypes()
                    .Where(e => ReflectionUtilities.GetAllImplementingInterfaces(e.SystemType)
                        .Contains(objectType.SystemType))
                    .Select(e => e as GraphQLComplexType)
                    .ToArray();
        }

        private void AddInputType(GraphQLInputType type)
        {
            if (type is ISystemTypeBound)
                this.inputBindings.Add(((ISystemTypeBound)type).SystemType, type);
            else
                this.inputBindings.Add(type.GetType(), type);
        }

        private void AddOutputType(GraphQLBaseType type)
        {
            if (type is ISystemTypeBound)
                this.outputBindings.Add(((ISystemTypeBound)type).SystemType, type);
            else
                this.outputBindings.Add(type.GetType(), type);
        }

        private GraphQLBaseType GetSchemaTypeFor(Type originalType, Type presumedSchemaType)
        {
            if (this.outputBindings.ContainsKey(presumedSchemaType))
                return this.outputBindings[presumedSchemaType];

            presumedSchemaType = presumedSchemaType.GetTypeInfo().BaseType;
            if (presumedSchemaType != null)
                return this.GetSchemaTypeFor(originalType, presumedSchemaType);

            return null;
        }

        private GraphQLBaseType GetSchemaTypeForWithNoError(Type type)
        {
            if (ReflectionUtilities.IsCollection(type))
                return new GraphQLList(this.GetSchemaTypeFor(ReflectionUtilities.GetCollectionMemberType(type)));

            return this.GetSchemaTypeFor(type, type);
        }
    }
}