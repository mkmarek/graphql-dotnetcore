namespace GraphQLCore.Type
{
    using Exceptions;
    using System;
    using System.Linq.Expressions;

    public abstract class GraphQLInterfaceType<T> : GraphQLInterfaceType
        where T : class
    {
        public override Type SystemType { get; protected set; }

        public GraphQLInterfaceType(string name, string description) : base(name, description, typeof(T))
        {
            this.SystemType = typeof(T);
        }

        public void Field<TProperty>(string fieldName, Expression<Func<T, TProperty>> accessor)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.Fields.Add(fieldName, this.CreateFieldInfo(fieldName, accessor));
        }
    }
}