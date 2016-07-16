using GraphQLCore.Language.AST;
using GraphQLCore.Type;
using GraphQLCore.Type.Translation;
using GraphQLCore.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

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
                return this.GetValue(this.variables, variableName, typeDefinition);
            }

            throw new NotImplementedException();
        }

        public object GetValue(GraphQLVariable value) => this.GetValue(value.Name.Value);

        private static List<GraphQLObjectTypeFieldInfo> GetFieldsFromDefinition(GraphQLObjectType typeDefinition)
        {
            return typeDefinition.GetFieldsInfo()
                .Where(e => e.IsResolver == false)
                .ToList();
        }

        private object CreateParameterObject(
            GraphQLObjectType typeDefinition, IDictionary<string, dynamic> variable, IEnumerable<GraphQLObjectTypeFieldInfo> fields)
        {
            var systemType = this.GetSystemType(typeDefinition);
            var resultObject = Activator.CreateInstance(systemType);
            var objectTypeTranslator = this.typeTranslator.GetObjectTypeTranslatorFor(typeDefinition);

            foreach (var field in fields)
            {
                if (!variable.ContainsKey(field.Name))
                    continue;

                var variableProp = this.GetValue(variable, field.Name, objectTypeTranslator.GetField(field.Name).Type);
                variableProp = ReflectionUtilities.ChangeValueType(
                    variableProp, ReflectionUtilities.GetReturnValueFromLambdaExpression(field.Lambda));

                this.MakeSetterFromLambda(field.Lambda).DynamicInvoke(resultObject, variableProp);
            }

            return resultObject;
        }

        private object GetInputObjectValue(dynamic variableValue, GraphQLObjectType typeDefinition)
        {
            var fields = GetFieldsFromDefinition(typeDefinition);

            return this.CreateParameterObject(typeDefinition, variableValue, fields);
        }

        private object GetInputScalarValue(object variableValue, GraphQLScalarType typeDefinition)
        {
            var systemType = this.GetSystemType(typeDefinition);
            return ReflectionUtilities.ChangeValueType(variableValue, systemType);
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
                return new Type.GraphQLNonNullType((GraphQLNullableType)this.GetTypeDefinition(((Language.AST.GraphQLNonNullType)typeDefinition).Type));

            if (typeDefinition is GraphQLListType)
                return new GraphQLList(this.GetTypeDefinition(((GraphQLListType)typeDefinition).Type));

            return null;
        }

        private object GetValue(IDictionary<string, object> values, string variableName, GraphQLScalarType typeDefinition)
        {
            var variableValue = values[variableName];

            if (variableValue is ExpandoObject && typeDefinition is GraphQLObjectType)
                return this.GetInputObjectValue(variableValue, (GraphQLObjectType)typeDefinition);

            return this.GetInputScalarValue(variableValue, typeDefinition);
        }

        private GraphQLVariableDefinition GetVariableDefinition(string variableName)
        {
            return this.variableDefinitions
                .SingleOrDefault(e => e.Variable.Name.Value == variableName);
        }

        private Delegate MakeSetterFromLambda(LambdaExpression lambda)
        {
            var member = (MemberExpression)lambda.Body;
            var param = Expression.Parameter(member.Type, "value");
            var setter = Expression.Lambda(Expression.Assign(member, param), lambda.Parameters[0], param);

            return setter.Compile();
        }
    }
}