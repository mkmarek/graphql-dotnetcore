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

        public static IList CreateOutputList(GraphQLInputType inputType, ISchemaRepository schemaRepository)
        {
            IList output = (IList)Activator.CreateInstance(
                ReflectionUtilities.CreateListTypeOf(
                    schemaRepository.GetInputSystemTypeFor(inputType)));

            return output;
        }

        public override Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind == ASTNodeKind.NullValue)
                return new Result(null);

            var inputType = this.MemberType as GraphQLInputType;
            var output = CreateOutputList(inputType, schemaRepository);

            if (astValue.Kind != ASTNodeKind.ListValue)
            {
                var value = inputType.GetFromAst(astValue, schemaRepository);

                if (!value.IsValid)
                    return Result.Invalid;

                output.Add(value.Value);

                return new Result(output);
            }

            var list = ((GraphQLListValue)astValue).Values;

            foreach (var item in list)
            {
                var result = inputType.GetFromAst(item, schemaRepository);

                if (!result.IsValid)
                    return Result.Invalid;
                
                output.Add(result.Value);
            }

            return new Result(output);
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