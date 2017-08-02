using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLCore.WsMiddleware
{
    using Execution;
    using Handlers;
    using Payloads;
    using Type;

    public class OperationManager
    {
        public IGraphQLSchema Schema { get; }
        private readonly string clientId;
        private readonly Dictionary<string, IDisposable> operations;

        public OperationManager(IGraphQLSchema schema)
        {
            this.operations = new Dictionary<string, IDisposable>();

            this.clientId = GenerateClientId();
            this.Schema = schema;
        }

        public IDisposable Subscribe(string operationId, StartPayload payload, IObserver<ExecutionResult> observer)
        {
            this.Unsubscribe(operationId);

            var observable = this.Schema.Subscribe(payload.Query, payload.Variables, payload.OperationName, clientId, operationId);
            var subscription = observable.Subscribe(observer);
            this.operations.Add(operationId, subscription);

            return subscription;
        }

        public void Dispose()
        {
            foreach (var operation in this.operations)
            {
                operation.Value.Dispose();
                this.Schema.Unsubscribe(this.clientId, operation.Key);
            }
        }

        public void Unsubscribe(string operationId)
        {
            if (this.operations.ContainsKey(operationId))
            {
                this.operations[operationId].Dispose();
                this.Schema.Unsubscribe(this.clientId, operationId);
                this.operations.Remove(operationId);
            }
        }

        private static string GenerateClientId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
