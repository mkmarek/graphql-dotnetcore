namespace GraphQLCore.WsMiddleware.Handlers
{
    using System;
    using System.Net.WebSockets;
    using Execution;
    using Payloads;
    using WsMiddleware;

    public class ExecutionObserver : IObserver<ExecutionResult>
    {
        private WebSocket socket;
        private OperationMessage input;

        public ExecutionObserver(WebSocket socket, OperationMessage input)
        {
            this.socket = socket;
            this.input = input;
        }

        public async void OnCompleted()
        {
            await socket.SendResponse(MessageType.GQL_COMPLETE, input.Id);
        }

        public async void OnError(Exception error)
        {
            await socket.SendResponse(MessageType.GQL_ERROR, input.Id,
                new ErrorPayload() { Error = error });
        }

        public async void OnNext(ExecutionResult value)
        {
            await socket.SendResponse(MessageType.GQL_DATA, input.Id,
                new DataPayload() { Data = value });
        }
    }
}
