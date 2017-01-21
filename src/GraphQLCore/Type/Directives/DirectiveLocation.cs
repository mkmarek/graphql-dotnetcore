namespace GraphQLCore.Type.Directives
{
    using System;

    [Flags]
    public enum DirectiveLocation
    {
        // Operations
        QUERY = 1,
        MUTATION = 2,
        SUBSCRIPTION = 4,
        FIELD = 8,
        FRAGMENT_DEFINITION = 16,
        FRAGMENT_SPREAD = 32,
        INLINE_FRAGMENT = 64,

        // Schema Definitions
        SCHEMA = 128,
        SCALAR = 256,
        OBJECT = 512,
        FIELD_DEFINITION = 1024,
        ARGUMENT_DEFINITION = 2048,
        INTERFACE = 4096,
        UNION = 8192,
        ENUM = 16384,
        ENUM_VALUE = 32768,
        INPUT_OBJECT = 65536,
        INPUT_FIELD_DEFINITION = 131072,
    }
}