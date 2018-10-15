using System;
using System.Linq;
using DocSaw.Confluence;
using Newtonsoft.Json;

namespace DocSaw.Rules
{
    public class LinkLabelsMustBeRaw : IRule
    {
        public void Check(Page page, ErrorReporter reporter)
        {
            var doc = JsonConvert.DeserializeObject<AtlasItem>(page.Body.AtlasDocFormat.Value);

            var links = doc.Descendants().Where(x => x.Type == "text" && x.HasMark("link"));

            foreach (var link in links)
            {
                Action<string> report = msg => reporter.Report(page, msg);

                var linkMark = link.Mark("link");

                if (linkMark.Attrs.ContainsKey("__confluenceMetadata"))
                {
                    CheckConfluenceLink(linkMark, link, report);
                }
                else
                {
                    CheckExternalLink(linkMark, link, report);
                }
            }
        }

        private void CheckConfluenceLink(AtlasMark linkMark, AtlasItem link, Action<string> report)
        {
            var cflMeta = (dynamic)linkMark.Attrs["__confluenceMetadata"];

            var linkType = (string) cflMeta.linkType;

            var linkText = link.Text.Trim();

            if (linkType == "page")
            {
                var targetPageTitle = ((string) cflMeta.contentTitle).Trim();

                if (linkText != targetPageTitle)
                {
                    report($"Link '{link.Text}' is different from target Confluence page '{targetPageTitle}'");
                }
            }
            else if (linkType == "attachment")
            {
                var targetFileName = ((string) cflMeta.fileName).Trim();

                if (linkText != targetFileName)
                {
                    report($"Link '{link.Text}' is different from target attachment file name '{targetFileName}'");
                }
            }
            else if (linkType == "self")
            {
                // not much we can do here
            }
            else
            {
                report($"Link '{linkText}' has unrecognized link type '{linkType}'");
            }
        }

        private static void CheckExternalLink(AtlasMark linkMark, AtlasItem link, Action<string> report)
        {
            var href = (string) linkMark.Attrs["href"];

            var linkText = link.Text.Trim();

            if (href != linkText)
            {
                report($"Link '{linkText}' is different from underlying URL '{href}'");
            }
        }
    }
}