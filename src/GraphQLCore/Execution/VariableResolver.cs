using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLCore.Language.AST;
using GraphQLCore.Type;
using GraphQLCore.Type.Translation;
using GraphQLCore.Utils;

namespace GraphQLCore.Execution
{
    public class VariableResolver
    {
        private IEnumerable<GraphQLVariableDefinition> variableDefinitions;
        private Dictionary<string, object> variables;
        private ITypeTranslator typeTranslator;

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
            var systemType = this.GetSystemType(typeDefinition);

            if (this.variables.ContainsKey(variableName))
                return ReflectionUtilities.ChangeValueType(this.variables[variableName], systemType);

            throw new NotImplementedException();
        }

        private System.Type GetSystemType(GraphQLScalarType typeDefinition)
        {
            return this.typeTranslator.GetType(typeDefinition);
        }

        private GraphQLScalarType GetTypeDefinition(GraphQLType typeDefinition)
        {
            if (typeDefinition is GraphQLNamedType)
                return this.typeTranslator.GetType((GraphQLNamedType)typeDefinition);

            if (typeDefinition is Language.AST.GraphQLNonNullType)
                return new Type.GraphQLNonNullType((GraphQLNullableType)
                    this.GetTypeDefinition(((Language.AST.GraphQLNonNullType)typeDefinition).Type));

            if (typeDefinition is GraphQLListType)
                return new GraphQLList(this.GetTypeDefinition(((GraphQLListType)typeDefinition).Type));

            return null;
        }


        private GraphQLVariableDefinition GetVariableDefinition(string variableName)
        {
            return this.variableDefinitions
                .SingleOrDefault(e => e.Variable.Name.Value == variableName);
        }

        internal object GetValue(GraphQLVariable value)
        {
            return this.GetValue(value.Name.Value);
        }
    }
}
