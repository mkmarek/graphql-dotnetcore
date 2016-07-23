namespace GraphQLCore.Type
{
    using Complex;
    using Introspection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Translation;
    using Utils;

    public abstract class GraphQLInputObjectType : GraphQLInputType
    {
        public GraphQLInputObjectType(string name, string description) : base(name, description)
        {
            this.Fields = new Dictionary<string, GraphQLInputObjectTypeFieldInfo>();
        }

        protected Dictionary<string, GraphQLInputObjectTypeFieldInfo> Fields { get; set; }

        public bool ContainsField(string fieldName)
        {
            return this.Fields.ContainsKey(fieldName);
        }

        public GraphQLInputObjectTypeFieldInfo GetFieldInfo(string fieldName)
        {
            if (!this.ContainsField(fieldName))
                return null;

            return this.Fields[fieldName];
        }

        public GraphQLInputObjectTypeFieldInfo[] GetFieldsInfo()
        {
            return this.Fields.Select(e => e.Value)
                .ToArray();
        }

        public override IntrospectedType Introspect(ISchemaRepository schemaRepository)
        {
            var introspectedType = new IntrospectedInputObject(schemaRepository, this);

            introspectedType.Name = this.Name;
            introspectedType.Description = this.Description;
            introspectedType.Kind = TypeKind.INPUT_OBJECT;

            return introspectedType;
        }
    }
}