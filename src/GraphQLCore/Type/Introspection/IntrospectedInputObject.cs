namespace GraphQLCore.Type
{
    using Introspection;
    using System.Linq;
    using Translation;

    public class IntrospectedInputObject : IntrospectedType
    {
        private ISchemaObserver schemaObserver;
        private GraphQLInputObjectType type;

        public IntrospectedInputObject(ISchemaObserver schemaObserver, GraphQLInputObjectType type)
        {
            this.type = type;
            this.schemaObserver = schemaObserver;
        }

        public override IntrospectedInputValue[] InputFields
        {
            get
            {
                return this.type.GetFieldsInfo()
                    .Select(field => new IntrospectedInputValue()
                    {
                        Name = field.Name,
                        Type = this.GetInputTypeFrom(field.SystemType, this.schemaObserver)
                            .Introspect(this.schemaObserver)
                    }).ToArray();
            }
        }
    }
}