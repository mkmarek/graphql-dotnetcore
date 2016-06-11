using System;
using System.Linq;

namespace GraphQL.Parser.Language
{
    public class LexerContext : IDisposable
    {
        private static readonly int[] NameCharacters = new int[] { 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 95, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122 };
        private static readonly int[] NumberCharacters = new int[] { 45, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57 };
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
            this.CurrentIndex = this.GetPositionAfterWhitespace(this.Source.Body, this.CurrentIndex);

            if (this.CurrentIndex >= this.Source.Body.Length)
                return this.CreateEOFToken();

            int code = this.Source.Body[this.CurrentIndex];

            this.ValidateCharacterCode(code);

            var token = this.CheckForPunctuationTokens(code);
            if (token != null)
                return token;

            if (NameCharacters.Contains(code))
                return this.ReadName();

            if (NumberCharacters.Contains(code))
                return this.ReadNumber();

            if (code == 34)
                return this.ReadString();

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

        public Token ReadNumber()
        {
            bool isFloat = false;
            int start = this.CurrentIndex;
            int code = this.Source.Body[start];

            if (code == 45)
                code = this.NextCode();

            if (code == 48)
                code = this.NextCode();
            else
                code = this.ReadDigitsFromOwnSource(code);

            if (code == 46)
            {
                isFloat = true;
                code = this.ReadDigitsFromOwnSource(this.NextCode());
            }

            if (code == 69 || code == 101)
            {
                isFloat = true;
                code = this.NextCode();
                if (code == 43 || code == 45)
                {
                    code = this.NextCode();
                }
                code = this.ReadDigitsFromOwnSource(code);
            }

            return isFloat ? CreateFloatToken(start) : CreateIntToken(start);
        }

        public Token ReadString()
        {
            int start = this.CurrentIndex;
            var value = ProcessStringChunks();

            return new Token()
            {
                Kind = TokenKind.STRING,
                Value = value,
                Start = start,
                End = this.CurrentIndex + 1
            };
        }

        private static void CheckStringTermination(int code)
        {
            if (code != 34)
            {
                throw new InvalidCharacterException($"Unterminated string");
            }
        }

        private static bool IsValidNameCharacter(int code)
        {
            return
                code == 95 || //
                code >= 48 && code <= 57 || // 0-9
                code >= 65 && code <= 90 || // A-Z
                code >= 97 && code <= 122; // a-z
        }

        private string AppendCharactersFromLastChunk(string value, int chunkStart)
        {
            return value + this.Source.Body.Substring(chunkStart, this.CurrentIndex - chunkStart - 1);
        }

        private string AppendToValueByCode(string value, int code)
        {
            switch (code)
            {
                case 34: value += '"'; break;
                case 47: value += '/'; break;
                case 92: value += '\\'; break;
                case 98: value += '\b'; break;
                case 102: value += '\f'; break;
                case 110: value += '\n'; break;
                case 114: value += '\r'; break;
                case 116: value += '\t'; break;
                case 117: value += this.GetUnicodeChar(); break;
            }

            return value;
        }

        private int CharToHex(int code)
        {
            return
                code >= 48 && code <= 57 ? code - 48 : // 0-9
                code >= 65 && code <= 70 ? code - 55 : // code-F
                code >= 97 && code <= 102 ? code - 87 : // code-f
                -1;
        }

        private Token CheckForPunctuationTokens(int code)
        {
            switch (code)
            {
                case 33: return this.CreatePunctuationToken(TokenKind.BANG, 1);
                // $
                case 36: return this.CreatePunctuationToken(TokenKind.DOLLAR, 1);
                // (
                case 40: return this.CreatePunctuationToken(TokenKind.PAREN_L, 1);
                // )
                case 41: return this.CreatePunctuationToken(TokenKind.PAREN_R, 1);
                // .
                case 46: return this.CheckForSpreadOperator();
                // :
                case 58: return this.CreatePunctuationToken(TokenKind.COLON, 1);
                // =
                case 61: return this.CreatePunctuationToken(TokenKind.EQUALS, 1);
                // @
                case 64: return this.CreatePunctuationToken(TokenKind.AT, 1);
                // [
                case 91: return this.CreatePunctuationToken(TokenKind.BRACKET_L, 1);
                // ]
                case 93: return this.CreatePunctuationToken(TokenKind.BRACKET_R, 1);
                // {
                case 123: return this.CreatePunctuationToken(TokenKind.BRACE_L, 1);
                // |
                case 124: return this.CreatePunctuationToken(TokenKind.PIPE, 1);
                // }
                case 125: return this.CreatePunctuationToken(TokenKind.BRACE_R, 1);
            }

            return null;
        }

