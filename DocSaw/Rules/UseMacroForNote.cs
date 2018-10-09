using System.Linq;
using DocSaw.Confluence;
using Newtonsoft.Json;

namespace DocSaw.Rules
{
    public class UseMacroForNote : IRule
    {
        public void Check(Page page, ErrorReporter reporter)
        {
            var doc = JsonConvert.DeserializeObject<AtlasItem>(page.Body.AtlasDocFormat.Value);

            var paragraphs = doc.Descendants().Where(x => x.Type == "paragraph");

            foreach (var paragraph in paragraphs)
            {
                var textItems = paragraph.Content
                    .Where(x => x.Type == "text" && !x.HasMark("code"));

                var text = string.Join(" ", textItems.Select(x => x.Text));

                if (text.StartsWith("Note:"))
                {
                    reporter.Report(page, $"Note detected outside of macro: '{text}'");
                }
            }
        }
    }
}