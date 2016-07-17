namespace GraphQLCore.Type
{
    using Introspection;
    using Language.AST;
    using System.Collections;
    using System.Collections.Generic;
    using Translation;

    public class GraphQLList : GraphQLInputType
    {
        public GraphQLList(GraphQLBaseType memberType) : base(null, null)
        {
            this.MemberType = memberType;
        }

        public GraphQLBaseType MemberType { get; private set; }

        public override object GetFromAst(GraphQLValue astValue)
        {
            if (!(this.MemberType is GraphQLInputType))
                return null;

            var inputType = this.MemberType as GraphQLInputType;
            var singleValue = inputType.GetFromAst(astValue);

            if (singleValue != null)
                return singleValue;

            IList output = new List<object>();
            var list = ((GraphQLListValue)astValue).Values;

            foreach (var item in list)
                output.Add(inputType.GetFromAst(item));

            return output;
        }

        public override IntrospectedType Introspect(ISchemaObserver schemaObserver)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Name = this.Name;
            introspectedType.Description = this.Description;
            introspectedType.Kind = TypeKind.LIST;
            introspectedType.OfType = this.MemberType.Introspect(schemaObserver);

            return introspectedType;
        }
    }
}