namespace GraphQLCore.Type
{
    using Language.AST;
    using System;
    using Translation;
    using Utils;

    public struct DefaultValue
    {
        public object Value { get; }
        public bool IsSet { get; }

        public DefaultValue(object value, Type systemType)
        {
            var result = ReflectionUtilities.ChangeValueTypeWithResult(value, systemType);

            this.Value = result.Value;
            this.IsSet = result.IsValid;
        }

        public GraphQLValue GetAstValue(GraphQLInputType type, ISchemaRepository schemaRepository)
        {
            if (!this.IsSet)
                return null;

            return type.GetAstFromValue(this.Value, schemaRepository);
        }

        public string GetSerialized(GraphQLInputType type, ISchemaRepository schemaRepository)
        {
            return this.GetAstValue(type, schemaRepository)?.ToString();
        }
    }
}
