namespace GraphQLCore.GraphiQLExample.Controllers
{
    using Exceptions;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Dynamic;
    using System.Linq;
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
                return this.Json(
                    new
                    {
                        data = this.schema.Execute(input.Query, GetVariables(input), input.OperationName)
                    }
                );
            }
            catch (GraphQLValidationException ex)
            {
                return this.Json(
                    new
                    {
                        errors = ex.Errors
                    }
                );
            }
            catch (GraphQLException ex)
            {
                return this.Json(
                    new
                    {
                        errors = ex
                    });
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

        private static dynamic GetVariables(GraphiQLInput input)
        {
            var variables = input.Variables?.ToString();

            if (string.IsNullOrEmpty(variables))
                return new ExpandoObject();

            return JsonConvert.DeserializeObject<ExpandoObject>(variables);
        }
    }
}
