namespace GraphQLCore.Type
{
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
                    .Select(field => new IntrospectedInputValue()
                    {
                        Name = field.Name,
                        Type = this.GetInputTypeFrom(field.SystemType, this.schemaRepository)
                            .Introspect(this.schemaRepository)
                    }).ToArray();
            }
        }
    }
}