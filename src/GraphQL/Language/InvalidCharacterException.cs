using System;

namespace GraphQL.Language
{
    public class InvalidCharacterException : Exception
    {
        public InvalidCharacterException(string message) : base(message)
        {
        }
    }
}