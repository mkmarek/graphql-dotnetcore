using GraphQLCore.Exceptions;
using System;
using System.Globalization;

namespace GraphQLCore.Language
{
    public class LexerContext : IDisposable
    {
        private int currentIndex;
        private ISource source;

        public LexerContext(ISource source, int index)
        {
            this.currentIndex = index;
            this.source = source;
        }

        public void Dispose()
        {
        }

        public Token GetToken()
        {
            if (this.source.Body == null)
                return CreateEOFToken();

            this.currentIndex = GetPositionAfterWhitespace(this.source.Body, this.currentIndex);

            if (this.currentIndex >= this.source.Body.Length)
                return CreateEOFToken();

            var code = this.source.Body[this.currentIndex];

            this.ValidateCharacterCode(code);

            var token = CheckForPunctuationTokens(code);
            if (token != null)
                return token;

            if (char.IsLetter(code) || code == '_')
                return ReadName();

            if (char.IsNumber(code) || code == '-')
                return ReadNumber();

            if (code == '"')
                return ReadString();

            throw new NotImplementedException();
        }

        public Token ReadNumber()
        {
            var isFloat = false;
            var start = this.currentIndex;
            var code = this.source.Body[start];

            if (code == '-')
                code = NextCode();

            code = code == '0'
                ? NextCode()
                : ReadDigitsFromOwnSource(code);

            if (code == '.')
            {
                isFloat = true;
                code = ReadDigitsFromOwnSource(NextCode());
            }

            if (code == 'E' || code == 'e')
            {
                isFloat = true;
                code = NextCode();
                if (code == '+' || code == '-')
                {
                    code = NextCode();
                }
                code = ReadDigitsFromOwnSource(code);
            }

            return isFloat ? CreateFloatToken(start) : CreateIntToken(start);
        }

        public Token ReadString()
        {
            var start = this.currentIndex;
            var value = ProcessStringChunks();

            return new Token()
            {
                Kind = TokenKind.STRING,
                Value = value,
                Start = start,
                End = this.currentIndex + 1
            };
        }

        private void CheckStringTermination(char code)
        {
            if (code != '"')
            {
                var location = new Location(this.source, this.currentIndex);
                throw new GraphQLException($"Syntax Error GraphQL ({location.Line}:{location.Column}) Unterminated string.");
            }
        }

        private static bool IsValidNameCharacter(char code)
        {
            return code == '_' || char.IsLetterOrDigit(code);
        }

        private string AppendCharactersFromLastChunk(string value, int chunkStart)
        {
            return value + this.source.Body.Substring(chunkStart, this.currentIndex - chunkStart - 1);
        }

        private string AppendToValueByCode(string value, char code)
        {
            switch (code)
            {
                case '"': value += '"'; break;
                case '/': value += '/'; break;
                case '\\': value += '\\'; break;
                case 'b': value += '\b'; break;
                case 'f': value += '\f'; break;
                case 'n': value += '\n'; break;
                case 'r': value += '\r'; break;
                case 't': value += '\t'; break;
                case 'u': value += this.GetUnicodeChar(); break;
                default:
                    var location = new Location(this.source, this.currentIndex);
                    throw new GraphQLException($"Syntax Error GraphQL ({location.Line}:{location.Column}) Invalid character escape sequence: \\{code}.");
            }
            return value;
        }

        private int CharToHex(char code)
        {
            return Convert.ToByte(code.ToString(), 16);
        }

        private Token CheckForPunctuationTokens(char code)
        {
            switch (code)
            {
                case '!': return this.CreatePunctuationToken(TokenKind.BANG, 1);
                case '$': return this.CreatePunctuationToken(TokenKind.DOLLAR, 1);
                case '(': return this.CreatePunctuationToken(TokenKind.PAREN_L, 1);
                case ')': return this.CreatePunctuationToken(TokenKind.PAREN_R, 1);
                case '.': return this.CheckForSpreadOperator();
                case ':': return this.CreatePunctuationToken(TokenKind.COLON, 1);
                case '=': return this.CreatePunctuationToken(TokenKind.EQUALS, 1);
                case '@': return this.CreatePunctuationToken(TokenKind.AT, 1);
                case '[': return this.CreatePunctuationToken(TokenKind.BRACKET_L, 1);
                case ']': return this.CreatePunctuationToken(TokenKind.BRACKET_R, 1);
                case '{': return this.CreatePunctuationToken(TokenKind.BRACE_L, 1);
                case '|': return this.CreatePunctuationToken(TokenKind.PIPE, 1);
                case '}': return this.CreatePunctuationToken(TokenKind.BRACE_R, 1);
                default: return null;
            }
        }

        private Token CheckForSpreadOperator()
        {
            if (this.source.Body[this.currentIndex + 1] == '.' && this.source.Body[this.currentIndex + 2] == '.')
            {
                return this.CreatePunctuationToken(TokenKind.SPREAD, 3);
            }
            return null;
        }

        private Token CreateEOFToken()
        {
            return new Token()
            {
                Start = this.currentIndex,
                End = this.currentIndex,
                Kind = TokenKind.EOF
            };
        }

