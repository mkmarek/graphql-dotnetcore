using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLCore.Utils
{
    using System.Reflection;

    public static class AsyncUtils
    {
        public static async Task<object> HandleAsyncTaskIfAsync(object result)
        {
            if (
                result is Task &&
                (!result.GetType().GetTypeInfo().GetGenericArguments().Any() ||
                 result.GetType().GetTypeInfo().GetGenericArguments()?.FirstOrDefault()?.Name == "VoidTaskResult"))
            {
                await (Task)result;

                return null;
            }

            if (result is Task)
            {
                Task r = (Task)result;

                return await Task.Run(() => ((dynamic)result).GetAwaiter().GetResult());
            }

            return await Task.FromResult(result);
        }
    }
}
