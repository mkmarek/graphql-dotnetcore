namespace GraphQLCore.Language
{
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    public class Location : ISerializable
    {
        public Location(ISource source, int position)
        {
            var lineRegex = new Regex("\r\n|[\n\r]", RegexOptions.ECMAScript);
            this.Line = 1;
            this.Column = position + 1;

            var matches = lineRegex.Matches(source.Body);
            foreach (Match match in matches)
            {
                if (match.Index >= position)
                    break;

                this.Line++;
                this.Column = position + 1 - (match.Index + matches[0].Length);
            }
        }

        public int Column { get; }
        public int Line { get; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("line", this.Line);
            info.AddValue("column", this.Column);
        }
    }
}