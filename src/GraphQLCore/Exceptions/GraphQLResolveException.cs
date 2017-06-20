namespace GraphQLCore.Exceptions
{
    using System;

    public class GraphQLResolveException : Exception
    {
        internal GraphQLResolveException(string message) : base(message)
        {
        }
    }
}
