using System;
using System.Linq;
using DocSaw.Confluence;
using Newtonsoft.Json;

namespace DocSaw.Rules
{
    public class DocumentationMustBeInEnglish : IRule
    {
        public void Check(Page page, ErrorReporter reporter)
        {
            if (HasPolishLetters(page.Title))
            {
                reporter.Report(page, "Page title is in Polish");
            }

            if (DoesBodyContainPolishLettersInLowercaseWords(page.Body.AtlasDocFormat.Value))
            {
                reporter.Report(page, "Page has Polish letters");
            }
        }

        private bool HasPolishLetters(string text)
        {
            var arePolishLetters = text.ToLower().IndexOfAny(new char[] { 'ą', 'ę', 'ć', 'ó', 'ś', 'ł', 'ż', 'ź', 'ń' }) != -1;
            return arePolishLetters;
        }

        private bool DoesBodyContainPolishLettersInLowercaseWords(string text)
        {
            var terms = text.Trim().Split(' ');

            return terms.Where(x => x == x.ToLower()).Any(HasPolishLetters);
        }
    }
}