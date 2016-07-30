namespace GraphQLCore.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class StringUtils
    {
        public static string QuotedOrList(IEnumerable<string> input)
        {
            var inputSize = Math.Min(input.Count(), 5);
            var index = 0; // Ugly but aggregate is missing index parameter

            return input
                .Take(inputSize)
                .Select(e => $"\"{e}\"")
                .Aggregate((list, quoted) =>
                    list +
                    (inputSize > 2 ? ", " : " ") +
                    (++index == inputSize - 1 ? "or " : string.Empty) +
                    quoted);
        }

        public static IEnumerable<string> SuggestionList(string input, IEnumerable<string> options)
        {
            if (string.IsNullOrWhiteSpace(input))
                return options;

            var inputThreshold = input.Length / 2;

            return options
                .Select(e => new { option = e, distance = LexicalDistance(input, e) })
                .Where(e => e.distance <= Math.Max(inputThreshold, e.option.Length / 2))
                .OrderBy(e => e.distance)
                .Select(e => e.option)
                .ToArray();
        }

        public static int LexicalDistance(string first, string second)
        {
            int i;
            int j;
            var firstLength = first.Length;
            var secondLength = second.Length;
            var d = new int[firstLength + 1][];

            for (i = 0; i <= firstLength; i++)
            {
                d[i] = new int[secondLength + 1];
                d[i][0] = i;
            }

            for (j = 1; j <= secondLength; j++)
            {
                d[0][j] = j;
            }

            for (i = 1; i <= firstLength; i++)
            {
                for (j = 1; j <= secondLength; j++)
                {
                    var cost = first[i - 1] == second[j - 1] ? 0 : 1;

                    d[i][j] = Math.Min(
                      Math.Min(
                           d[i - 1][j] + 1,
                           d[i][j - 1] + 1),
                      d[i - 1][j - 1] + cost);

                    if (i > 1 && j > 1 &&
                        first[i - 1] == second[j - 2] &&
                        first[i - 2] == second[j - 1])
                    {
                        d[i][j] = Math.Min(d[i][j], d[i - 2][j - 2] + cost);
                    }
                }
            }

            return d[firstLength][secondLength];
        }
    }
}
