using GraphQLCore.Language.AST;
using GraphQLCore.Type;
using GraphQLCore.Type.Translation;
using GraphQLCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace GraphQLCore.Execution
{
    public class VariableResolver : IVariableResolver
    {
        private ISchemaRepository schemaRepository;
        private IEnumerable<GraphQLVariableDefinition> variableDefinitions;
        private Dictionary<string, object> variables;

        public VariableResolver(dynamic variables, ISchemaRepository schemaRepository, IEnumerable<GraphQLVariableDefinition> variableDefinitions)
        {
            this.variables = ((ExpandoObject)variables).ToDictionary(e => e.Key, e => e.Value);
            this.variableDefinitions = variableDefinitions;
            this.schemaRepository = schemaRepository;
            this.schemaRepository.VariableResolver = this;
        }

        public object CreateObjectFromDynamic(GraphQLInputObjectType inputObjectType, ExpandoObject inputObject)
        {
            var systemType = this.schemaRepository.GetInputSystemTypeFor(inputObjectType);
            var fields = inputObjectType.GetFieldsInfo();
            var inputObjectDictionary = (IDictionary<string, object>)inputObject;

            var resultObject = Activator.CreateInstance(systemType);

            foreach (var field in fields)
            {
                if (!inputObjectDictionary.ContainsKey(field.Name))
                    continue;

                this.AssignValueToField(inputObjectDictionary[field.Name], resultObject, field.Lambda);
            }

            return resultObject;
        }

        public object GetValue(string variableName)
        {
            var variableDefinition = this.GetVariableDefinition(variableName);
            var typeDefinition = this.GetTypeDefinition(variableDefinition.Type);

            if (this.variables.ContainsKey(variableName))
            {
                return this.TranslatePerDefinition(this.variables[variableName], typeDefinition);
            }

            throw new NotImplementedException();
        }

        public object GetValue(GraphQLVariable value) => this.GetValue(value.Name.Value);

        public object TranslatePerDefinition(object inputObject, GraphQLBaseType typeDefinition)
        {
            if (typeDefinition is GraphQLInputObjectType)
                return this.CreateObjectFromDynamic((GraphQLInputObjectType)typeDefinition, (ExpandoObject)inputObject);

            if (typeDefinition is GraphQLList)
                return this.CreateList((IEnumerable)inputObject, (GraphQLList)typeDefinition);

            return inputObject;
        }

        public object TranslatePerDefinition(object inputObject, System.Type type)
        {
            var typeDefinition = this.schemaRepository.GetSchemaInputTypeFor(type);

            if (inputObject is ExpandoObject && typeDefinition is GraphQLInputObjectType)
                return this.CreateObjectFromDynamic((GraphQLInputObjectType)typeDefinition, (ExpandoObject)inputObject);

            return ReflectionUtilities.ChangeValueType(inputObject, type);
        }

        private void AssignValueToField(object value, object resultObject, LambdaExpression expression)
        {
            var variableProp = this.TranslatePerDefinition(
                value,
                ReflectionUtilities.GetReturnValueFromLambdaExpression(expression));

            ReflectionUtilities.MakeSetterFromLambda(expression).DynamicInvoke(resultObject, variableProp);
        }

        private IEnumerable<object> CreateList(IEnumerable inputObject, GraphQLList typeDefinition)
        {
            foreach (var item in inputObject)
                yield return this.TranslatePerDefinition(item, typeDefinition.MemberType);
        }

        private GraphQLBaseType GetTypeDefinition(GraphQLType typeDefinition)
        {
            if (typeDefinition is GraphQLNamedType)
                return this.schemaRepository.GetSchemaInputTypeByName(((GraphQLNamedType)typeDefinition).Name.Value);

            if (typeDefinition is Language.AST.GraphQLNonNullType)
                return new Type.GraphQLNonNullType(this.GetTypeDefinition(((Language.AST.GraphQLNonNullType)typeDefinition).Type));

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