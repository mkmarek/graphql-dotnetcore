namespace GraphQLCore.Utils
{
    using Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Type;
    using Type.Complex;
    using Type.Directives;
    using Type.Scalar;

    public class SchemaPrinter
    {
        private IGraphQLSchema schema;

        public SchemaPrinter(IGraphQLSchema schema)
        {
            this.schema = schema;
        }

        public string PrintSchema()
        {
            return this.PrintFilteredSchema(e => !IsSpecDirective(e), IsDefinedType);
        }

        public string PrintIntrospectionSchema()
        {
            return this.PrintFilteredSchema(IsSpecDirective, IsIntrospectionType);
        }

        private static bool IsSpecDirective(string directiveName)
        {
            return
                directiveName == "skip" ||
                directiveName == "include" ||
                directiveName == "deprecated";
        }

        private static bool IsDefinedType(string typename)
        {
            return !IsIntrospectionType(typename) && !IsBuiltInScalar(typename);
        }

        private static bool IsIntrospectionType(string typename)
        {
            return typename.StartsWith("__");
        }

        private static bool IsBuiltInScalar(string typename)
        {
            var scalarNames = new[] { "String", "Boolean", "Int", "Float", "ID", "Long" };

            return scalarNames.Contains(typename);
        }

        private static bool IsSchemaOfCommonNames(IGraphQLSchema schema)
        {
            return
                (schema.QueryType == null || schema.QueryType.Name == "Query") &&
                (schema.MutationType == null || schema.MutationType.Name == "Mutation") &&
                (schema.SubscriptionType == null || schema.SubscriptionType.Name == "Subscription");
        }

        private static string PrintSchemaDefinition(IGraphQLSchema schema)
        {
            var operationTypes = new List<string>();

            if (IsSchemaOfCommonNames(schema))
                return null;

            if (schema.QueryType != null)
                operationTypes.Add($"  query: {schema.QueryType.Name}");
            if (schema.MutationType != null)
                operationTypes.Add($"  mutation: {schema.MutationType.Name}");
            if (schema.SubscriptionType != null)
                operationTypes.Add($"  subscription: {schema.SubscriptionType.Name}");

            return $"schema {{\n{string.Join("\n", operationTypes)}\n}}";
        }

        private static string PrintEnum(GraphQLEnumType type)
        {
            return
                PrintDescription(type) +
                $"enum {type.Name} {{\n" +
                $"{PrintEnumValues(type.Values.Values)}\n}}";
        }

        private static string PrintScalar(GraphQLScalarType type)
        {
            return $"{PrintDescription(type)}scalar {type.Name}";
        }

        private static string PrintEnumValues(IEnumerable<GraphQLEnumValueInfo> values)
        {
            return
                string.Join("\n", values.Select((value, i) =>
                    $"{PrintDescription(value, "  ", i == 0)}  {value.Name}{PrintDeprecated(value)}"));
        }

        private static string PrintDeprecated(GraphQLFieldInfo type)
        {
            var reason = type.DeprecationReason;

            if (reason == null)
                return string.Empty;
            if (reason == string.Empty || reason == "No longer supported")
                return " @deprecated";
            return $" @deprecated(reason: \"{reason}\")";
        }

        private static string PrintDescription(GraphQLBaseType type, string indentation = "", bool firstInBlock = true)
        {
            return PrintDescription(type.Description, indentation, firstInBlock);
        }

        private static string PrintDescription(GraphQLFieldInfo field, string indentation = "", bool firstInBlock = true)
        {
            return PrintDescription(field.Description, indentation, firstInBlock);
        }

        private static string PrintDescription(string rawDescription, string indentation = "", bool firstInBlock = true)
        {
            if (string.IsNullOrEmpty(rawDescription))
                return string.Empty;

            var lines = rawDescription.Split('\n');
            var description = new StringBuilder();

            if (indentation != string.Empty && !firstInBlock)
                description.Append('\n');

            foreach (var line in lines)
            {
                if (line == string.Empty)
                    description.Append($"{indentation}#\n");
                else
                {
                    var sublines = StringUtils.BreakLine(line, 120 - indentation.Length);
                    description.Append(string.Join(string.Empty, sublines.Select(subline => $"{indentation}# {subline}\n")));
                }
            }

            return description.ToString();
        }

        private string PrintFilteredSchema(Func<string, bool> directiveFilter, Func<string, bool> typeFilter)
        {
            var directives = this.schema.SchemaRepository.GetDirectives()
                .Where(e => directiveFilter(e.Name));
            var types = this.schema.SchemaRepository.GetAllKnownTypes()
                .Where(e => typeFilter(e.Name))
                .OrderBy(e => e.Name);

            var lines = new[] { PrintSchemaDefinition(this.schema) }
                .Concat(directives.Select(this.PrintDirective))
                .Concat(types.Select(this.PrintType))
                .Where(e => e != null);

            return string.Join("\n\n", lines);
        }

        private string PrintType(GraphQLBaseType type)
        {
            if (type is GraphQLEnumType)
                return PrintEnum((GraphQLEnumType)type);
            if (type is GraphQLScalarType)
                return PrintScalar((GraphQLScalarType)type);
            if (type is GraphQLObjectType)
                return this.PrintObject((GraphQLObjectType)type);
            if (type is GraphQLInterfaceType)
                return this.PrintInterface((GraphQLInterfaceType)type);
            if (type is GraphQLUnionType)
                return this.PrintUnion((GraphQLUnionType)type);
            if (type is GraphQLInputObjectType)
                return this.PrintInputObject((GraphQLInputObjectType)type);

            throw new GraphQLException($"Type {type.Name} can't be printed.");
        }

        private string PrintObject(GraphQLObjectType type)
        {
            var interfaces = this.schema.SchemaRepository.GetImplementingInterfaces(type);
            var implementedInterfaces = interfaces.Any()
                ? $" implements {string.Join(", ", interfaces.Select(e => e.Name))}"
                : string.Empty;

            return
                PrintDescription(type) +
                $"type {type.Name}{implementedInterfaces} {{\n" +
                $"{this.PrintFields(type)}\n}}";
        }

        private string PrintInterface(GraphQLInterfaceType type)
        {
            return 
                PrintDescription(type) +
                $"interface {type.Name} {{\n" +
                $"{this.PrintFields(type)}\n}}";
        }

        private string PrintUnion(GraphQLUnionType type)
        {
            var printedTypes = string.Join(" | ",
                type.PossibleTypes.Select(t =>
                    this.schema.SchemaRepository.GetSchemaTypeFor(t).Name));

            return
                PrintDescription(type) +
                $"union {type.Name} = {printedTypes}";
        }

        private string PrintInputObject(GraphQLInputObjectType type)
        {
            var fields = type.GetFieldsInfo();
            var printedFields = string.Join("\n", fields.Select((field, i) =>
                $"{PrintDescription(field, "  ", i == 0)}  {this.PrintInputValue(field)}{PrintDeprecated(field)}"));

            return
                PrintDescription(type) +
                $"input {type.Name} {{\n" +
                $"{printedFields}\n}}";
        }

        private string PrintFields(GraphQLComplexType type)
        {
            var fields = type.GetFieldsInfo();

            return string.Join("\n", fields.Select((field, i) => this.PrintField(field, i == 0)));
        }

        private string PrintField(GraphQLFieldInfo field, bool firstInBlock)
        {
            var type = field.GetGraphQLType(this.schema.SchemaRepository);

            return
                PrintDescription(field, "  ", firstInBlock) +
                $"  {field.Name}{this.PrintArguments(field.Arguments.Values, "  ")}: " +
                $"{type}{PrintDeprecated(field)}";
        }

        private string PrintArguments(IEnumerable<GraphQLObjectTypeArgumentInfo> arguments, string indentation = "")
        {
            if (!arguments.Any())
                return string.Empty;

            if (arguments.All(e => e.Description == null))
                return $"({string.Join(", ", arguments.Select(this.PrintInputValue))})";

            var printedArguments = arguments.Select((argument, i) =>
                $"{PrintDescription(argument, "  " + indentation, i == 0)}  {indentation}{this.PrintInputValue(argument)}");

            return $"(\n{string.Join("\n", printedArguments)}\n{indentation})";
        }

        private string PrintInputValue(GraphQLFieldInfo arg)
        {
            var argType = arg.GetGraphQLType(this.schema.SchemaRepository) as GraphQLInputType;
            var argDeclaration = $"{arg.Name}: {argType}";

            if (arg.DefaultValue.IsSet)
                argDeclaration += $" = {arg.DefaultValue.GetSerialized(argType, this.schema.SchemaRepository)}";

            return argDeclaration;
        }

        private string PrintDirective(GraphQLDirectiveType directive)
        {
            return
                PrintDescription(directive.Description) +
                $"directive @{directive.Name}{this.PrintArguments(directive.GetArguments())}" +
                $" on {string.Join(" | ", directive.Locations)}";
        }
    }
}
