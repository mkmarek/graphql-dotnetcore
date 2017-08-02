namespace GraphQLCore.Execution
{
    using Exceptions;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ValueToComplete
    {
        public object Input { get; set; }
        public Type InputType { get; set; }
        public GraphQLFieldSelection Selection { get; set; }
        public IEnumerable<object> Path { get; set; }
        public IList<GraphQLException> Errors { get; set; }

        private ValueToComplete() { }

        public static ValueToComplete Create(object input, Type inputType, GraphQLFieldSelection selection, IEnumerable<object> path, IList<GraphQLException> errors)
        {
            return new ValueToComplete()
            {
                Input = input,
                InputType = inputType,
                Selection = selection,
                Path = path,
                Errors = errors
            };
        }

        public static ValueToComplete CreateForCollection(object input, Type inputType, GraphQLFieldSelection selection, IEnumerable<object> path, int index, IList<GraphQLException> errors)
        {
            return new ValueToComplete()
            {
                Input = input,
                InputType = inputType,
                Selection = selection,
                Path = path.Append(index),
                Errors = errors
            };
        }
    }
}
