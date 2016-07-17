using GraphQLCore.Type.Translation;
using System.Linq;

namespace GraphQLCore.Type.Introspection
{
    public class ComplexIntrospectedType : IntrospectedType
    {
        private ISchemaObserver schemaObserver;
        private GraphQLComplexType type;

        internal ComplexIntrospectedType(ISchemaObserver schemaObjerver, GraphQLComplexType type)
        {
            this.schemaObserver = schemaObjerver;
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
                            Type = this.GetInputTypeFrom(argument.Value.Type, this.schemaObserver)
                                .Introspect(this.schemaObserver)
                        }).ToArray(),
                        Type = this.GetOutputTypeFrom(field.SystemType, this.schemaObserver)
                            .Introspect(this.schemaObserver)
                    }).ToArray();
            }
        }

        public override IntrospectedType[] Interfaces
        {
            get
            {
                return this.schemaObserver.GetImplementingInterfaces(this.type)
                    .Select(e => e.Introspect(this.schemaObserver))
                    .ToArray();
            }
        }

        public override IntrospectedType[] PossibleTypes
        {
            get
            {
                return this.schemaObserver.GetTypesImplementing(this.type)
                    .Select(e => e.Introspect(this.schemaObserver))
                    .ToArray();
            }
        }
    }
}