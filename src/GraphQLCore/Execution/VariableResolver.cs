using GraphQLCore.Exceptions;
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
using System.Reflection;

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

        public Result GetValue(string variableName)
        {
            var variableDefinition = this.GetVariableDefinition(variableName);
            if (variableDefinition == null)
                return new Result(null);

            var typeDefinition = this.GetTypeDefinition(variableDefinition.Type);

            object variableValue;

            if (this.variables.TryGetValue(variableName, out variableValue))
                return new Result(this.TranslatePerDefinition(variableValue, typeDefinition));

            if (variableDefinition.DefaultValue != null)
                return ((GraphQLInputType)typeDefinition).GetValueFromAst(variableDefinition.DefaultValue,
                    this.schemaRepository);

            if (typeDefinition is GraphQLNonNull)
                throw new GraphQLException($"Variable \"{variableName}\" of required type \"{typeDefinition}\" was not provided.",
                    new[] { variableDefinition });

            return new Result(null);
        }

        public Result GetValue(GraphQLVariable value) => this.GetValue(value.Name.Value);

        public object TranslatePerDefinition(object inputObject, GraphQLBaseType typeDefinition)
        {
            if (typeDefinition is GraphQLNonNull)
                return this.TranslatePerDefinition(inputObject, ((GraphQLNonNull)typeDefinition).UnderlyingNullableType);

            if (typeDefinition is GraphQLInputObjectType)
                return this.CreateObjectFromDynamic((GraphQLInputObjectType)typeDefinition, (ExpandoObject)inputObject);

            if (typeDefinition is GraphQLList)
            {
                if (inputObject == null)
                    return null;

                if (ReflectionUtilities.IsCollection(inputObject.GetType()))
                    return ReflectionUtilities.ChangeValueType(this.CreateList((IEnumerable)inputObject, (GraphQLList)typeDefinition), this.schemaRepository.GetInputSystemTypeFor(typeDefinition));

                return this.CreateSingleValueList(inputObject, (GraphQLList)typeDefinition);
            }

            return inputObject;
        }

        public object TranslatePerDefinition(object inputObject, System.Type type)
        {
            if (inputObject == null)
                return null;

            var typeDefinition = this.schemaRepository.GetSchemaInputTypeFor(type);

            if (inputObject is ExpandoObject && typeDefinition is GraphQLInputObjectType)
                return this.CreateObjectFromDynamic((GraphQLInputObjectType)typeDefinition, (ExpandoObject)inputObject);

            if (ReflectionUtilities.IsCollection(inputObject.GetType()) && typeDefinition is GraphQLList)
                return ReflectionUtilities.ChangeValueType(this.CreateList((IEnumerable)inputObject, (GraphQLList)typeDefinition), type);

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

        private IEnumerable CreateSingleValueList(object inputObject, GraphQLList typeDefinition)
        {
            var systemType = this.schemaRepository.GetInputSystemTypeFor(typeDefinition.MemberType);

            var singleValue = this.TranslatePerDefinition(inputObject, typeDefinition.MemberType);
            singleValue = ReflectionUtilities.ChangeValueType(singleValue, systemType);

            yield return singleValue;
        }

        private GraphQLBaseType GetTypeDefinition(GraphQLType typeDefinition)
        {
            if (typeDefinition is GraphQLNamedType)
                return this.schemaRepository.GetSchemaInputTypeByName(((GraphQLNamedType)typeDefinition).Name.Value);

            if (typeDefinition is GraphQLNonNullType)
                return new GraphQLNonNull(this.GetTypeDefinition(((GraphQLNonNullType)typeDefinition).Type));

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