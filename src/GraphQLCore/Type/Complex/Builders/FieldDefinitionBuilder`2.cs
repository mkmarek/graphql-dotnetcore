namespace GraphQLCore.Type.Complex
{
    public abstract class FieldDefinitionBuilder<TDefinitionBuilder, TFieldInfo>
        where TFieldInfo : GraphQLFieldInfo
        where TDefinitionBuilder : FieldDefinitionBuilder<TDefinitionBuilder, TFieldInfo>
    {
        protected TFieldInfo FieldInfo { get; }

        protected FieldDefinitionBuilder(TFieldInfo fieldInfo)
        {
            this.FieldInfo = fieldInfo;
        }

        public TDefinitionBuilder ResolveWithUnion<TUnionType>()
            where TUnionType : GraphQLUnionType
        {
            this.FieldInfo.SystemType = typeof(TUnionType);

            return (TDefinitionBuilder)this;
        }

        public TDefinitionBuilder WithDescription(string description)
        {
            this.FieldInfo.Description = description;

            return (TDefinitionBuilder)this;
        }

        public TDefinitionBuilder IsDeprecated(string deprecationReason)
        {
            if (deprecationReason != null)
            {
                this.FieldInfo.IsDeprecated = true;
                this.FieldInfo.DeprecationReason = deprecationReason;
            }

            return (TDefinitionBuilder)this;
        }
    }
}
