using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLCore.Language.AST;

namespace GraphQLCore.Language
{
    public class GraphQLAstVisitor
    {
        public void Visit(GraphQLDocument ast)
        {
            foreach (var definition in ast.Definitions)
                this.VisitNode(definition);
        }

        public virtual ASTNode VisitNode(ASTNode node)
        {
            switch (node.Kind)
            {
                case ASTNodeKind.OperationDefinition: return this.VisitOperationDefinition((GraphQLOperationDefinition)node);
                case ASTNodeKind.SelectionSet: return this.VisitSelectionSet((GraphQLSelectionSet)node);
                case ASTNodeKind.Field: return this.VisitFieldSelection((GraphQLFieldSelection)node);
                case ASTNodeKind.Name: return this.VisitName((GraphQLName)node);
                case ASTNodeKind.Argument: return this.VisitArgument((GraphQLArgument)node);
                case ASTNodeKind.FragmentSpread: return this.VisitFragmentSpread((GraphQLFragmentSpread)node);
                case ASTNodeKind.FragmentDefinition: return this.VisitFragmentDefinition((GraphQLFragmentDefinition)node);
                case ASTNodeKind.InlineFragment: return this.VisitInlineFragment((GraphQLInlineFragment)node);
                case ASTNodeKind.NamedType: return this.VisitNamedType((GraphQLNamedType)node);
                case ASTNodeKind.Directive: return this.VisitDirective((GraphQLDirective)node);
                case ASTNodeKind.Variable: return this.VisitVariable((GraphQLVariable)node);
                case ASTNodeKind.IntValue: return this.VisitIntValue((GraphQLValue<int>)node);
                case ASTNodeKind.FloatValue: return this.VisitFloatValue((GraphQLValue<float>)node);
                case ASTNodeKind.StringValue: return this.VisitStringValue((GraphQLValue<string>)node);
                case ASTNodeKind.BooleanValue: return this.VisitBooleanValue((GraphQLValue<bool>)node);
                case ASTNodeKind.EnumValue: return this.VisitEnumValue((GraphQLValue<string>)node);
            }

            throw new NotImplementedException();
        }

        public virtual GraphQLValue<int> VisitIntValue(GraphQLValue<int> value)
        {
            return value;
        }

        public virtual GraphQLVariable VisitVariable(GraphQLVariable variable)
        {
            if (variable.Name != null)
                this.VisitNode(variable.Name);

            return variable;
        }

        public virtual GraphQLFragmentDefinition VisitFragmentDefinition(GraphQLFragmentDefinition node)
        {
            this.VisitNode(node.TypeCondition);
            return node;
        }

        public virtual GraphQLOperationDefinition VisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            if (definition.Name != null)
                this.VisitNode(definition.Name);

            this.VisitNode(definition.SelectionSet);
            return definition;
        }

        public virtual GraphQLSelectionSet VisitSelectionSet(GraphQLSelectionSet selectionSet)
        {
            foreach (var selection in selectionSet.Selections)
                this.VisitNode(selection);

            return selectionSet;
        }

        public virtual GraphQLFieldSelection VisitFieldSelection(GraphQLFieldSelection selection)
        {
            this.VisitNode(selection.Name);

            if (selection.Alias != null)
                this.VisitAlias((GraphQLName)this.VisitNode(selection.Alias));

            if (selection.SelectionSet != null)
                this.VisitNode(selection.SelectionSet);

            if (selection.Arguments != null)
                this.VisitArguments(selection.Arguments);

            return selection;
        }

        public virtual GraphQLName VisitName(GraphQLName name)
        {
            return name;
        }

        public virtual IEnumerable<GraphQLArgument> VisitArguments(IEnumerable<GraphQLArgument> arguments)
        {
            foreach (var argument in arguments)
                this.VisitNode(argument);

            return arguments;
        }

        public virtual GraphQLArgument VisitArgument(GraphQLArgument argument)
        {
            if (argument.Name != null)
                this.VisitNode(argument.Name);

            if (argument.Value != null)
                this.VisitNode(argument.Value);

            return argument;
        }

        public virtual GraphQLName VisitAlias(GraphQLName alias)
        {
            return alias;
        }

        public virtual GraphQLFragmentSpread VisitFragmentSpread(GraphQLFragmentSpread fragmentSpread)
        {
            this.VisitNode(fragmentSpread.Name);
            return fragmentSpread;
        }

        public virtual GraphQLNamedType VisitNamedType(GraphQLNamedType typeCondition)
        {
            return typeCondition;
        }

        public virtual GraphQLInlineFragment VisitInlineFragment(GraphQLInlineFragment inlineFragment)
        {
            if (inlineFragment.TypeCondition != null)
                this.VisitNode(inlineFragment.TypeCondition);

            if (inlineFragment.Directives != null)
                this.VisitDirectives(inlineFragment.Directives);

            if (inlineFragment.SelectionSet != null)
                this.VisitSelectionSet(inlineFragment.SelectionSet);

            return inlineFragment;
        }

        public virtual IEnumerable<GraphQLDirective> VisitDirectives(IEnumerable<GraphQLDirective> directives)
        {
            foreach (var directive in directives)
                this.VisitNode(directive);

            return directives;
        }

        public virtual GraphQLDirective VisitDirective(GraphQLDirective directive)
        {
            if (directive.Name != null)
                this.VisitNode(directive.Name);

            if (directive.Arguments != null)
                this.VisitArguments(directive.Arguments);

            return directive;
        }

        public virtual GraphQLValue<float> VisitFloatValue(GraphQLValue<float> value)
        {
            return value;
        }

        public virtual GraphQLValue<string> VisitStringValue(GraphQLValue<string> value)
        {
            return value;
        }

        public virtual GraphQLValue<bool> VisitBooleanValue(GraphQLValue<bool> value)
        {
            return value;
        }

        public virtual GraphQLValue<string> VisitEnumValue(GraphQLValue<string> value)
        {
            return value;
        }
    }
}
