using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLCore.Execution
{
    internal class FieldContext<T> : IContext<T>
    {
        public T Instance { get; private set; }

        public FieldContext(T instance)
        {
            this.Instance = instance;
        }
    }
}
