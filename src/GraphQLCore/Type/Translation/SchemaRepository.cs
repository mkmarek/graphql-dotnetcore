namespace GraphQLCore.Type.Translation
{
    using Exceptions;
    using Execution;
    using GraphQLCore.Type.Directives;
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

        private Dictionary<string, GraphQLDirectiveType> directives;

        public IVariableResolver VariableResolver { get; set; }

        public SchemaRepository()
        {
            this.outputBindings = new Dictionary<Type, GraphQLBaseType>();
            this.inputBindings = new Dictionary<Type, GraphQLInputType>();
            this.directives = new Dictionary<string, GraphQLDirectiveType>();

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
        }

        public void AddOrReplaceDirective(GraphQLDirectiveType directive)
        {
            if (this.directives.ContainsKey(directive.Name))
                this.directives.Remove(directive.Name);

            this.directives.Add(directive.Name, directive);
        }

        public GraphQLDirectiveType GetDirective(string name)
        {
            if (this.directives.ContainsKey(name))
                return this.directives[name];

            return null;
        }

        public IEnumerable<GraphQLDirectiveType> GetDirectives()
        {
            return this.directives.Select(e => e.Value)
                .ToList();
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
            if (type is GraphQLNonNullType)
                return this.GetNonNullInputSystemTypeFor(((GraphQLNonNullType)type).UnderlyingNullableType);

            var inputType = ReflectionUtilities.CreateNullableType(
                this.GetNonNullInputSystemTypeFor(type));

            return inputType;
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
                return this.CreateInputList(type);

            var inputType = this.GetSchemaInputTypeForType(type);

            if (ReflectionUtilities.IsValueType(type) && !ReflectionUtilities.IsNullable(type))
                return new GraphQLNonNullType(inputType);

            return inputType;
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

        public GraphQLList CreateList(Type arrayType)
        {
            var memberType = ReflectionUtilities.GetCollectionMemberType(arrayType);
            var schemaType = this.GetSchemaTypeFor(memberType);
            var list = new GraphQLList(schemaType);

            return list;
        }

        public GraphQLList CreateInputList(Type arrayType)
        {
            var memberType = ReflectionUtilities.GetCollectionMemberType(arrayType);
            var schemaType = this.GetSchemaInputTypeFor(memberType);
            var list = new GraphQLList(schemaType);

            return list;
        }

        private GraphQLInputType GetSchemaInputTypeForType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
                type = underlyingType;

            if (this.inputBindings.ContainsKey(type))
                return this.inputBindings[type];

            throw new GraphQLException($"Unknown input type {type} have you added it to known types?");
        }

        private Type GetNonNullInputSystemTypeFor(GraphQLBaseType type)
        {
            if (type is GraphQLList)
            {
                return ReflectionUtilities.CreateListTypeOf(
                    this.GetInputSystemTypeFor(((GraphQLList)type).MemberType));
            }

            var reflectedType = this.inputBindings
                .Where(e => e.Value.Name == type.Name)
                .Select(e => e.Key)
                .FirstOrDefault();

            return reflectedType;
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
                return this.CreateList(type);

            var underlyingNullableType = Nullable.GetUnderlyingType(type);
            if (underlyingNullableType != null)
                return this.GetSchemaTypeFor(underlyingNullableType, underlyingNullableType);

            if (ReflectionUtilities.IsValueType(type))
                return new GraphQLNonNullType(this.GetSchemaTypeFor(type, type));

            return this.GetSchemaTypeFor(type, type);
        }
    }
}
