using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DocSaw.Confluence;
using Newtonsoft.Json;

namespace DocSaw.Rules
{
    public class SentenceMustNotBeTooLong : IRule
    {
        private readonly Config _config;
        private static readonly Regex SentenceEdge = new Regex(@"(?<=[.?!])\s+(?=[A-Z0-9])");

        public SentenceMustNotBeTooLong(Config config)
        {
            _config = config;
        }

        public class Config
        {
            public int MaxWords { get; set; } = 25;
        }

        public void Check(Page page, ErrorReporter reporter)
        {
            var doc = JsonConvert.DeserializeObject<AtlasItem>(page.Body.AtlasDocFormat.Value);

            var paragraphs = doc.Descendants().Where(x => x.Type == "paragraph").ToList();

            var texts = new List<string>();

            foreach (var paragraph in paragraphs)
            {
                var textItems = paragraph.Content
                    .Where(x => (x.Type == "text" || x.Type == "hardBreak") && !x.HasMark("code"));

                var text = string.Join("", textItems.Select(GetElementText));
                texts.Add(text);
            }

            foreach (var text in texts)
            {
                var sentences = SentenceEdge.Split(text);

                foreach (var sentence in sentences)
                {
                    var wordCount = CountWords(sentence);
                    if (wordCount > _config.MaxWords)
                    {
                        reporter.Report(page, $"Sentence '{sentence}' is too long: {wordCount} words");
                    }
                }
            }
        }

        private static string GetElementText(AtlasItem x)
        {
            if (x.Type == "hardBreak")
            {
                return " ";
            }

            return x.Text;
        }

        private int CountWords(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
            {
                return 0;
            }

            return sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}