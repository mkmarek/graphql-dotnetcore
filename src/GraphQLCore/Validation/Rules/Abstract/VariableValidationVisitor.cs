namespace GraphQLCore.Validation.Rules.Abstract
{
    using Exceptions;
    using Language;
    using Language.AST;
    using System;
    using Type;
    using Type.Translation;

    public abstract class VariableValidationVisitor : ValidationASTVisitor
    {
        private ISchemaRepository schemaRepository;

        public VariableValidationVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.schemaRepository = schema.SchemaRepository;
        }

        protected virtual GraphQLBaseType GetOutputType(GraphQLType type)
        {
            if (type is GraphQLNamedType)
                return this.GetOutputTypeFromNamedType((GraphQLNamedType)type);

            if (type is Language.AST.GraphQLNonNullType)
                return this.GetOutputType(((Language.AST.GraphQLNonNullType)type).Type);

            if (type is GraphQLListType)
                return this.GetOutputType(((GraphQLListType)type).Type);

            throw new NotImplementedException();
        }

        protected virtual GraphQLBaseType GetOutputTypeFromNamedType(GraphQLNamedType type)
        {
            var inputType = this.schemaRepository.GetSchemaInputTypeByName(type.Name.Value);

            if (inputType != null)
                return null;

            return this.schemaRepository.GetSchemaOutputTypeByName(type.Name.Value);
        }

        protected virtual GraphQLBaseType GetInputType(GraphQLType type)
        {
            if (type is GraphQLNamedType)
                return this.GetInputTypeFromNamedType((GraphQLNamedType)type);

            if (type is Language.AST.GraphQLNonNullType)
            {
                var inputType = this.GetInputType(((Language.AST.GraphQLNonNullType)type).Type);

                if (inputType == null)
                    return null;

                return new GraphQLCore.Type.GraphQLNonNullType(inputType);
            }

            if (type is GraphQLListType)
                return new GraphQLList(this.GetInputType(((GraphQLListType)type).Type));

            throw new NotImplementedException();
        }

        protected virtual GraphQLBaseType GetInputTypeFromNamedType(GraphQLNamedType type)
        {
            return this.schemaRepository.GetSchemaInputTypeByName(type.Name.Value);
        }
    }
}
