namespace GraphQLCore.Type
{
    using Introspection;
    using Language.AST;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Translation;
    using Utils;

    public class GraphQLList : GraphQLInputType
    {
        public override bool IsLeafType
        {
            get
            {
                return this.MemberType.IsLeafType;
            }
        }

        public GraphQLList(GraphQLBaseType memberType) : base(null, null)
        {
            this.MemberType = memberType;
        }

        public GraphQLBaseType MemberType { get; private set; }

        public override object GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (!(this.MemberType is GraphQLInputType))
                return null;

            var inputType = this.MemberType as GraphQLInputType;
            var singleValue = inputType.GetFromAst(astValue, schemaRepository);

            IList output = (IList)Activator.CreateInstance(
                ReflectionUtilities.CreateListTypeOf(
                    schemaRepository.GetInputSystemTypeFor(inputType)));

            if (!ReflectionUtilities.IsNullOrEmptyCollection(singleValue))
            {
                output.Add(singleValue);
                return output;
            }

            var list = ((GraphQLListValue)astValue).Values;

            foreach (var item in list)
                output.Add(inputType.GetFromAst(item, schemaRepository));

            return output;
        }

        public override IntrospectedType Introspect(ISchemaRepository schemaRepository)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Name = this.Name;
            introspectedType.Description = this.Description;
            introspectedType.Kind = TypeKind.LIST;
            introspectedType.OfType = this.MemberType.Introspect(schemaRepository);

            return introspectedType;
        }

        public override string ToString()
        {
            return $"[{this.MemberType}]";
        }
    }
}