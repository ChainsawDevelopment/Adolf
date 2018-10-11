using System;
using System.Collections.Generic;
using System.Linq;
using DocSaw.Confluence;
using Newtonsoft.Json;

namespace DocSaw.Rules
{
    public class UseMacroForNote : IRule
    {
        private static readonly string[] Beginnings = new[] {"Note:", "NOTE", "CAUTION:", "Please note"};

        public void Check(Page page, ErrorReporter reporter)
        {
            var doc = JsonConvert.DeserializeObject<AtlasItem>(page.Body.AtlasDocFormat.Value);

            var paragraphs = doc.Descendants().Where(x => x.Type == "paragraph");

            Action<string> report = s => reporter.Report(page, $"Note detected outside of macro: '{s}'");

            foreach (var paragraph in paragraphs)
            {
                NoteAfterHardBreak(paragraph, report);
            }
        }

        private void NoteAfterHardBreak(AtlasItem paragraph, Action<string> report)
        {
            var parts = SplitAtHardBreaks(paragraph.Content);

            foreach (var part in parts)
            {
                var textItems = part
                    .Where(x => x.Type == "text" && !x.HasMark("code"));

                var text = string.Join(" ", textItems.Select(x => x.Text));

                if (Beginnings.Any(x => text.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                {
                    report(text);
                }
            }
        }

        private IEnumerable<IEnumerable<AtlasItem>> SplitAtHardBreaks(IEnumerable<AtlasItem> items)
        {
            var part = new List<AtlasItem>();

            foreach (var item in items)
            {
                if (item.Type == "hardBreak")
                {
                    yield return part;
                    part = new List<AtlasItem>();
                    continue;
                }

                part.Add(item);
            }

            if (part.Any())
            {
                yield return part;
            }
        }
    }
}