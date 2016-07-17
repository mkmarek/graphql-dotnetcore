namespace GraphQLCore.Type
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class GraphQLObjectTypeFieldInfo
    {
        public IDictionary<string, GraphQLObjectTypeArgumentInfo> Arguments { get; set; }
        public bool IsResolver { get; set; }
        public LambdaExpression Lambda { get; set; }
        public string Name { get; set; }
        public System.Type SystemType { get; set; }
    }
}