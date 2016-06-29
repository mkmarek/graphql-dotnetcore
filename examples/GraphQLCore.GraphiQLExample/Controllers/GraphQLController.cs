namespace GraphQLCore.GraphiQLExample.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Schema;
    using System;
    using Type;

    [Route("api/[controller]")]
    public class GraphQLController : Controller
    {
        [HttpPost]
        public JsonResult Post([FromBody] GraphiQLInput input)
        {
            GraphQLSchema schema = GetSchema();

            try
            {
                return this.Json(new { data = schema.Execute(input.Query) });
            } catch(Exception ex)
            {
                return this.Json(new { error = ex });
            }
        }

        private static GraphQLSchema GetSchema()
        {
            var schema = new GraphQLSchema();
            var root = new Query(schema);
            schema.SetRoot(root);
            return schema;
        }
    }
}