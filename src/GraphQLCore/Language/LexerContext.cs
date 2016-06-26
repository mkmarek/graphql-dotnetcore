using System;
using System.Globalization;

namespace GraphQLCore.Language
{
    public class LexerContext : IDisposable
    {
        private int CurrentIndex;
        private ISource Source;

        public LexerContext(ISource source, int index)
        {
            this.CurrentIndex = index;
            this.Source = source;
        }

        public void Dispose()
        {
        }

        public Token GetToken()
        {
            if (this.Source.Body == null)
                return CreateEOFToken();

            this.CurrentIndex = GetPositionAfterWhitespace(this.Source.Body, this.CurrentIndex);

            if (this.CurrentIndex >= this.Source.Body.Length)
                return CreateEOFToken();

            var code = this.Source.Body[this.CurrentIndex];

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
            var start = this.CurrentIndex;
            var code = this.Source.Body[start];

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
            var start = this.CurrentIndex;
            var value = ProcessStringChunks();

            return new Token()
            {
                Kind = TokenKind.STRING,
                Value = value,
                Start = start,
                End = this.CurrentIndex + 1
            };
        }

        private static void CheckStringTermination(char code)
        {
            if (code != '"')
            {
                throw new InvalidCharacterException($"Unterminated string");
            }
        }

        private static bool IsValidNameCharacter(char code)
        {
            return code == '_' || char.IsLetterOrDigit(code);
        }

        private string AppendCharactersFromLastChunk(string value, int chunkStart)
        {
            return value + this.Source.Body.Substring(chunkStart, this.CurrentIndex - chunkStart - 1);
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
                default: return value;
            }
            return value;
        }

        private byte CharToHex(char code)
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
            if (this.Source.Body[this.CurrentIndex + 1] == '.' && this.Source.Body[this.CurrentIndex + 2] == '.')
            {
                return this.CreatePunctuationToken(TokenKind.SPREAD, 3);
            }
            return null;
        }

        private Token CreateEOFToken()
        {
            return new Token()
            {
                Start = this.CurrentIndex,
                End = this.CurrentIndex,
                Kind = TokenKind.EOF
            };
        }

        private Token CreateFloatToken(int start)
        {
            return new Token()
            {
                Kind = TokenKind.FLOAT,
                Start = start,
                End = this.CurrentIndex,
                Value = Convert.ToSingle(this.Source.Body.Substring(start, this.CurrentIndex - start), CultureInfo.InvariantCulture)
            };
        }

        private Token CreateIntToken(int start)
        {
            return new Token()
            {
                Kind = TokenKind.INT,
                Start = start,
                End = this.CurrentIndex,
                Value = Convert.ToInt32(this.Source.Body.Substring(start, this.CurrentIndex - start), CultureInfo.InvariantCulture)
            };
        }

        private Token CreateNameToken(int start)
        {
            return new Token()
            {
                Start = start,
                End = this.CurrentIndex,
                Kind = TokenKind.NAME,
                Value = this.Source.Body.Substring(start, this.CurrentIndex - start)
            };
        }

        private Token CreatePunctuationToken(TokenKind kind, int offset)
        {
            return new Token()
            {
                Start = this.CurrentIndex,
                End = this.CurrentIndex + offset,
                Kind = kind,
                Value = null
            };
        }

        private char GetCode()
        {
            return this.CurrentIndex < this.Source.Body.Length
                ? this.Source.Body[this.CurrentIndex]
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

        private char GetUnicodeChar()
        {
            return (char)(
                CharToHex(NextCode()) << 12 |
                CharToHex(NextCode()) << 8 |
                CharToHex(NextCode()) << 4 |
                CharToHex(NextCode()));
        }

        private char NextCode()
        {
            return ++this.CurrentIndex < this.Source.Body.Length
                ? this.Source.Body[this.CurrentIndex]
                : (char)0;
        }

        private char ProcessCharacter(ref string value, ref int chunkStart)
        {
            var code = GetCode();
            ++this.CurrentIndex;

            if (code == '\\')
            {
                value = AppendToValueByCode(AppendCharactersFromLastChunk(value, chunkStart), GetCode());

                ++this.CurrentIndex;
                chunkStart = this.CurrentIndex;
            }

            return GetCode();
        }

        private string ProcessStringChunks()
        {
            var chunkStart = ++this.CurrentIndex;
            var code = GetCode();
            var value = "";

            while (this.CurrentIndex < this.Source.Body.Length && code != 0x000A && code != 0x000D && code != '"')
            {
                code = ProcessCharacter(ref value, ref chunkStart);
            }

            CheckStringTermination(code);
            value += this.Source.Body.Substring(chunkStart, this.CurrentIndex - chunkStart);
            return value;
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
            this.CurrentIndex = ReadDigits(Source, this.CurrentIndex, code);
            code = GetCode();
            return code;
        }

        private Token ReadName()
        {
            var start = this.CurrentIndex;
            var code = (char)0;

            while (++this.CurrentIndex != this.Source.Body.Length && (code = GetCode()) != 0 && IsValidNameCharacter(code)) { }
            return CreateNameToken(start);
        }

        private void ValidateCharacterCode(int code)
        {
            if (code < 0x0020 && code != 0x0009 && code != 0x000A && code != 0x000D)
            {
                throw new InvalidCharacterException($"Invalid character \"\\u{code.ToString("D4")}\"");
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