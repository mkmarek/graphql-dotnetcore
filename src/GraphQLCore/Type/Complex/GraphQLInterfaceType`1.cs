namespace GraphQLCore.Type
{
    using Exceptions;
    using System;
    using System.Linq.Expressions;
    using Complex;

    public abstract class GraphQLInterfaceType<T> : GraphQLInterfaceType
        where T : class
    {
        public override Type SystemType { get; protected set; }

        public GraphQLInterfaceType(string name, string description) : base(name, description, typeof(T))
        {
            this.SystemType = typeof(T);
        }

        public FieldDefinitionBuilder Field<TProperty>(string fieldName, Expression<Func<T, TProperty>> accessor, string description = null)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            var fieldInfo = this.CreateFieldInfo(fieldName, accessor, description);
            this.Fields.Add(fieldName, fieldInfo);

            return new FieldDefinitionBuilder(fieldInfo);
        }
    }
}