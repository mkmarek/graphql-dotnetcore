namespace GraphQLCore.Type.Introspection
{
    using System.Linq;
    using Translation;

    public class ComplexIntrospectedType : IntrospectedType
    {
        private ISchemaRepository schemaRepository;
        private GraphQLComplexType type;

        internal ComplexIntrospectedType(ISchemaRepository schemaRepository, GraphQLComplexType type)
        {
            this.schemaRepository = schemaRepository;
            this.type = type;
        }

        public override NonNullable<IntrospectedField>[] Fields
        {
            get
            {
                if (this.type is GraphQLUnionType)
                    return null;

                return this.type.GetFieldsInfo()
                    .Select(field => (NonNullable<IntrospectedField>)new IntrospectedField()
                    {
                        Name = field.Name,
                        Arguments = field.Arguments?.Select(e => e.Value.Introspect(this.schemaRepository)).ToArray(),
                        Type = field.GetGraphQLType(this.schemaRepository).Introspect(this.schemaRepository),
                        Description = field.Description,
                        IsDeprecated = field.IsDeprecated,
                        DeprecationReason = field.DeprecationReason
                    }).ToArray();
            }
        }

        public override NonNullable<IntrospectedType>[] Interfaces
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

        public override NonNullable<IntrospectedType>[] PossibleTypes
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

                return new NonNullable<IntrospectedType>[] { };
            }
        }
    }
}