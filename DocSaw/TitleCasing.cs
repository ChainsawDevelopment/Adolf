using System;
using System.Linq;

namespace DocSaw
{
    public static class TitleCasing
    {
        public static bool IsCamelCase(string title)
        {
            var terms = title.Trim().Split(' ');

            var allWordsStarsWithUpperLetter = terms
                .Except(new[]
                {
                    "after",
                    "although",
                    "as",
                    "as",
                    "if",
                    "as",
                    "long",
                    "as",
                    "because",
                    "before",
                    "despite",
                    "even",
                    "if",
                    "even",
                    "though",
                    "if",
                    "in",
                    "order",
                    "that",
                    "rather",
                    "than",
                    "since",
                    "so",
                    "that",
                    "that",
                    "though",
                    "unless",
                    "until",
                    "when",
                    "where",
                    "whereas",
                    "whether",
                    "and",
                    "while",
                    "of",
                    "the",
                    "by",
                    "-",
                    "for",
                    "on"
                })
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .Where(x => Char.IsLetter(x[0]))
                .All(x => Char.IsUpper(x[0]));

            return allWordsStarsWithUpperLetter;
        }
    }
}