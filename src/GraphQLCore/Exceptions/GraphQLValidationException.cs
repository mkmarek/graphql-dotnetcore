namespace GraphQLCore.Exceptions
{
    using System;
    using System.Collections.Generic;

    public class GraphQLValidationException : Exception
    {
        public IEnumerable<GraphQLException> Errors { get; private set; }

        public GraphQLValidationException(string message, IEnumerable<GraphQLException> errors) : base(message)
        {
            this.Errors = errors;
        }
    }
}