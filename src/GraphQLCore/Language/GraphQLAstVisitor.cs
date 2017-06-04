namespace GraphQLCore.Language
{
    using AST;
    using System.Collections.Generic;

    public class GraphQLAstVisitor
    {
        protected IDictionary<string, GraphQLFragmentDefinition> Fragments { get; private set; }

        public GraphQLAstVisitor()
        {
            this.Fragments = new Dictionary<string, GraphQLFragmentDefinition>();
        }

        public virtual GraphQLName BeginVisitAlias(GraphQLName alias)
        {
            return alias;
        }

        public virtual GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            if (argument.Name != null)
                this.BeginVisitNode(argument.Name);

            if (argument.Value != null)
                this.BeginVisitNode(argument.Value);

            return this.EndVisitArgument(argument);
        }

        public virtual GraphQLScalarValue BeginVisitBooleanValue(GraphQLScalarValue value)
        {
            return value;
        }

        public virtual GraphQLDirective BeginVisitDirective(GraphQLDirective directive)
        {
            if (directive.Name != null)
                this.BeginVisitNode(directive.Name);

            if (directive.Arguments != null)
                this.BeginVisitNodeCollection(directive.Arguments);

            return this.EndVisitDirective(directive);
        }

        public virtual GraphQLEnumTypeDefinition BeginVisitEnumTypeDefinition(GraphQLEnumTypeDefinition node)
        {
            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            if (node.Values != null)
                this.BeginVisitNodeCollection(node.Values);

            return node;
        }

        public virtual GraphQLScalarValue BeginVisitEnumValue(GraphQLScalarValue value)
        {
            return value;
        }

        public virtual GraphQLEnumValueDefinition BeginVisitEnumValueDefinition(GraphQLEnumValueDefinition node)
        {
            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            return node;
        }

        public virtual GraphQLFieldDefinition BeginVisitFieldDefinition(GraphQLFieldDefinition node)
        {
            if (node.Arguments != null)
                this.BeginVisitNodeCollection(node.Arguments);

            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            return node;
        }

        public virtual GraphQLFieldSelection BeginVisitFieldSelection(GraphQLFieldSelection selection)
        {
            this.BeginVisitNode(selection.Name);

            if (selection.Alias != null)
                this.BeginVisitAlias((GraphQLName)this.BeginVisitNode(selection.Alias));

            if (selection.Arguments != null)
                this.BeginVisitNodeCollection(selection.Arguments);

            if (selection.Directives != null)
                this.BeginVisitNodeCollection(selection.Directives);

            if (selection.SelectionSet != null)
                this.BeginVisitNode(selection.SelectionSet);

            return this.EndVisitFieldSelection(selection);
        }

        public virtual GraphQLScalarValue BeginVisitFloatValue(GraphQLScalarValue value)
        {
            return value;
        }

        public virtual GraphQLFragmentDefinition BeginVisitFragmentDefinition(GraphQLFragmentDefinition node)
        {
            this.BeginVisitNode(node.TypeCondition);
            this.BeginVisitNode(node.Name);

            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            if (node.SelectionSet != null)
                this.BeginVisitNode(node.SelectionSet);

            return this.EndVisitFragmentDefinition(node);
        }

        public virtual GraphQLFragmentDefinition EndVisitFragmentDefinition(GraphQLFragmentDefinition node)
        {
            return node;
        }

        public virtual GraphQLFragmentSpread BeginVisitFragmentSpread(GraphQLFragmentSpread fragmentSpread)
        {
            this.BeginVisitNode(fragmentSpread.Name);

            if (fragmentSpread.Directives != null)
                this.BeginVisitNodeCollection(fragmentSpread.Directives);

            return fragmentSpread;
        }

        public virtual GraphQLInlineFragment BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment)
        {
            if (inlineFragment.TypeCondition != null)
                this.BeginVisitNode(inlineFragment.TypeCondition);

            if (inlineFragment.Directives != null)
                this.BeginVisitNodeCollection(inlineFragment.Directives);

            if (inlineFragment.SelectionSet != null)
                this.BeginVisitSelectionSet(inlineFragment.SelectionSet);

            return this.EndVisitInlineFragment(inlineFragment);
        }

        public virtual GraphQLInputObjectTypeDefinition BeginVisitInputObjectTypeDefinition(GraphQLInputObjectTypeDefinition node)
        {
            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            if (node.Fields != null)
                this.BeginVisitNodeCollection(node.Fields);

            return node;
        }

        public virtual GraphQLInputValueDefinition BeginVisitInputValueDefinition(GraphQLInputValueDefinition node)
        {
            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            return node;
        }

        public virtual GraphQLInterfaceTypeDefinition BeginVisitInterfaceTypeDefinition(GraphQLInterfaceTypeDefinition node)
        {
            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            if (node.Fields != null)
                this.BeginVisitNodeCollection(node.Fields);

            return node;
        }

        public virtual GraphQLScalarValue BeginVisitIntValue(GraphQLScalarValue value)
        {
            return value;
        }

        public virtual GraphQLListValue BeginVisitListValue(GraphQLListValue node)
        {
            this.BeginVisitNodeCollection(node.Values);

            return this.EndVisitListValue(node);
        }

        public virtual GraphQLName BeginVisitName(GraphQLName name)
        {
            return name;
        }

        public virtual GraphQLNamedType BeginVisitNamedType(GraphQLNamedType typeCondition)
        {
            return typeCondition;
        }

        public virtual ASTNode BeginVisitNode(ASTNode node)
        {
            switch (node.Kind)
            {
                case ASTNodeKind.OperationDefinition: return this.BeginVisitOperationDefinition((GraphQLOperationDefinition)node);
                case ASTNodeKind.Argument: return this.BeginVisitArgument((GraphQLArgument)node);
                case ASTNodeKind.SelectionSet: return this.BeginVisitSelectionSet((GraphQLSelectionSet)node);
                case ASTNodeKind.Field: return this.BeginVisitNonIntrospectionFieldSelection((GraphQLFieldSelection)node);
                case ASTNodeKind.Name: return this.BeginVisitName((GraphQLName)node);
                case ASTNodeKind.FragmentSpread: return this.BeginVisitFragmentSpread((GraphQLFragmentSpread)node);
                case ASTNodeKind.FragmentDefinition: return this.BeginVisitFragmentDefinition((GraphQLFragmentDefinition)node);
                case ASTNodeKind.InlineFragment: return this.BeginVisitInlineFragment((GraphQLInlineFragment)node);
                case ASTNodeKind.NamedType: return this.BeginVisitNamedType((GraphQLNamedType)node);
                case ASTNodeKind.Directive: return this.BeginVisitDirective((GraphQLDirective)node);
                case ASTNodeKind.Variable: return this.BeginVisitVariable((GraphQLVariable)node);
                case ASTNodeKind.IntValue: return this.BeginVisitIntValue((GraphQLScalarValue)node);
                case ASTNodeKind.FloatValue: return this.BeginVisitFloatValue((GraphQLScalarValue)node);
                case ASTNodeKind.StringValue: return this.BeginVisitStringValue((GraphQLScalarValue)node);
                case ASTNodeKind.BooleanValue: return this.BeginVisitBooleanValue((GraphQLScalarValue)node);
                case ASTNodeKind.EnumValue: return this.BeginVisitEnumValue((GraphQLScalarValue)node);
                case ASTNodeKind.ListValue: return this.BeginVisitListValue((GraphQLListValue)node);
                case ASTNodeKind.ObjectValue: return this.BeginVisitObjectValue((GraphQLObjectValue)node);
                case ASTNodeKind.ObjectField: return this.BeginVisitObjectField((GraphQLObjectField)node);
                case ASTNodeKind.VariableDefinition: return this.BeginVisitVariableDefinition((GraphQLVariableDefinition)node);
                case ASTNodeKind.EnumTypeDefinition: return this.BeginVisitEnumTypeDefinition((GraphQLEnumTypeDefinition)node);
                case ASTNodeKind.EnumValueDefinition: return this.BeginVisitEnumValueDefinition((GraphQLEnumValueDefinition)node);
                case ASTNodeKind.FieldDefinition: return this.BeginVisitFieldDefinition((GraphQLFieldDefinition)node);
                case ASTNodeKind.InputObjectTypeDefinition: return this.BeginVisitInputObjectTypeDefinition((GraphQLInputObjectTypeDefinition)node);
                case ASTNodeKind.InputValueDefinition: return this.BeginVisitInputValueDefinition((GraphQLInputValueDefinition)node);
                case ASTNodeKind.InterfaceTypeDefinition: return this.BeginVisitInterfaceTypeDefinition((GraphQLInterfaceTypeDefinition)node);
                case ASTNodeKind.ObjectTypeDefinition: return this.BeginVisitObjectTypeDefinition((GraphQLObjectTypeDefinition)node);
                case ASTNodeKind.ScalarTypeDefinition: return this.BeginVisitScalarTypeDefinition((GraphQLScalarTypeDefinition)node);
                case ASTNodeKind.SchemaDefinition: return this.BeginVisitSchemaDefinition((GraphQLSchemaDefinition)node);
                case ASTNodeKind.UnionTypeDefinition: return this.BeginVisitUnionTypeDefinition((GraphQLUnionTypeDefinition)node);
            }

            return null;
        }

        public virtual IEnumerable<T> BeginVisitNodeCollection<T>(IEnumerable<T> nodes)
            where T : ASTNode
        {
            foreach (var node in nodes)
                this.BeginVisitNode(node);

            return nodes;
        }

        public virtual GraphQLObjectField BeginVisitObjectField(GraphQLObjectField node)
        {
            this.BeginVisitNode(node.Name);

            this.BeginVisitNode(node.Value);

            return node;
        }

        public virtual GraphQLObjectTypeDefinition BeginVisitObjectTypeDefinition(GraphQLObjectTypeDefinition node)
        {
            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            if (node.Fields != null)
                this.BeginVisitNodeCollection(node.Fields);

            return node;
        }

        public virtual GraphQLObjectValue BeginVisitObjectValue(GraphQLObjectValue node)
        {
            this.BeginVisitNodeCollection(node.Fields);

            return this.EndVisitObjectValue(node);
        }

        public virtual GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            if (definition.Name != null)
                this.BeginVisitNode(definition.Name);

            if (definition.Directives != null)
                this.BeginVisitNodeCollection(definition.Directives);

            if (definition.VariableDefinitions != null)
                this.BeginVisitNodeCollection(definition.VariableDefinitions);

            this.BeginVisitNode(definition.SelectionSet);

            return this.EndVisitOperationDefinition(definition);
        }

        public virtual GraphQLScalarTypeDefinition BeginVisitScalarTypeDefinition(GraphQLScalarTypeDefinition node)
        {
            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            return node;
        }

        public virtual GraphQLSchemaDefinition BeginVisitSchemaDefinition(GraphQLSchemaDefinition node)
        {
            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            return node;
        }

        public virtual GraphQLSelectionSet BeginVisitSelectionSet(GraphQLSelectionSet selectionSet)
        {
            this.BeginVisitNodeCollection(selectionSet.Selections);

            return selectionSet;
        }

        public virtual GraphQLScalarValue BeginVisitStringValue(GraphQLScalarValue value)
        {
            return value;
        }

        public virtual GraphQLUnionTypeDefinition BeginVisitUnionTypeDefinition(GraphQLUnionTypeDefinition node)
        {
            if (node.Directives != null)
                this.BeginVisitNodeCollection(node.Directives);

            return node;
        }

        public virtual GraphQLVariable BeginVisitVariable(GraphQLVariable variable)
        {
            if (variable.Name != null)
                this.BeginVisitNode(variable.Name);

            return this.EndVisitVariable(variable);
        }

        public virtual GraphQLVariableDefinition BeginVisitVariableDefinition(GraphQLVariableDefinition node)
        {
            this.BeginVisitNode(node.Type);

            return node;
        }

        public virtual GraphQLArgument EndVisitArgument(GraphQLArgument argument)
        {
            return argument;
        }

        public virtual GraphQLDirective EndVisitDirective(GraphQLDirective directive)
        {
            return directive;
        }

        public virtual GraphQLFieldSelection EndVisitFieldSelection(GraphQLFieldSelection selection)
        {
            return selection;
        }

        public virtual GraphQLInlineFragment EndVisitInlineFragment(GraphQLInlineFragment inlineFragment)
        {
            return inlineFragment;
        }

        public virtual GraphQLListValue EndVisitListValue(GraphQLListValue node)
        {
            return node;
        }

        public virtual GraphQLObjectValue EndVisitObjectValue(GraphQLObjectValue node)
        {
            return node;
        }

        public virtual GraphQLOperationDefinition EndVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            return definition;
        }

        public virtual GraphQLVariable EndVisitVariable(GraphQLVariable variable)
        {
            return variable;
        }

        public virtual void Visit(GraphQLDocument ast)
        {
            foreach (var definition in ast.Definitions)
            {
                if (definition.Kind == ASTNodeKind.FragmentDefinition)
                {
                    var fragment = (GraphQLFragmentDefinition)definition;
                    if (!this.Fragments.ContainsKey(fragment.Name.Value))
                        this.Fragments.Add(fragment.Name.Value, fragment);
                }
            }

            this.BeginVisitNodeCollection(ast.Definitions);
        }

        private ASTNode BeginVisitNonIntrospectionFieldSelection(GraphQLFieldSelection selection)
        {
            return this.BeginVisitFieldSelection(selection);
        }
    }
}