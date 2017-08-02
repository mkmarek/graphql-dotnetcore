namespace GraphQLCore.Type
{
    using Complex;
    using Introspection;
    using System;
    using Translation;

    public class GraphQLEnumValueInfo : GraphQLFieldInfo
    {
        public override Type SystemType { get; set; }

        public NonNullable<IntrospectedEnumValue> Introspect()
        {
            return new IntrospectedEnumValue()
            {
                Name = this.Name,
                Description = this.Description,
                IsDeprecated = this.IsDeprecated,
                DeprecationReason = this.DeprecationReason
            };
        }

        protected override GraphQLBaseType GetSchemaType(Type type, ISchemaRepository schemaRepository)
        {
            return this.GetGraphQLType(schemaRepository);
        }
    }
}
