namespace GraphQLCore.WsMiddleware
{
    using System.Collections.Generic;

    public static class MessageTypes
    {
        public static readonly Dictionary<MessageType, string> ClientTypes = new Dictionary<MessageType, string>
        {
            { MessageType.GQL_CONNECTION_INIT, "connection_init" },
            { MessageType.GQL_CONNECTION_TERMINATE, "connection_terminate" },
            { MessageType.GQL_START, "start" },
            { MessageType.GQL_STOP, "stop" },
        };
        
        public static readonly Dictionary<MessageType, string> ServerTypes = new Dictionary<MessageType, string>
        {
            { MessageType.GQL_CONNECTION_ACK, "connection_ack" },
            { MessageType.GQL_CONNECTION_ERROR, "connection_error" },
            { MessageType.GQL_CONNECTION_KEEP_ALIVE, "ka" },
            { MessageType.GQL_DATA, "data" },
            { MessageType.GQL_ERROR, "error" },
            { MessageType.GQL_COMPLETE, "complete" },
        };
    }
}
