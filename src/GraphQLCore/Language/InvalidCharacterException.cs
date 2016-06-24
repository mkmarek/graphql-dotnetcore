using System;

namespace GraphQLCore.Language
{
    public class InvalidCharacterException : Exception
    {
        public InvalidCharacterException(string message) : base(message)
        {
        }
    }
}