namespace GraphQLCore.Type.Introspection
{
    using Complex;
    using System.Collections.Generic;
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
                        Arguments = field.Arguments?.Select(this.GetInputValueFromArgument).ToArray(),
                        Type = field.GetGraphQLType(this.schemaRepository)
                            .Introspect(this.schemaRepository),
                        Description = field.Description,
                        IsDeprecated = field.IsDeprecated,
                        DeprecationReason = field.DeprecationReason
                    }).ToArray();
            }
        }

        private IntrospectedInputValue GetInputValueFromArgument(KeyValuePair<string, GraphQLObjectTypeArgumentInfo> argument)
        {
            var type = argument.Value.GetGraphQLType(this.schemaRepository);

            return new IntrospectedInputValue()
            {
                Name = argument.Key,
                Type = type.Introspect(this.schemaRepository),
                Description = argument.Value.Description,
                DefaultValue = argument.Value.DefaultValue.GetSerialized((GraphQLInputType)type, this.schemaRepository)
            };
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