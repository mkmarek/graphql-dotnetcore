using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLCore.Exceptions
{
    public class GraphQLException : Exception
    {
        public GraphQLException(string message) : base(message)
        {

        }

        public GraphQLException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
