namespace GraphQLCore.GraphiQLExample.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;
    using System.Dynamic;
    using Type;

    [Route("api/[controller]")]
    public class GraphQLController : Controller
    {
        private IGraphQLSchema schema;

        public GraphQLController(IGraphQLSchema schema)
        {
            this.schema = schema;
        }

        [HttpPost]
        public JsonResult Post([FromBody] GraphiQLInput input)
        {
            return this.Json(this.schema.Execute(input.Query, GetVariables(input), input.OperationName));
        }

        private static dynamic GetVariables(GraphiQLInput input)
        {
            var variables = input.Variables?.ToString();

            if (string.IsNullOrEmpty(variables))
                return new ExpandoObject();

            return JsonConvert.DeserializeObject<ExpandoObject>(variables);
        }
    }
}
