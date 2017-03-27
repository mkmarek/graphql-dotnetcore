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
                if (this.type is GraphQLUnionType)
                    return null;

                return this.type.GetFieldsInfo()
                    .Select(field => new IntrospectedField()
                    {
                        Name = field.Name,
                        Arguments = field.Arguments?.Select(argument => new IntrospectedInputValue()
                        {
                            Name = argument.Key,
                            Type = argument.Value.GetGraphQLType(this.schemaRepository)
                                .Introspect(this.schemaRepository)
                        }).ToArray(),
                        Type = field.GetGraphQLType(this.schemaRepository)
                            .Introspect(this.schemaRepository)
                    }).ToArray();
            }
        }

        public override IntrospectedType[] Interfaces
        {
            get
            {
                if (this.type is GraphQLUnionType)
                    return null;

                return this.schemaRepository.GetImplementingInterfaces(this.type)
                    .Select(e => e.Introspect(this.schemaRepository))
                    .ToArray();
            }
        }

        public override IntrospectedType[] PossibleTypes
        {
            get
            {
                if (this.type is GraphQLInterfaceType)
                {
                    return this.schemaRepository.GetTypesImplementing((GraphQLInterfaceType)this.type)
                        .Select(e => e.Introspect(this.schemaRepository))
                        .ToArray();
                }

                if (this.type is GraphQLUnionType)
                {
                    return this.schemaRepository.GetPossibleTypesForUnion((GraphQLUnionType)this.type)
                        .Select(e => e.Introspect(this.schemaRepository))
                        .ToArray();
                }

                return new IntrospectedType[] { };
            }
        }
    }
}