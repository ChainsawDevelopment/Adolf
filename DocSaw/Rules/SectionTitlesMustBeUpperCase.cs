using System;
using System.Linq;
using DocSaw.Confluence;
using Newtonsoft.Json;
using RestSharp;

namespace DocSaw.Rules
{
    public class SectionTitlesMustBeUpperCase : IRule
    {
        private readonly RestClient _client;

        public SectionTitlesMustBeUpperCase(RestSharp.RestClient client)
        {
            _client = client;
        }

        public void Check(Page page, ErrorReporter reporter)
        {
            var doc = JsonConvert.DeserializeObject<AtlasItem>(page.Body.AtlasDocFormat.Value);

            var headings = doc.Descendants().Where(x => x.Type == "heading");

            foreach (var heading in headings)
            {
                var texts = heading.Descendants().Where(x => x.Type == "text").Select(x => x.Text);

                var text = string.Join(" ", texts);

                if (!TitleCasing.IsCamelCase(text))
                {
                    reporter.Report(page, $"Heading '{text}' is not camel case");
                }
            }
        }
    }
}