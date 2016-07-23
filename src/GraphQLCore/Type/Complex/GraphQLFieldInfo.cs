namespace GraphQLCore.Type.Complex
{
    using System;
    using System.Collections.Generic;
    using Translation;
    using Utils;

    public abstract class GraphQLFieldInfo
    {
        public string Name { get; set; }

        public IDictionary<string, GraphQLObjectTypeArgumentInfo> Arguments { get; set; }
        public abstract Type SystemType { get; set; }

        public GraphQLBaseType GetGraphQLType(ISchemaRepository schemaRepository)
        {
            return this.GetGraphQLType(this.SystemType, schemaRepository);
        }

        protected abstract GraphQLBaseType GetSchemaType(Type type, ISchemaRepository schemaRepository);

        private GraphQLBaseType GetGraphQLType(Type type, ISchemaRepository schemaRepository)
        {
            if (ReflectionUtilities.IsCollection(type))
            {
                return new GraphQLList(this.GetGraphQLType(
                    ReflectionUtilities.GetCollectionMemberType(type),
                    schemaRepository));
            }

            if (ReflectionUtilities.IsNullable(type))
                return this.GetSchemaType(Nullable.GetUnderlyingType(type), schemaRepository);

            if (ReflectionUtilities.IsValueType(type))
                return new GraphQLNonNullType(this.GetSchemaType(type, schemaRepository));

            return this.GetSchemaType(type, schemaRepository);
        }
    }
}