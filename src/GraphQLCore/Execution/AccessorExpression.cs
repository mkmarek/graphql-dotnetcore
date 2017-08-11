namespace GraphQLCore.Execution
{
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Utils;

    public class AccessorExpression : IFieldExpression
    {
        private LambdaExpression Lambda { get; set; }
        private object Parent { get; set; }

        private AccessorExpression() { }

        public static IFieldExpression Create(LambdaExpression lambda, object parent)
        {
            return new AccessorExpression()
            {
                Lambda = lambda,
                Parent = parent
            };
        }

        public async Task<object> GetResult()
        {
            var accessorResult = this.Lambda.Compile().DynamicInvoke(new object[] { this.Parent });
            return await AsyncUtils.HandleAsyncTaskIfAsync(accessorResult);
        }
    }
}