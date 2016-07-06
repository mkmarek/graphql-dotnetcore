namespace GraphQLCore.GraphiQLExample.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using System;
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
            try
            {
                return this.Json(new { data = schema.Execute(input.Query) });
            }
            catch (Exception ex)
            {
                return this.Json(
                    new
                    {
                        errors = new dynamic[] { new { message = ex.Message } }
                    }
                );
            }
        }
    }
}