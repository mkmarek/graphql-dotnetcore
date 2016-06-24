namespace GraphQLCore.GraphiQLExample.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Type;
    using Schema;
    using Models;

    [Route("api/[controller]")]
    public class GraphQLController : Controller
    {
        [HttpPost]
        public JsonResult Post([FromBody] GraphiQLInput input)
        {
            GraphQLSchema schema = GetSchema();

            return this.Json(schema.Execute(input.Query));
        }

        private static GraphQLSchema GetSchema()
        {
            var schema = new GraphQLSchema();
            var root = new RootQuery(schema);
            schema.SetRoot(root);
            return schema;
        }
    }
}
