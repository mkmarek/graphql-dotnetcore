using System;

namespace GraphQL.Parser.Language
{
    public class InvalidCharacterException : Exception
    {
        public InvalidCharacterException(string message) : base(message)
        {
        }
    }
}