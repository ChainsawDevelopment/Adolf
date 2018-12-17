using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Autofac.Features.AttributeFilters;
using DocSaw.Confluence;
using Newtonsoft.Json;

namespace DocSaw.Rules
{
    public class DoNotUseBlacklistedWord : IRule
    {
        private readonly Config _config;
        private readonly List<(string Word, Regex Regex)> _blacklist;

        private const string RegexLeftSeparator = @"(?<=^|\W)(";
        private const string RegexRightSeparator = @")(?=\W|$)";

        public class Config
        {
            public string BlackListFile { get; set; } 
        }

        public DoNotUseBlacklistedWord(Config config, [KeyFilter("ConfigFile")]string configFile)
        {
            _config = config;
            _blacklist = ReadBlacklist(Path.Combine(Path.GetDirectoryName(configFile), config.BlackListFile));
        }

        private List<(string Word, Regex Regex)> ReadBlacklist(string filePath)
        {
            var lines = File.ReadAllLines(filePath);

            var result = new List<(string Word, Regex Regex)>(lines.Length);

            foreach (var line in lines)
            {
                string regex;
                if (line.StartsWith("/") && line.EndsWith("/"))
                {
                    var partial = line.Substring(1, line.Length - 2);

                    regex = RegexLeftSeparator + partial + RegexRightSeparator;
                }
                else
                {
                    regex = RegexLeftSeparator + Regex.Escape(line) + RegexRightSeparator;
                }

                result.Add((line, new Regex(regex, RegexOptions.Compiled)));
            }

            return result;
        }

        public void Check(Page page, ErrorReporter reporter)
        {
            var doc = JsonConvert.DeserializeObject<AtlasItem>(page.Body.AtlasDocFormat.Value);

            var paragraphs = doc.Descendants().Where(x => x.Type == "paragraph").ToList();

            foreach (var paragraph in paragraphs)
            {
                var textItems = paragraph.Content
                    .Where(x => (x.Type == "text" || x.Type == "hardBreak") && !x.HasMark("code"));

                var text = string.Join("", textItems.Select(GetElementText));

                var blacklistedWords = _blacklist.Select(x => new {x.Word, Matches = x.Regex.Matches(text)});

                foreach (var blacklistedWord in blacklistedWords)
                {
                    foreach (Match match in blacklistedWord.Matches)
                    {
                        reporter.Report(page, $"Found blacklisted word '{match.Value}' matching pattern '{blacklistedWord.Word}'");
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
    }
}