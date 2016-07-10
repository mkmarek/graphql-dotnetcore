namespace GraphQLCore.Type
{
    using GraphQLCore.Type.Translation;
    using System.Collections.Generic;
    using System.Linq;
    using Utils;

    public class ObjectTypeTranslator : IObjectTypeTranslator
    {
        private GraphQLNullableType objectType;
        private ISchemaObserver schemaObserver;
        private ITypeTranslator typeTranslator;

        public ObjectTypeTranslator(GraphQLNullableType objectType, ITypeTranslator typeTranslator, ISchemaObserver schemaObserver)
        {
            this.objectType = objectType;
            this.typeTranslator = typeTranslator;
            this.schemaObserver = schemaObserver;
        }

        public GraphQLFieldConfig GetField(string fieldName)
        {
            if (this.objectType is GraphQLObjectType)
                return this.GetFieldFromObject((GraphQLObjectType)this.objectType, fieldName);

            return null;
        }

        public GraphQLFieldConfig[] GetFields()
        {
            if (this.objectType is GraphQLInputObjectType)
                return this.GetInputFieldsFromObject((GraphQLComplexType)this.objectType);

            if (this.objectType is GraphQLComplexType)
                return this.GetFieldsFromObject((GraphQLComplexType)this.objectType);

            return null;
        }

        public GraphQLComplexType[] GetImplementingInterfaces()
        {
            var type = ReflectionUtilities.GetGenericArgumentsEagerly(this.objectType.GetType());
            var interfacesTypes = ReflectionUtilities.GetAllImplementingInterfaces(type);

            return interfacesTypes.Select(e => this.typeTranslator.GetType(e))
                .Select(e => e as GraphQLComplexType)
                .Where(e => e != null)
                .ToArray();
        }

        public GraphQLComplexType[] GetPossibleTypes()
        {
            return this.schemaObserver.GetTypesImplementing(this.objectType);
        }

        private GraphQLFieldConfig CreateFieldConfigTypeFromFieldInfo(GraphQLObjectTypeFieldInfo fieldInfo)
        {
            return new GraphQLFieldConfig()
            {
                Name = fieldInfo.Name,
                Type = this.typeTranslator.GetType(fieldInfo.ReturnValueType),
                Arguments = GetArguments(fieldInfo)
            };
        }

        private GraphQLFieldConfig CreateInputFieldConfigTypeFromFieldInfo(GraphQLObjectTypeFieldInfo fieldInfo)
        {
            return new GraphQLFieldConfig()
            {
                Name = fieldInfo.Name,
                Type = this.typeTranslator.GetInputType(fieldInfo.ReturnValueType),
                Arguments = GetArguments(fieldInfo)
            };
        }

        private Dictionary<string, GraphQLScalarType> GetArguments(GraphQLObjectTypeFieldInfo fieldInfo)
        {
            return fieldInfo.Arguments
                .Select(e => new { Name = e.Key, Type = this.typeTranslator.GetInputType(e.Value.Type) })
                .ToDictionary(e => e.Name, e => e.Type);
        }

        private GraphQLFieldConfig GetFieldFromObject(GraphQLObjectType type, string fieldName)
        {
            var fieldInfo = type.GetFieldInfo(fieldName);

            return this.CreateFieldConfigTypeFromFieldInfo(fieldInfo);
        }

        private GraphQLFieldConfig[] GetFieldsFromObject(GraphQLComplexType type)
        {
            var fieldInfos = type.GetFieldsInfo();

            return fieldInfos.Select(e => this.CreateFieldConfigTypeFromFieldInfo(e)).ToArray();
        }

        private GraphQLFieldConfig[] GetInputFieldsFromObject(GraphQLComplexType type)
        {
            var fieldInfos = type.GetFieldsInfo();

            return fieldInfos.Select(e => this.CreateInputFieldConfigTypeFromFieldInfo(e)).ToArray();
        }
    }
}