namespace GraphQL.Parser.Language
{
    using AST;
    using System;
    using System.Collections.Generic;

    public class ParserContext : IDisposable
    {
        private Token CurrentToken;
        private ILexer Lexer;
        private ISource Source;

        public ParserContext(ISource source, ILexer lexer)
        {
            this.Source = source;
            this.Lexer = lexer;

            this.CurrentToken = Lexer.Lex(source);
        }

        public void Dispose()
        {
        }

        public GraphQLDocument Parse()
        {
            return this.ParseDocument();
        }

        private void Advance()
        {
            this.CurrentToken = this.Lexer.Lex(this.Source, this.CurrentToken.End);
        }

        private GraphQLValue AdvanceAndParseValue()
        {
            this.Expect(TokenKind.COLON);
            return this.ParseValueLiteral(false);
        }

        private GraphQLType AdvanceThroughColonAndParseType()
        {
            this.Expect(TokenKind.COLON);
            return this.ParseType();
        }

        private IEnumerable<T> Any<T>(TokenKind open, Func<T> next, TokenKind close)
            where T : ASTNode
        {
            this.Expect(open);

            List<T> nodes = new List<T>();
            while (!this.Skip(close))
                nodes.Add(next());

            return nodes;
        }

        private GraphQLDocument CreateDocument(int start, List<ASTNode> definitions)
        {
            return new GraphQLDocument()
            {
                Location = new GraphQLLocation()
                {
                    Start = start,
                    End = this.CurrentToken.End
                },
                Definitions = definitions
            };
        }

        private ASTNode CreateOperationDefinition(int start, OperationType operation, GraphQLName name)
        {
            return new GraphQLOperationDefinition()
            {
                Operation = operation,
                Name = name,
                VariableDefinitions = this.ParseVariableDefinitions(),
                Directives = this.ParseDirectives(),
                SelectionSet = this.ParseSelectionSet(),
                Location = this.GetLocation(start)
            };
        }

        private ASTNode CreateOperationDefinition(int start)
        {
            return new GraphQLOperationDefinition()
            {
                Operation = OperationType.Query,
                Directives = new GraphQLDirective[] { },
                SelectionSet = this.ParseSelectionSet(),
                Location = this.GetLocation(start)
            };
        }

        private void Expect(TokenKind kind)
        {
            if (this.CurrentToken.Kind == kind)
                this.Advance();
        }

        private void ExpectKeyword(string keyword)
        {
            var token = this.CurrentToken;
            if (token.Kind == TokenKind.NAME && token.Value.Equals(keyword))
            {
                this.Advance();
                return;
            }

            throw new NotImplementedException();
        }

        private GraphQLNamedType ExpectOnKeywordAndParseNamedType()
        {
            this.ExpectKeyword("on");
            return this.ParseNamedType();
        }

        private GraphQLLocation GetLocation(int start)
        {
            return new GraphQLLocation()
            {
                Start = start,
                End = this.CurrentToken.End
            };
        }

        private GraphQLName GetName()
        {
            return this.Peek(TokenKind.NAME) ? this.ParseName() : null;
        }

        private IEnumerable<T> Many<T>(TokenKind open, Func<T> next, TokenKind close)
        {
            this.Expect(open);

            List<T> nodes = new List<T>() { next() };
            while (!this.Skip(close))
                nodes.Add(next());

            return nodes;
        }

        private GraphQLArgument ParseArgument()
        {
            var start = this.CurrentToken.Start;

            return new GraphQLArgument()
            {
                Name = this.ParseName(),
                Value = AdvanceAndParseValue(),
                Location = this.GetLocation(start)
            };
        }

        private IEnumerable<GraphQLInputValueDefinition> ParseArgumentDefs()
        {
            if (!this.Peek(TokenKind.PAREN_L))
            {
                return new GraphQLInputValueDefinition[] { };
            }

            return this.Many(TokenKind.PAREN_L, () => this.ParseInputValueDef(), TokenKind.PAREN_R);
        }

