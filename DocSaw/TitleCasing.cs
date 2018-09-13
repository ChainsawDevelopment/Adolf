using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace DocSaw
{
    public static class TitleCasing
    {
        private static readonly string[] Articles = new[] {"a", "an", "the"};

        private static readonly string[] Conjunction2 = new[]
        {
            "and", "but", "or", "for", "nor", "so", "yet"
        };

        private static readonly string[] Prepositions = new[]
        {
            "about", "above", "across", "after", "against", "along", "among", "around", "as", "at", "before", "behind", "below", "beneath", "beside", "between", "beyond", "by",
            "down", "during", "except", "for", "from", "in", "inside", "into", "like", "near", "of", "off", "on", "onto", "out", "outside", "over", "past", "regarding", "since",
            "through", "throughout", "till", "toward", "under", "until", "up", "upon", "with", "within", "without"
        };

        private static readonly string[] To = new[]
        {
            "to"
        };

        private static readonly char[] Separators = new[]
        {
            ' ', '-', ',', '.', '(', ')', '/'
        };

        private static readonly string[] Units = new[]
        {
            "km"
        };

        public static bool IsCamelCase(string title)
        {
            string tmp;
            return IsCamelCase(title, out tmp);
        }

        public static bool IsCamelCase(string title, out string invalidStart)
        {
            bool result = true;
            invalidStart = "";

            var remaining = new StringSegment(title).Trim();
            var initialLength = remaining.Length;

            if (remaining.Length > 0 && !char.IsLetterOrDigit(remaining[0]))
            {
                invalidStart = title;
                return false;
            }


            var allowedLowerCase =
                Articles
                    .Union(Conjunction2)
                    .Union(Prepositions)
                    .Union(To)
                    .Union(Units)
                    .ToList();

            while (remaining.Length > 0)
            {
                if (Separators.Contains(remaining[0]))
                {
                    remaining = remaining.Subsegment(1);
                    continue;
                }

                if (char.IsDigit(remaining[0]))
                {
                    remaining = SplitAtWhitespace(remaining);
                    continue;
                }

                {
                    var part = allowedLowerCase.FirstOrDefault(Predicate(remaining));

                    if (part != null)
                    {
                        if (remaining.Length == initialLength)
                        {
                            part = char.ToUpper(part[0]) + part.Substring(1);
                        }

                        var matchingInTitle = remaining.Subsegment(0, part.Length);

                        if (matchingInTitle != part)
                        {
                            invalidStart = remaining.ToString();
                            return false;
                        }

                        remaining = remaining.Subsegment(part.Length).TrimStart();
                        continue;
                    }
                }

                if (!char.IsUpper(remaining[0]))
                {
                    invalidStart = remaining.ToString();
                    return false;
                }

                remaining = SplitAtWhitespace(remaining);
            }

            return true;
        }

        private static StringSegment SplitAtWhitespace(StringSegment segment)
        {
            var idx = segment.IndexOf(' ');
            if (idx == -1)
            {
                return StringSegment.Empty;
            }

            return segment.Subsegment(idx + 1).TrimStart();
        }

        private static Func<string, bool> Predicate(StringSegment remaining)
        {
            return x =>
            {
                if (!remaining.StartsWith(x, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                if (remaining.Length == x.Length)
                {
                    return true;
                }

                if (!char.IsWhiteSpace(remaining[x.Length]))
                {
                    return false;
                }

                return true;
            };
        }
    }
}