namespace GraphQLCore.Type
{
    using Complex;
    using System;

    public class GraphQLEnumType<T> : GraphQLEnumType
        where T : struct, IConvertible
    {
        public GraphQLEnumType(string name, string description) : base(name, description, typeof(T))
        {
        }

        protected EnumValueDefinitionBuilder EnumValue(T value)
        {
            return new EnumValueDefinitionBuilder(this.Values[value.ToString()]);
        }
    }
}