        private IEnumerable<GraphQLArgument> ParseArguments()
        {
            return this.Peek(TokenKind.PAREN_L) ?
                this.Many(TokenKind.PAREN_L, () => this.ParseArgument(), TokenKind.PAREN_R) :
                new GraphQLArgument[] { };
        }

        private GraphQLValue ParseBooleanValue(Token token)
        {
            this.Advance();
            return new GraphQLValue<bool>(ASTNodeKind.BooleanValue)
            {
                Value = token.Value.Equals("true"),
                Location = this.GetLocation(token.Start)
            };
        }

        private GraphQLValue ParseConstantValue()
        {
            return this.ParseValueLiteral(true);
        }

        private ASTNode ParseDefinition()
        {
            if (this.Peek(TokenKind.BRACE_L))
            {
                return this.ParseOperationDefinition();
            }

            if (this.Peek(TokenKind.NAME))
            {
                ASTNode definition = null;
                if ((definition = this.ParseNamedDefinition()) != null)
                    return definition;
            }

            throw new NotImplementedException();
        }

        private GraphQLDirective ParseDirective()
        {
            var start = this.CurrentToken.Start;
            this.Expect(TokenKind.AT);
            return new GraphQLDirective()
            {
                Name = this.ParseName(),
                Arguments = this.ParseArguments(),
                Location = this.GetLocation(start)
            };
        }

        private GraphQLDirectiveDefinition ParseDirectiveDefinition()
        {
            var start = this.CurrentToken.Start;
            this.ExpectKeyword("directive");
            this.Expect(TokenKind.AT);

            var name = this.ParseName();
            var args = this.ParseArgumentDefs();

            this.ExpectKeyword("on");
            var locations = this.ParseDirectiveLocations();

            return new GraphQLDirectiveDefinition()
            {
                Name = name,
                Arguments = args,
                Locations = locations,
                Location = this.GetLocation(start)
            };
        }

        private IEnumerable<GraphQLName> ParseDirectiveLocations()
        {
            var locations = new List<GraphQLName>();
            do
            {
                locations.Add(this.ParseName());
            } while (this.Skip(TokenKind.PIPE));

            return locations;
        }

        private IEnumerable<GraphQLDirective> ParseDirectives()
        {
            var directives = new List<GraphQLDirective>();
            while (this.Peek(TokenKind.AT))
                directives.Add(this.ParseDirective());

            return directives;
        }

        private GraphQLDocument ParseDocument()
        {
            int start = this.CurrentToken.Start;
            List<ASTNode> definitions = new List<ASTNode>();

            do
            {
                definitions.Add(this.ParseDefinition());
            } while (!this.Skip(TokenKind.EOF));

            return CreateDocument(start, definitions);
        }