        private Token CheckForSpreadOperator()
        {
            if (this.Source.Body[this.CurrentIndex + 1] == 46 &&
                       this.Source.Body[this.CurrentIndex + 2] == 46)
            {
                return this.CreatePunctuationToken(TokenKind.SPREAD, 3);
            }

            return null;
        }

        private Token CreateFloatToken(int start)
        {
            return new Token()
            {
                Kind = TokenKind.FLOAT,
                Start = start,
                End = this.CurrentIndex,
                Value = Convert.ToSingle(this.Source.Body.Substring(start, this.CurrentIndex - start))
            };
        }

        private Token CreateIntToken(int start)
        {
            return new Token()
            {
                Kind = TokenKind.INT,
                Start = start,
                End = this.CurrentIndex,
                Value = Convert.ToInt32(this.Source.Body.Substring(start, this.CurrentIndex - start))
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
            return this.CurrentIndex < this.Source.Body.Length ? this.Source.Body[this.CurrentIndex] : (char)0;
        }

        private int GetPositionAfterWhitespace(string body, int start)
        {
            int position = start;

            while (position < body.Length)
            {
                int code = body[position];

                if (
                      code == 0xFEFF || // BOM
                      code == 0x0009 || // tab
                      code == 0x0020 || // space
                      code == 0x000A || // new line
                      code == 0x000D || // carriage return
                      code == 0x002C    // Comma
                    )
                {
                    ++position;
                }
                else if (code == 35) // #
                {
                    position = this.WaitForEndOfComment(body, position, code);
                }
                else
                {
                    break;
                }
            }

            return position;
        }

        private char GetUnicodeChar()
        {
            return (char)(this.CharToHex(this.NextCode()) << 12 |
                this.CharToHex(this.NextCode()) << 8 |
                this.CharToHex(this.NextCode()) << 4 |
                this.CharToHex(this.NextCode()));
        }

        private char NextCode()
        {
            return ++this.CurrentIndex < this.Source.Body.Length ? this.Source.Body[this.CurrentIndex] : (char)0;
        }

        private int ProcessCharacter(ref string value, ref int chunkStart)
        {
            int code = this.GetCode();
            ++this.CurrentIndex;

            if (code == 92)
            {
                value = AppendToValueByCode(this.AppendCharactersFromLastChunk(value, chunkStart), this.GetCode());

                ++this.CurrentIndex;
                chunkStart = this.CurrentIndex;
            }

            return this.GetCode();
        }

        private string ProcessStringChunks()
        {
            int start = this.CurrentIndex;
            int chunkStart = ++this.CurrentIndex;
            int code = this.GetCode();
            string value = "";

            while (this.CurrentIndex < this.Source.Body.Length && code != 0x000A && code != 0x000D && code != 34)
            {
                code = ProcessCharacter(ref value, ref chunkStart);
            }

            CheckStringTermination(code);
            value += this.Source.Body.Substring(chunkStart, this.CurrentIndex - chunkStart);
            return value;
        }

        private int ReadDigits(ISource source, int start, int firstCode)
        {
            var body = source.Body;
            int position = start;
            int code = firstCode;

            if (code >= 48 && code <= 57)
            { // 0 - 9
                do
                {
                    code = ++position < body.Length ? body[position] : 0;
                } while (code >= 48 && code <= 57); // 0 - 9

                return position;
            }

            throw new NotImplementedException();
        }

        private int ReadDigitsFromOwnSource(int code)
        {
            this.CurrentIndex = this.ReadDigits(this.Source, this.CurrentIndex, code);
            code = GetCode();
            return code;
        }

        private Token ReadName()
        {
            var start = this.CurrentIndex;
            int code = 0;

            while (++this.CurrentIndex != this.Source.Body.Length && (code = this.GetCode()) != 0 && IsValidNameCharacter(code)) { }
            return CreateNameToken(start);
        }

        private void ValidateCharacterCode(int code)
        {
            if (code < 0x0020 && code != 0x0009 && code != 0x000A && code != 0x000D)
            {
                throw new InvalidCharacterException($"Invalid character \"\\u{code.ToString("D4")}\"");
            };
        }

        private int WaitForEndOfComment(string body, int position, int code)
        {
            while (++position < body.Length && (code = body[position]) != 0 && (code > 0x001F || code == 0x0009) && code != 0x000A && code != 0x000D)
            {
            }

            return position;
        }
    }
}