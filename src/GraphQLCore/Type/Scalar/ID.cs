namespace GraphQLCore.Type.Scalar
{
    public struct ID
    {
        private readonly string value;

        public ID(string value)
        {
            this.value = value;
        }

        public static implicit operator ID(string value)
        {
            return new ID(value);
        }

        public static implicit operator ID(uint value)
        {
            return new ID(value.ToString());
        }

        public static implicit operator ID(ulong value)
        {
            return new ID(value.ToString());
        }

        public static implicit operator string(ID id)
        {
            return id.value;
        }

        public override string ToString()
        {
            return this.value;
        }
    }
}
