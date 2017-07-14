namespace GraphQLCore.Exceptions
{
    using Language;
    using Language.AST;
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    public class GraphQLException : Exception, ISerializable
    {
        public ASTNode[] Nodes { get; }
        public ISource ASTSource { get; }
        public int[] Positions { get; }
        public Location[] Locations { get; }
        public IEnumerable Path { get; }

        public GraphQLException(Exception baseException) : base(baseException.Message, baseException)
        {
        }

        public GraphQLException(string message, IEnumerable<ASTNode> nodes = null, ISource source = null,
            IEnumerable<int> positions = null, IEnumerable path = null,
            Exception innerException = null) : base(message, innerException)
        {
            if (source == null && nodes?.Count() > 0)
                source = nodes.First().Location.Source;

            if (positions == null && nodes != null)
                positions = nodes.Where(e => e.Location != null).Select(e => e.Location.Start);

            if (positions?.Count() == 0)
                positions = null;

            var locations = source != null && positions != null
                ? positions.Select(e => new Location(source, e)).ToArray()
                : null;

            this.Nodes = nodes?.ToArray();
            this.ASTSource = source;
            this.Positions = positions?.ToArray();
            this.Locations = locations?.ToArray();
            this.Path = path;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("message", this.Message);

            this.AddFieldIfNotNull("locations", this.Locations, info);
            this.AddFieldIfNotNull("path", this.Path, info);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static GraphQLException LocateException(GraphQLException originalException = null, IEnumerable<ASTNode> nodes = null,
            IEnumerable path = null)
        {
            if (originalException?.Path != null)
                return originalException;

            var message = originalException?.Message ?? "An unknown error occured.";

            var error = new GraphQLException(
                message,
                originalException?.Nodes ?? nodes,
                originalException?.ASTSource,
                originalException?.Positions,
                path,
                originalException);

            return error;
        }

        private void AddFieldIfNotNull(string name, object field, SerializationInfo info)
        {
            if (field != null)
                info.AddValue(name, field);
        }
    }
}