        private Token CreateFloatToken(int start)
        {
            return new Token()
            {
                Kind = TokenKind.FLOAT,
                Start = start,
                End = this.currentIndex,
                Value = Convert.ToSingle(this.source.Body.Substring(start, this.currentIndex - start), CultureInfo.InvariantCulture)
            };
        }

        private Token CreateIntToken(int start)
        {
            return new Token()
            {
                Kind = TokenKind.INT,
                Start = start,
                End = this.currentIndex,
                Value = Convert.ToInt32(this.source.Body.Substring(start, this.currentIndex - start), CultureInfo.InvariantCulture)
            };
        }

        private Token CreateNameToken(int start)
        {
            return new Token()
            {
                Start = start,
                End = this.currentIndex,
                Kind = TokenKind.NAME,
                Value = this.source.Body.Substring(start, this.currentIndex - start)
            };
        }

        private Token CreatePunctuationToken(TokenKind kind, int offset)
        {
            return new Token()
            {
                Start = this.currentIndex,
                End = this.currentIndex + offset,
                Kind = kind,
                Value = null
            };
        }

        private char GetCode()
        {
            return this.currentIndex < this.source.Body.Length
                ? this.source.Body[this.currentIndex]
                : (char)0;
        }

        private int GetPositionAfterWhitespace(string body, int start)
        {
            var position = start;

            while (position < body.Length)
            {
                var code = body[position];
                switch (code)
                {
                    case '\xFEFF': // BOM
                    case '\t': // tab
                    case ' ': // space
                    case '\n': // new line
                    case '\r': // carriage return
                    case ',': // Comma
                        ++position;
                        break;

                    case '#':
                        position = this.WaitForEndOfComment(body, position, code);
                        break;

                    default:
                        return position;
                }
            }

            return position;
        }

        public bool OnlyHexInString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        private char GetUnicodeChar()
        {
            var expression = this.source.Body.Substring(this.currentIndex,5);

            if (!this.OnlyHexInString(expression.Substring(1)))
            {
                var location = new Location(this.source, this.currentIndex);
                throw new GraphQLException($"Syntax Error GraphQL ({location.Line}:{location.Column}) Invalid character escape sequence: \\{expression}.");
            }

            var character = (char)(
                CharToHex(NextCode()) << 12 |
                CharToHex(NextCode()) << 8 |
                CharToHex(NextCode()) << 4 |
                CharToHex(NextCode()));

            return character;
        }

        private char NextCode()
        {
            return ++this.currentIndex < this.source.Body.Length
                ? this.source.Body[this.currentIndex]
                : (char)0;
        }

        private char ProcessCharacter(ref string value, ref int chunkStart)
        {
            var code = GetCode();
            ++this.currentIndex;

            if (code == '\\')
            {
                value = AppendToValueByCode(AppendCharactersFromLastChunk(value, chunkStart), GetCode());

                ++this.currentIndex;
                chunkStart = this.currentIndex;
            }

            return GetCode();
        }

        private string ProcessStringChunks()
        {
            var chunkStart = ++this.currentIndex;
            var code = GetCode();
            var value = "";

            while (this.currentIndex < this.source.Body.Length && code != 0x000A && code != 0x000D && code != '"')
            {
                CheckForInvalidCharacters(code);
                code = ProcessCharacter(ref value, ref chunkStart);
            }

            CheckStringTermination(code);
            value += this.source.Body.Substring(chunkStart, this.currentIndex - chunkStart);
            return value;
        }

        private void CheckForInvalidCharacters(char code)
        {
            if (code < 0x0020 && code != 0x0009)
            {
                var location = new Location(this.source, this.currentIndex);
                throw new GraphQLException($"Syntax Error GraphQL ({location.Line}:{location.Column}) Invalid character within String: \\u{((int)code).ToString("D4")}.");
            }
        }

        private int ReadDigits(ISource source, int start, char firstCode)
        {
            var body = source.Body;
            var position = start;
            var code = firstCode;

            if (!char.IsNumber(code)) throw new NotImplementedException();
            do
            {
                code = ++position < body.Length
                    ? body[position]
                    : (char)0;
            } while (char.IsNumber(code));

            return position;
        }

        private char ReadDigitsFromOwnSource(char code)
        {
            this.currentIndex = ReadDigits(source, this.currentIndex, code);
            code = GetCode();
            return code;
        }

        private Token ReadName()
        {
            var start = this.currentIndex;
            var code = (char)0;

            while (++this.currentIndex != this.source.Body.Length && (code = GetCode()) != 0 && IsValidNameCharacter(code)) { }
            return CreateNameToken(start);
        }

        private void ValidateCharacterCode(int code)
        {
            if (code < 0x0020 && code != 0x0009 && code != 0x000A && code != 0x000D)
            {
                var location = new Location(this.source, this.currentIndex);
                throw new GraphQLException($"Syntax Error GraphQL ({location.Line}:{location.Column}) Invalid character \\u{code.ToString("D4")}.");
            };
        }

        private int WaitForEndOfComment(string body, int position, char code)
        {
            while (++position < body.Length && (code = body[position]) != 0 && (code > 0x001F || code == 0x0009) && code != 0x000A && code != 0x000D)
            {
            }

            return position;
        }
    }
}