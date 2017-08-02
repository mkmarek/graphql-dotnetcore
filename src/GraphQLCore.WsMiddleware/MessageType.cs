namespace GraphQLCore.WsMiddleware
{
    public enum MessageType
    {
        GQL_CONNECTION_INIT,
        GQL_CONNECTION_TERMINATE,
        GQL_START,
        GQL_STOP,
        GQL_CONNECTION_ACK,
        GQL_CONNECTION_ERROR,
        GQL_CONNECTION_KEEP_ALIVE,
        GQL_DATA,
        GQL_ERROR,
        GQL_COMPLETE
    }
}