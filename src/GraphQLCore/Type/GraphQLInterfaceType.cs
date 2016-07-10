namespace GraphQLCore.Type
{
    using Exceptions;
    using System;
    using System.Reflection;

    public abstract class GraphQLInterfaceType : GraphQLComplexType
    {
        private Type interfaceType;

        public GraphQLInterfaceType(string name, string description, Type interfaceType) : base(name, description)
        {
            if (!interfaceType.GetTypeInfo().IsInterface)
                throw new GraphQLException($"Type {interfaceType.FullName} has to be an interface type");

            this.interfaceType = interfaceType;
        }
    }
}