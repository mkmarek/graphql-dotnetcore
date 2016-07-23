namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using Type;
    using Type.Translation;

    public class VariablesAreInputTypesVisitor : GraphQLAstVisitor
    {
        private ISchemaRepository schemaRepository;

        public VariablesAreInputTypesVisitor(ISchemaRepository schemaRepository) : base()
        {
            this.Errors = new List<GraphQLException>();
            this.schemaRepository = schemaRepository;
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(GraphQLVariableDefinition variableDefinition)
        {
            var variableName = variableDefinition.Variable.Name.Value;
            var outputTupe = this.GetOutputType(variableDefinition.Type);

            if (outputTupe != null)
                this.Errors.Add(new GraphQLException($"Variable \"${variableName}\" cannot be non-input type \"{variableDefinition.Type}\"."));

            return variableDefinition;
        }

        private GraphQLBaseType GetOutputType(GraphQLType type)
        {
            if (type is GraphQLNamedType)
                return this.GetOutputTypeFromNamedType((GraphQLNamedType)type);

            if (type is Language.AST.GraphQLNonNullType)
                return this.GetOutputType(((Language.AST.GraphQLNonNullType)type).Type);

            if (type is GraphQLListType)
                return this.GetOutputType(((GraphQLListType)type).Type);

            throw new NotImplementedException();
        }

        private GraphQLBaseType GetOutputTypeFromNamedType(GraphQLNamedType type)
        {
            var inputType = this.schemaRepository.GetSchemaInputTypeByName(type.Name.Value);

            if (inputType != null)
                return null;

            return this.schemaRepository.GetSchemaOutputTypeByName(type.Name.Value);
        }
    }
}