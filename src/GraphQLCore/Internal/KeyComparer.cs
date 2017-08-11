namespace GraphQLCore.Internal
{
    using System.Collections.Generic;

    internal class KeyComparer : Comparer<int[]>
    {
        public override int Compare(int[] first, int[] second)
        {
            if (first == null)
                return -1;
            if (second == null)
                return 1;

            var secondLength = second.Length;
            for (var i = 0; i < first.Length; i++)
            {
                if (i == secondLength)
                    return 1;

                var result = first[i].CompareTo(second[i]);
                if (result != 0)
                    return result;
            }

            return -1;
        }
    }
}