namespace GraphQLCore.Execution
{
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Type.Translation;
    using Utils;

    public class ResolverExpression : IFieldExpression
    {
        private ISchemaRepository schemaRepository;

        private LambdaExpression Lambda { get; set; }
        private object Parent { get; set; }
        private IList<GraphQLArgument> Arguments { get; set; }

        private ResolverExpression() { }

        public static IFieldExpression Create(LambdaExpression lambda, ISchemaRepository schemaRepository, object parent, IList<GraphQLArgument> arguments)
        {
            return new ResolverExpression()
            {
                schemaRepository = schemaRepository,
                Lambda = lambda,
                Arguments = arguments,
                Parent = parent
            };
        }

        public async Task<object> GetResult()
        {
            var argumentFetcher = new ArgumentFetcher(this.schemaRepository);
            var argumentValues = argumentFetcher.FetchArgumentValues(this.Lambda, this.Arguments, this.Parent);

            var result = this.Lambda.Compile().DynamicInvoke(argumentValues);

            return await AsyncUtils.HandleAsyncTaskIfAsync(result);
        }
    }
}
