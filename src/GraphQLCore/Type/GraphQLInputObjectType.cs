namespace GraphQLCore.Type
{
    using Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLInputObjectType : GraphQLComplexType
    {
        public GraphQLInputObjectType(string name, string description) : base(name, description)
        {
        }
    }
}