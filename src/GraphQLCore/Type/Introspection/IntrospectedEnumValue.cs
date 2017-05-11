namespace GraphQLCore.Type.Introspection
{
    using System;
    using System.Linq;
    using Utils;

    public class IntrospectedEnumValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeprecated { get; set; }
        public string DeprecationReason { get; set; }

        public static IntrospectedEnumValue[] GetEnumValuesFor(Type type)
        {
            if (!ReflectionUtilities.IsEnum(type))
                throw new ArgumentException("T must be an enum type");

            return Enum.GetNames(type).Select(e => new IntrospectedEnumValue()
            {
                Name = e
            }).ToArray();
        }
    }
}