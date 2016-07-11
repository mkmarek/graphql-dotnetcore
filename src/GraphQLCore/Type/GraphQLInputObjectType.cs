namespace GraphQLCore.Type
{
    public abstract class GraphQLInputObjectType : GraphQLComplexType
    {
        public GraphQLInputObjectType(string name, string description) : base(name, description)
        {
        }
    }
}