        private GraphQLEnumTypeDefinition ParseEnumTypeDefinition()
        {
            var start = this.CurrentToken.Start;
            this.ExpectKeyword("enum");
            var name = this.ParseName();
            var directives = this.ParseDirectives();
            var values = this.Many(TokenKind.BRACE_L, () => this.ParseEnumValueDefinition(), TokenKind.BRACE_R);

            return new GraphQLEnumTypeDefinition()
            {
                Name = name,
                Directives = directives,
                Values = values,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLValue ParseEnumValue(Token token)
        {
            this.Advance();
            return new GraphQLValue<string>(ASTNodeKind.EnumValue)
            {
                Value = token.Value.ToString(),
                Location = this.GetLocation(token.Start)
            };
        }

        private GraphQLEnumValueDefinition ParseEnumValueDefinition()
        {
            var start = this.CurrentToken.Start;
            var name = this.ParseName();
            var directives = this.ParseDirectives();

            return new GraphQLEnumValueDefinition()
            {
                Name = name,
                Directives = directives,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLFieldDefinition ParseFieldDefinition()
        {
            var start = this.CurrentToken.Start;
            var name = this.ParseName();
            var args = this.ParseArgumentDefs();
            this.Expect(TokenKind.COLON);
            var type = this.ParseType();
            var directives = this.ParseDirectives();

            return new GraphQLFieldDefinition()
            {
                Name = name,
                Arguments = args,
                Type = type,
                Directives = directives,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLFieldSelection ParseFieldSelection()
        {
            var start = this.CurrentToken.Start;
            var nameOrAlias = this.ParseName();
            GraphQLName name = null;
            GraphQLName alias = null;

            if (this.Skip(TokenKind.COLON))
            {
                name = this.ParseName();
                alias = nameOrAlias;
            }
            else
            {
                alias = null;
                name = nameOrAlias;
            }

            return new GraphQLFieldSelection()
            {
                Alias = alias,
                Name = name,
                Arguments = this.ParseArguments(),
                Directives = this.ParseDirectives(),
                SelectionSet = this.Peek(TokenKind.BRACE_L) ? this.ParseSelectionSet() : null,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLValue ParseFloat(bool isConstant)
        {
            var token = this.CurrentToken;
            this.Advance();
            return new GraphQLValue<float>(ASTNodeKind.IntValue)
            {
                Value = (float)token.Value,
                Location = this.GetLocation(token.Start)
            };
        }

        private ASTNode ParseFragment()
        {
            var start = this.CurrentToken.Start;
            this.Expect(TokenKind.SPREAD);

            if (this.Peek(TokenKind.NAME) && !this.CurrentToken.Value.Equals("on"))
            {
                return new GraphQLFragmentSpread()
                {
                    Name = this.ParseFragmentName(),
                    Directives = this.ParseDirectives(),
                    Location = this.GetLocation(start)
                };
            }

            GraphQLNamedType typeCondition = null;
            if (this.CurrentToken.Value != null && this.CurrentToken.Value.Equals("on"))
            {
                this.Advance();
                typeCondition = this.ParseNamedType();
            }

            return new GraphQLInlineFragment()
            {
                TypeCondition = typeCondition,
                Directives = this.ParseDirectives(),
                SelectionSet = this.ParseSelectionSet(),
                Location = this.GetLocation(start)
            };
        }

        private GraphQLFragmentDefinition ParseFragmentDefinition()
        {
            var start = this.CurrentToken.Start;
            this.ExpectKeyword("fragment");

            return new GraphQLFragmentDefinition()
            {
                Name = this.ParseFragmentName(),
                TypeCondition = this.ExpectOnKeywordAndParseNamedType(),
                Directives = this.ParseDirectives(),
                SelectionSet = this.ParseSelectionSet(),
                Location = this.GetLocation(start)
            };
        }

        private GraphQLName ParseFragmentName()
        {
            if (this.CurrentToken.Value.Equals("on"))
            {
                throw new NotImplementedException();
            }
            return this.ParseName();
        }

        private IEnumerable<GraphQLNamedType> ParseImplementsInterfaces()
        {
            var types = new List<GraphQLNamedType>();
            if (this.CurrentToken.Value?.Equals("implements") == true)
            {
                this.Advance();
                do
                {
                    types.Add(this.ParseNamedType());
                } while (this.Peek(TokenKind.NAME));
            }
            return types;
        }

        private GraphQLInputObjectTypeDefinition ParseInputObjectTypeDefinition()
        {
            var start = this.CurrentToken.Start;
            this.ExpectKeyword("input");
            var name = this.ParseName();
            var directives = this.ParseDirectives();
            var fields = this.Any(TokenKind.BRACE_L, () => this.ParseInputValueDef(), TokenKind.BRACE_R);

            return new GraphQLInputObjectTypeDefinition()
            {
                Name = name,
                Directives = directives,
                Fields = fields,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLInputValueDefinition ParseInputValueDef()
        {
            var start = this.CurrentToken.Start;
            var name = this.ParseName();
            this.Expect(TokenKind.COLON);
            var type = this.ParseType();

            GraphQLValue defaultValue = null;
            if (this.Skip(TokenKind.EQUALS))
            {
                defaultValue = this.ParseConstantValue();
            }

            var directives = this.ParseDirectives();

            return new GraphQLInputValueDefinition()
            {
                Name = name,
                Type = type,
                DefaultValue = defaultValue,
                Directives = directives,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLValue ParseInt(bool isConstant)
        {
            var token = this.CurrentToken;
            this.Advance();
            return new GraphQLValue<int>(ASTNodeKind.IntValue)
            {
                Value = (int)token.Value,
                Location = this.GetLocation(token.Start)
            };
        }

        private GraphQLInterfaceTypeDefinition ParseInterfaceTypeDefinition()
        {
            var start = this.CurrentToken.Start;
            this.ExpectKeyword("interface");
            var name = this.ParseName();
            var directives = this.ParseDirectives();
            var fields = this.Any(TokenKind.BRACE_L, () => this.ParseFieldDefinition(), TokenKind.BRACE_R);

            return new GraphQLInterfaceTypeDefinition()
            {
                Name = name,
                Directives = directives,
                Fields = fields,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLValue ParseList(bool isConstant)
        {
            var start = this.CurrentToken.Start;
            Func<GraphQLValue> constant = () => this.ParseConstantValue();
            Func<GraphQLValue> value = () => this.ParseValueValue();
            Func<GraphQLValue> parseFunction = isConstant ? constant : value;

            return new GraphQLValue<IEnumerable<GraphQLValue>>(ASTNodeKind.ListValue)
            {
                Value = this.Any(TokenKind.BRACKET_L, parseFunction, TokenKind.BRACKET_R),
                Location = this.GetLocation(start)
            };
        }

        private GraphQLName ParseName()
        {
            int start = this.CurrentToken.Start;
            var value = this.CurrentToken.Value;

            this.Expect(TokenKind.NAME);

            return new GraphQLName()
            {
                Location = this.GetLocation(start),
                Value = value as string
            };
        }

        private ASTNode ParseNamedDefinition()
        {
            switch (this.CurrentToken.Value as string)
            {
                // Note: subscription is an experimental non-spec addition.
                case "query":
                case "mutation":
                case "subscription":
                    return this.ParseOperationDefinition();

                case "fragment": return this.ParseFragmentDefinition();

                // Note: the Type System IDL is an experimental non-spec addition.
                case "schema":
                case "scalar":
                case "type":
                case "interface":
                case "union":
                case "enum":
                case "input":
                case "extend":
                case "directive": return this.ParseTypeSystemDefinition();
            }

            return null;
        }

        private GraphQLNamedType ParseNamedType()
        {
            var start = this.CurrentToken.Start;
            return new GraphQLNamedType()
            {
                Name = this.ParseName(),
                Location = this.GetLocation(start)
            };
        }

        private GraphQLValue ParseNameValue(bool isConstant)
        {
            var token = this.CurrentToken;

            if (token.Value.Equals("true") || token.Value.Equals("false"))
            {
                return this.ParseBooleanValue(token);
            }
            else if (token.Value != null)
            {
                return this.ParseEnumValue(token);
            }

            throw new NotImplementedException();
        }

        private GraphQLValue ParseObject(bool isConstant)
        {
            var start = this.CurrentToken.Start;
            this.Expect(TokenKind.BRACE_L);
            List<GraphQLObjectField> fields = new List<GraphQLObjectField>();

            while (!this.Skip(TokenKind.BRACE_R))
                fields.Add(this.ParseObjectField(isConstant));

            return new GraphQLObjectValue()
            {
                Fields = fields,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLObjectField ParseObjectField(bool isConstant)
        {
            var start = this.CurrentToken.Start;
            return new GraphQLObjectField()
            {
                Name = this.ParseName(),
                Value = this.AdvanceAndParseValue(),
                Location = this.GetLocation(start)
            };
        }

        private GraphQLObjectTypeDefinition ParseObjectTypeDefinition()
        {
            var start = this.CurrentToken.Start;
            this.ExpectKeyword("type");
            var name = this.ParseName();
            var interfaces = this.ParseImplementsInterfaces();
            var directives = this.ParseDirectives();
            var fields = this.Any(TokenKind.BRACE_L, () => this.ParseFieldDefinition(), TokenKind.BRACE_R);

            return new GraphQLObjectTypeDefinition()
            {
                Name = name,
                Interfaces = interfaces,
                Directives = directives,
                Fields = fields,
                Location = this.GetLocation(start)
            };
        }

        private ASTNode ParseOperationDefinition()
        {
            var start = this.CurrentToken.Start;

            if (this.Peek(TokenKind.BRACE_L))
            {
                return this.CreateOperationDefinition(start);
            }

            return this.CreateOperationDefinition(start, this.ParseOperationType(), this.GetName());
        }

        private OperationType ParseOperationType()
        {
            var token = this.CurrentToken;
            this.Expect(TokenKind.NAME);

            switch (token.Value as string)
            {
                case "query": return OperationType.Query;
                case "mutation": return OperationType.Mutation;
                case "subscription": return OperationType.Subscription;
            }

            throw new NotImplementedException();
        }

        private GraphQLOperationTypeDefinition ParseOperationTypeDefinition()
        {
            var start = this.CurrentToken.Start;
            var operation = this.ParseOperationType();
            this.Expect(TokenKind.COLON);
            var type = this.ParseNamedType();

            return new GraphQLOperationTypeDefinition()
            {
                Operation = operation,
                Type = type,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLScalarTypeDefinition ParseScalarTypeDefinition()
        {
            var start = this.CurrentToken.Start;
            this.ExpectKeyword("scalar");
            var name = this.ParseName();
            var directives = this.ParseDirectives();

            return new GraphQLScalarTypeDefinition()
            {
                Name = name,
                Directives = directives,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLSchemaDefinition ParseSchemaDefinition()
        {
            var start = this.CurrentToken.Start;
            this.ExpectKeyword("schema");
            var directives = this.ParseDirectives();
            var operationTypes = this.Many(TokenKind.BRACE_L, () => this.ParseOperationTypeDefinition(), TokenKind.BRACE_R);

            return new GraphQLSchemaDefinition()
            {
                Directives = directives,
                OperationTypes = operationTypes,
                Location = this.GetLocation(start)
            };
        }

        private ASTNode ParseSelection()
        {
            return this.Peek(TokenKind.SPREAD) ?
                this.ParseFragment() :
                this.ParseFieldSelection();
        }

        private GraphQLSelectionSet ParseSelectionSet()
        {
            var start = this.CurrentToken.Start;
            return new GraphQLSelectionSet()
            {
                Selections = this.Many(TokenKind.BRACE_L, () => this.ParseSelection(), TokenKind.BRACE_R),
                Location = this.GetLocation(start)
            };
        }

        private GraphQLValue ParseString(bool isConstant)
        {
            var token = this.CurrentToken;
            this.Advance();
            return new GraphQLValue<string>(ASTNodeKind.StringValue)
            {
                Value = token.Value as string,
                Location = this.GetLocation(token.Start)
            };
        }

        private GraphQLType ParseType()
        {
            GraphQLType type = null;
            var start = this.CurrentToken.Start;
            if (this.Skip(TokenKind.BRACKET_L))
            {
                type = this.ParseType();
                this.Expect(TokenKind.BRACKET_R);
                type = new GraphQLListType()
                {
                    Type = type,
                    Location = this.GetLocation(start)
                };
            }
            else
            {
                type = this.ParseNamedType();
            }

            if (this.Skip(TokenKind.BANG))
            {
                return new GraphQLNonNullType()
                {
                    Type = type,
                    Location = this.GetLocation(start)
                };
            }

            return type;
        }

        private GraphQLTypeExtensionDefinition ParseTypeExtensionDefinition()
        {
            var start = this.CurrentToken.Start;
            this.ExpectKeyword("extend");
            var definition = this.ParseObjectTypeDefinition();

            return new GraphQLTypeExtensionDefinition()
            {
                Definition = definition,
                Location = this.GetLocation(start)
            };
        }

        private ASTNode ParseTypeSystemDefinition()
        {
            if (this.Peek(TokenKind.NAME))
            {
                switch (this.CurrentToken.Value as string)
                {
                    case "schema": return this.ParseSchemaDefinition();
                    case "scalar": return this.ParseScalarTypeDefinition();
                    case "type": return this.ParseObjectTypeDefinition();
                    case "interface": return this.ParseInterfaceTypeDefinition();
                    case "union": return this.ParseUnionTypeDefinition();
                    case "enum": return this.ParseEnumTypeDefinition();
                    case "input": return this.ParseInputObjectTypeDefinition();
                    case "extend": return this.ParseTypeExtensionDefinition();
                    case "directive": return this.ParseDirectiveDefinition();
                }
            }

            throw new NotImplementedException();
        }

        private IEnumerable<GraphQLNamedType> ParseUnionMembers()
        {
            var members = new List<GraphQLNamedType>();
            do
            {
                members.Add(this.ParseNamedType());
            } while (this.Skip(TokenKind.PIPE));

            return members;
        }

        private GraphQLUnionTypeDefinition ParseUnionTypeDefinition()
        {
            var start = this.CurrentToken.Start;
            this.ExpectKeyword("union");
            var name = this.ParseName();
            var directives = this.ParseDirectives();
            this.Expect(TokenKind.EQUALS);
            var types = this.ParseUnionMembers();

            return new GraphQLUnionTypeDefinition()
            {
                Name = name,
                Directives = directives,
                Types = types,
                Location = this.GetLocation(start)
            };
        }

        private GraphQLValue ParseValueLiteral(bool isConstant)
        {
            var token = this.CurrentToken;

            switch (token.Kind)
            {
                case TokenKind.BRACKET_L: return this.ParseList(isConstant);
                case TokenKind.BRACE_L: return this.ParseObject(isConstant);
                case TokenKind.INT: return this.ParseInt(isConstant);
                case TokenKind.FLOAT: return this.ParseFloat(isConstant);
                case TokenKind.STRING: return this.ParseString(isConstant);
                case TokenKind.NAME: return this.ParseNameValue(isConstant);
                case TokenKind.DOLLAR: if (!isConstant) return this.ParseVariable(); break;
            }

            throw new NotImplementedException();
        }

        private GraphQLValue ParseValueValue()
        {
            return this.ParseValueLiteral(false);
        }

        private GraphQLVariable ParseVariable()
        {
            var start = this.CurrentToken.Start;
            this.Expect(TokenKind.DOLLAR);

            return new GraphQLVariable()
            {
                Name = this.GetName(),
                Location = this.GetLocation(start)
            };
        }

        private GraphQLVariableDefinition ParseVariableDefinition()
        {
            int start = this.CurrentToken.Start;
            return new GraphQLVariableDefinition()
            {
                Variable = this.ParseVariable(),
                Type = this.AdvanceThroughColonAndParseType(),
                DefaultValue = this.SkipEqualsAndParseValueLiteral()
            };
        }

        private IEnumerable<GraphQLVariableDefinition> ParseVariableDefinitions()
        {
            return this.Peek(TokenKind.PAREN_L) ?
                this.Many(
                  TokenKind.PAREN_L,
                  () => this.ParseVariableDefinition(),
                  TokenKind.PAREN_R
                ) : new GraphQLVariableDefinition[] { };
        }

        private bool Peek(TokenKind kind)
        {
            return this.CurrentToken.Kind == kind;
        }

        private bool Skip(TokenKind kind)
        {
            var match = this.CurrentToken.Kind == kind;
            if (match)
            {
                this.Advance();
            }
            return match;
        }

        private object SkipEqualsAndParseValueLiteral()
        {
            return this.Skip(TokenKind.EQUALS) ? this.ParseValueLiteral(true) : null;
        }
    }
}