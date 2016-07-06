namespace GraphQLCore.Type
{
    using System;

    public class GraphQLEnumType : GraphQLNullableType
    {
        public GraphQLEnumType(string name, string description, Type enumType) : base(name, description)
        {
            this.EnumType = enumType;
        }

        public Type EnumType { get; set; }
    }
}