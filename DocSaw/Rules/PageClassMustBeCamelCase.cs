using System;
using System.Linq;
using DocSaw.Confluence;

namespace DocSaw.Rules
{
    public class PageClassMustBeCamelCase
    {
        public void Check(Page page, ErrorReporter reporter)
        {
            var terms = page.Title.Trim().Split(' ');

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
                    "by"
                })
                .All(x => Char.IsUpper(x[0]));

            if (!allWordsStarsWithUpperLetter)
            {
                reporter.Report(page, "Page title is not camel case");
            }
        }
    }
}