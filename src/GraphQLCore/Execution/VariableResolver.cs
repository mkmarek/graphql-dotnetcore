using GraphQLCore.Language.AST;
using GraphQLCore.Type;
using GraphQLCore.Type.Translation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace GraphQLCore.Execution
{
    public class VariableResolver : IVariableResolver
    {
        private ITypeTranslator typeTranslator;
        private IEnumerable<GraphQLVariableDefinition> variableDefinitions;
        private Dictionary<string, object> variables;

        public VariableResolver(dynamic variables, ITypeTranslator typeTranslator, IEnumerable<GraphQLVariableDefinition> variableDefinitions)
        {
            this.variables = ((ExpandoObject)variables).ToDictionary(e => e.Key, e => e.Value);
            this.variableDefinitions = variableDefinitions;
            this.typeTranslator = typeTranslator;
        }

        public object GetValue(string variableName)
        {
            var variableDefinition = this.GetVariableDefinition(variableName);
            var typeDefinition = this.GetTypeDefinition(variableDefinition.Type);

            if (this.variables.ContainsKey(variableName))
            {
                return this.typeTranslator.TranslatePerDefinition(this.variables[variableName], typeDefinition);
            }

            throw new NotImplementedException();
        }

        public object GetValue(GraphQLVariable value) => this.GetValue(value.Name.Value);

        private GraphQLScalarType GetTypeDefinition(GraphQLType typeDefinition)
        {
            if (typeDefinition is GraphQLNamedType)
                return this.typeTranslator.GetType((GraphQLNamedType)typeDefinition);

            if (typeDefinition is Language.AST.GraphQLNonNullType)
                return new Type.GraphQLNonNullType((GraphQLNullableType)this.GetTypeDefinition(((Language.AST.GraphQLNonNullType)typeDefinition).Type));

            if (typeDefinition is GraphQLListType)
                return new GraphQLList(this.GetTypeDefinition(((GraphQLListType)typeDefinition).Type));

            return null;
        }

        private GraphQLVariableDefinition GetVariableDefinition(string variableName)
        {
            return this.variableDefinitions
                .SingleOrDefault(e => e.Variable.Name.Value == variableName);
        }
    }
}