using System.Linq;
using DocSaw.Confluence;
using WeCantSpell.Hunspell;

namespace DocSaw.Rules
{
    public class SpellcheckerMustBeApplied : IRule
    {
        private WordList _dictionary;

        public SpellcheckerMustBeApplied()
        {
            _dictionary = WordList.CreateFromFiles(@"Resources\en_US.dic", @"Resources\en_US.aff");
        }

        public void Check(Page page, ErrorReporter reporter)
        {
            CheckSpelling(reporter, page, page.Title);
            CheckSpelling(reporter, page, page.Body.AtlasDocFormat.Value);
        }

        private void CheckSpelling(ErrorReporter reporter, Page page, string text)
        {
            var terms = text.Trim().Split(' ');

            foreach (var term in terms)
            {
                var result = _dictionary.Check(term);

                if (!result)
                    reporter.Report(page, $"Unknown word: '{term}'");
            }
        }
    }
}