using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Parser.Language.AST
{
    public enum ASTNodeKind
    {
        Name,
        Document,
        OperationDefinition,
        VariableDefinition,
        Variable,
        SelectionSet,
        Field,
        Argument,
        FragmentSpread,
        InlineFragment,
        FragmentDefinition,
        IntValue,
        FloatValue,
        StringValue,
        BooleanValue,
        EnumValue,
        ListValue,
        ObjectValue,
        ObjectField,
        Directive,
        NamedType,
        ListType,
        NonNullType,
        SchemaDefinition,
        OperationTypeDefinition,
        ScalarTypeDefinition,
        ObjectTypeDefinition,
        FieldDefinition,
        InputValueDefinition,
        InterfaceTypeDefinition,
        UnionTypeDefinition,
        EnumTypeDefinition,
        EnumValueDefinition,
        InputObjectTypeDefinition,
        TypeExtensionDefinition,
        DirectiveDefinition
    }

    public enum OperationType
    {
        Query,
        Mutation,
        Subscription
    }

}
