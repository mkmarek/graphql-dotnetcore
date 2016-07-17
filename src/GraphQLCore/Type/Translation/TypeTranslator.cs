namespace GraphQLCore.Type.Translation
{
    using Scalar;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Utils;

    public class TypeTranslator : ITypeTranslator
    {
        private Dictionary<Type, GraphQLBaseType> bindings;
        private ISchemaObserver schemaObserver;

        public TypeTranslator(ISchemaObserver schemaObserver)
        {
            this.bindings = new Dictionary<Type, GraphQLBaseType>();
            this.schemaObserver = schemaObserver;
            this.RegisterBindings();
            this.RegisterScalarsToSchemeObserver();
        }

        public object CreateObjectFromDynamic(GraphQLObjectType inputObjectType, ExpandoObject inputObject)
        {
            var systemType = this.GetType(inputObjectType);
            var fields = GetAccessorsFromType(inputObjectType);
            var inputObjectDictionary = (IDictionary<string, object>)inputObject;

            var resultObject = Activator.CreateInstance(systemType);

            foreach (var field in fields)
            {
                if (!inputObjectDictionary.ContainsKey(field.Name))
                    continue;

                this.AssignValueToField(inputObjectDictionary[field.Name], resultObject, field.Lambda);
            }

            return resultObject;
        }

        public GraphQLBaseType GetType(Type type)
        {
            if (ReflectionUtilities.IsCollection(type))
                return new GraphQLList(this.GetType(ReflectionUtilities.GetCollectionMemberType(type)));

            if (this.bindings.ContainsKey(type))
                return this.bindings[type];

            return this.GetSchemaType(type);
        }

        public Type GetType(GraphQLBaseType type)
        {
            if (type is GraphQLList)
                return ReflectionUtilities.CreateListTypeOf(this.GetType(((GraphQLList)type).MemberType));

            var reflectedType = this.bindings
                .Where(e => IsMatchingValueType(type, e.Value))
                .Select(e => e.Key)
                .SingleOrDefault();

            if (reflectedType == null)
            {
                if (type is GraphQLNonNullType)
                    reflectedType = this.schemaObserver.GetTypeFor(((GraphQLNonNullType)type).UnderlyingNullableType);
                else
                    reflectedType = this.schemaObserver.GetTypeFor(type);
            }

            return reflectedType;
        }

        public GraphQLBaseType GetType(Language.AST.GraphQLNamedType type)
        {
            return this.schemaObserver.GetOutputKnownTypes()
                .Single(e => e.Name == type.Name.Value);
        }

        public object TranslatePerDefinition(object inputObject, GraphQLBaseType typeDefinition)
        {
            if (inputObject is ExpandoObject && typeDefinition is GraphQLObjectType)
                return this.CreateObjectFromDynamic((GraphQLObjectType)typeDefinition, (ExpandoObject)inputObject);

            var systemType = this.GetType(typeDefinition);
            return ReflectionUtilities.ChangeValueType(inputObject, systemType);
        }

        public object TranslatePerDefinition(object inputObject, Type type)
        {
            var typeDefinition = this.GetType(type);

            if (inputObject is ExpandoObject && typeDefinition is GraphQLObjectType)
                return this.CreateObjectFromDynamic((GraphQLObjectType)typeDefinition, (ExpandoObject)inputObject);

            return ReflectionUtilities.ChangeValueType(inputObject, type);
        }

        private static List<GraphQLObjectTypeFieldInfo> GetAccessorsFromType(GraphQLObjectType inputObjectType)
        {
            return inputObjectType.GetFieldsInfo()
                .Where(e => e.IsResolver == false)
                .ToList();
        }

        private static GraphQLBaseType GetUnderlyingNullableType(GraphQLBaseType type)
        {
            return ((GraphQLNonNullType)type).UnderlyingNullableType;
        }

        private static bool IsMatchingValueType(GraphQLBaseType type, GraphQLBaseType bindingType)
        {
            if (type is GraphQLNonNullType && bindingType is GraphQLNonNullType)
                return IsMatchingValueType(GetUnderlyingNullableType(type), GetUnderlyingNullableType(bindingType));

            if (type.GetType() != bindingType.GetType())
                return false;

            return type.Name == bindingType.Name;
        }

        private void AssignValueToField(object value, object resultObject, LambdaExpression expression)
        {
            var variableProp = this.TranslatePerDefinition(
                value,
                ReflectionUtilities.GetReturnValueFromLambdaExpression(expression));

            this.MakeSetterFromLambda(expression).DynamicInvoke(resultObject, variableProp);
        }

        private GraphQLBaseType GetSchemaType(Type type)
        {
            if (this.IsNullable(type))
                return this.schemaObserver.GetSchemaTypeFor(Nullable.GetUnderlyingType(type));

            if (this.IsValueType(type))
                return new GraphQLNonNullType(this.schemaObserver.GetSchemaTypeFor(type));

            return this.schemaObserver.GetSchemaTypeFor(type);
        }

        private bool IsNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        private bool IsValueType(Type type)
        {
            return
                ReflectionUtilities.IsStruct(type) ||
                ReflectionUtilities.IsEnum(type);
        }

        private Delegate MakeSetterFromLambda(LambdaExpression lambda)
        {
            var member = (MemberExpression)lambda.Body;
            var param = Expression.Parameter(member.Type, "value");
            var setter = Expression.Lambda(Expression.Assign(member, param), lambda.Parameters[0], param);

            return setter.Compile();
        }

        private void RegisterBinding<T>(GraphQLBaseType type)
        {
            this.bindings.Add(typeof(T), type);
        }

        private void RegisterBindings()
        {
            this.RegisterBinding<int?>(new GraphQLInt());
            this.RegisterBinding<float?>(new GraphQLFloat());
            this.RegisterBinding<bool?>(new GraphQLBoolean());
            this.RegisterBinding<string>(new GraphQLString());

            this.RegisterBinding<int>(new GraphQLNonNullType(new GraphQLInt()));
            this.RegisterBinding<float>(new GraphQLNonNullType(new GraphQLFloat()));
            this.RegisterBinding<bool>(new GraphQLNonNullType(new GraphQLBoolean()));
        }

        private void RegisterScalarsToSchemeObserver()
        {
        }
    }
}