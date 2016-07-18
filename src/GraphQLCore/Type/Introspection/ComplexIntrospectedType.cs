using GraphQLCore.Type.Translation;
using System.Linq;

namespace GraphQLCore.Type.Introspection
{
    public class ComplexIntrospectedType : IntrospectedType
    {
        private ISchemaRepository schemaRepository;
        private GraphQLComplexType type;

        internal ComplexIntrospectedType(ISchemaRepository schemaRepository, GraphQLComplexType type)
        {
            this.schemaRepository = schemaRepository;
            this.type = type;
        }

        public override IntrospectedField[] Fields
        {
            get
            {
                return this.type.GetFieldsInfo()
                    .Select(field => new IntrospectedField()
                    {
                        Name = field.Name,
                        Arguments = field.Arguments?.Select(argument => new IntrospectedInputValue()
                        {
                            Name = argument.Key,
                            Type = this.GetInputTypeFrom(argument.Value.Type, this.schemaRepository)
                                .Introspect(this.schemaRepository)
                        }).ToArray(),
                        Type = this.GetOutputTypeFrom(field.SystemType, this.schemaRepository)
                            .Introspect(this.schemaRepository)
                    }).ToArray();
            }
        }

        public override IntrospectedType[] Interfaces
        {
            get
            {
                return this.schemaRepository.GetImplementingInterfaces(this.type)
                    .Select(e => e.Introspect(this.schemaRepository))
                    .ToArray();
            }
        }

        public override IntrospectedType[] PossibleTypes
        {
            get
            {
                if (!(this.type is GraphQLInterfaceType))
                    return null;

                return this.schemaRepository.GetTypesImplementing((GraphQLInterfaceType)this.type)
                    .Select(e => e.Introspect(this.schemaRepository))
                    .ToArray();
            }
        }
    }
}