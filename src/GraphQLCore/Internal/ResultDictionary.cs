namespace GraphQLCore.Internal
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    internal class ResultDictionary
    {
        private SortedDictionary<int[], string> keysOrder;
        private ExpandoObject expandoObject;
        private IDictionary<string, object> innerDictionary;

        public object this[string key]
        {
            get { return this.innerDictionary[key]; }
        }

        public ResultDictionary()
        {
            this.keysOrder = new SortedDictionary<int[], string>(new KeyComparer());
            this.expandoObject = new ExpandoObject();
            this.innerDictionary = this.expandoObject;
        }

        public static explicit operator ExpandoObject(ResultDictionary value)
        {
            return value.expandoObject;
        }

        public ExpandoObject GetOrdered()
        {
            var result = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)result;

            foreach (var pair in this)
                dictionary.Add(pair);

            return result;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.keysOrder.Select(e =>
                new KeyValuePair<string, object>(e.Value, this.innerDictionary[e.Value])).GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            return this.innerDictionary.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return this.innerDictionary.Remove(key);
        }

        public void Insert(int[] index, string key, object value)
        {
            this.innerDictionary.Add(key, value);
            this.keysOrder.Add(index, key);
        }
    }
}
