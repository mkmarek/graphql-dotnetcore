namespace GraphQLCore.Type
{
    using Complex;
    using Introspection;
    using System.Linq;
    using Translation;

    public class IntrospectedInputObject : IntrospectedType
    {
        private ISchemaRepository schemaRepository;
        private GraphQLInputObjectType type;

        public IntrospectedInputObject(ISchemaRepository schemaRepository, GraphQLInputObjectType type)
        {
            this.type = type;
            this.schemaRepository = schemaRepository;
        }

        public override IntrospectedInputValue[] InputFields
        {
            get
            {
                return this.type.GetFieldsInfo()
                    .Select(this.GetIntrospectedFieldInputValue)
                    .ToArray();
            }
        }

        private IntrospectedInputValue GetIntrospectedFieldInputValue(GraphQLInputObjectTypeFieldInfo field)
        {
            var type = field.GetGraphQLType(this.schemaRepository);

            return new IntrospectedInputValue()
            {
                Name = field.Name,
                Type = type.Introspect(this.schemaRepository),
                Description = field.Description,
                DefaultValue = field.DefaultValue.GetSerialized((GraphQLInputType)type, this.schemaRepository)
            };
        }
    }
}