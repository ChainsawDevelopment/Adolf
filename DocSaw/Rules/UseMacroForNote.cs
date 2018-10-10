using System.Linq;
using DocSaw.Confluence;
using Newtonsoft.Json;

namespace DocSaw.Rules
{
    public class UseMacroForNote : IRule
    {
        private static readonly string[] Beginnings = new[] {"Note:", "CAUTION:"};

        public void Check(Page page, ErrorReporter reporter)
        {
            if (page.Id == "397541701")
            {
                
            }

            var doc = JsonConvert.DeserializeObject<AtlasItem>(page.Body.AtlasDocFormat.Value);

            var paragraphs = doc.Descendants().Where(x => x.Type == "paragraph");

            foreach (var paragraph in paragraphs)
            {
                var textItems = paragraph.Content
                    .Where(x => x.Type == "text" && !x.HasMark("code"));

                var text = string.Join(" ", textItems.Select(x => x.Text));

                if (Beginnings.Any(x => text.StartsWith(x)))
                {
                    reporter.Report(page, $"Note detected outside of macro: '{text}'");
                }
            }
        }
    }
}