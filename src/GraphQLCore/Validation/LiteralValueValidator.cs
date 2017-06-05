namespace GraphQLCore.Validation
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Translation;

    public class LiteralValueValidator
    {
        private ISchemaRepository schemaRepository;

        public LiteralValueValidator(ISchemaRepository schemaRepository)
        {
            this.schemaRepository = schemaRepository;
        }

        internal IEnumerable<GraphQLException> IsValid(GraphQLBaseType type, GraphQLValue astValue)
        {
            if (astValue is GraphQLVariable)
                return new GraphQLException[] { };

            if (astValue is GraphQLNullValue || astValue == null)
                return this.ValidateNullType(type, astValue);

            if (type is GraphQLNonNull)
                return this.IsValid(((GraphQLNonNull)type).UnderlyingNullableType, astValue);

            if (type is GraphQLList)
                return this.ValidateListType(type, astValue);

            if (type is GraphQLInputType)
                return this.ValidateInputType(type, astValue);

            return new GraphQLException[] { };
        }

        private IEnumerable<GraphQLException> ValidateInputType(GraphQLBaseType type, GraphQLValue astValue)
        {
            var result = ((GraphQLInputType)type).GetFromAst(astValue, this.schemaRepository);

            if (type is GraphQLInputObjectType)
                return this.ValidateObjectFields((GraphQLInputObjectType)type, (GraphQLObjectValue)astValue);

            if (!result.IsValid)
            {
                return new GraphQLException[]
                {
                    new GraphQLException($"Expected type \"{type}\", found {astValue}.")
                };
            }

            return new GraphQLException[] { };
        }

        private IEnumerable<GraphQLException> ValidateObjectFields(GraphQLInputObjectType objectType, GraphQLObjectValue objectValue)
        {
            foreach (var fieldInfo in objectType.GetFieldsInfo())
            {
                var field = objectValue.Fields.SingleOrDefault(e => e.Name.Value == fieldInfo.Name);
                var fieldType = fieldInfo.GetGraphQLType(this.schemaRepository);
                var fieldValue = field?.Value;

                var fieldErrors = this.IsValid(fieldType, fieldValue);

                foreach (var fieldError in fieldErrors)
                    yield return new GraphQLException($"In field \"{fieldInfo.Name}\": {fieldError.Message}");
            }
        }

        private IEnumerable<GraphQLException> ValidateListMembers(GraphQLBaseType itemType, GraphQLListValue astValue)
        {
            var values = astValue.Values;

            for (int i = 0; i < values.Count(); i++)
            {
                foreach (var error in this.IsValid(itemType, values.ElementAt(i)))
                    yield return new GraphQLException($"In element #{i}: {error.Message}");
            }
        }

        private IEnumerable<GraphQLException> ValidateListType(GraphQLBaseType type, GraphQLValue astValue)
        {
            var itemType = ((GraphQLList)type).MemberType;

            if (astValue.Kind == ASTNodeKind.ListValue)
                return this.ValidateListMembers(itemType, (GraphQLListValue)astValue);

            return this.IsValid(itemType, astValue);
        }

        private IEnumerable<GraphQLException> ValidateNullType(GraphQLBaseType type, GraphQLValue astValue)
        {
            if (type is GraphQLNonNull)
            {
                return new GraphQLException[]
                {
                    new GraphQLException($"Expected type \"{type}\", found null.")
                };
            }

            return new GraphQLException[] { };
        }
    }
}
