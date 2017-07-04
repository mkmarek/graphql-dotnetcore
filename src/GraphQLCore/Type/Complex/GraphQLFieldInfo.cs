namespace GraphQLCore.Type.Complex
{
    using System;
    using System.Collections.Generic;
    using Translation;

    public abstract class GraphQLFieldInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeprecated { get; set; }
        public string DeprecationReason { get; set; }

        public IDictionary<string, GraphQLObjectTypeArgumentInfo> Arguments { get; set; }
        public abstract Type SystemType { get; set; }

        public GraphQLBaseType GetGraphQLType(ISchemaRepository schemaRepository)
        {
            return this.GetSchemaType(this.SystemType, schemaRepository);
        }

        protected abstract GraphQLBaseType GetSchemaType(Type type, ISchemaRepository schemaRepository);
    }
}
