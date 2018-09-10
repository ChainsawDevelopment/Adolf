using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;

namespace DocSaw
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(args[0])
                .Build();

            var rs = new RestSharp.RestClient(config["ConfluenceUrl"]);
            rs.Authenticator = new HttpBasicAuthenticator(config["User"], config["Token"]);

            var pageUrl = $"/rest/api/content?expand=body,container,metadata.properties,ancestors&spaceKey={config["SpaceKey"]}&limit=100";

            int totalPages = 0;

            while (!string.IsNullOrWhiteSpace(pageUrl))
            {
                var response = await rs.ExecuteGetTaskAsync<Paged<Page>>(new RestRequest(pageUrl));

                foreach (var page in response.Data.Results)
                {
                    var path = string.Join(" -> ", page.Ancestors.Select(x => x.Title));

                    if (page.Ancestors.ElementAtOrDefault(1)?.Title != config["CheckRoot"])
                    {
                        continue;
                    }

                    
                    Check(page);
                }

                Console.WriteLine(response.Data._links?.Next);
                pageUrl = response.Data._links?.Next;
                totalPages += response.Data.Results.Count;
            }
        }

        private static void Check(Page page)
        {
            CheckCamelCase(page.Title);
        }

        private static void CheckCamelCase(string title)
        {
            var terms = title.Trim().Split(' ');

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
                .All(x => char.IsUpper(x[0]));

            if (!allWordsStarsWithUpperLetter)
            {
                Console.WriteLine($"Page '{title}' is not titled as camel case");
            }
        }
    }

    public class Paged<T>
    {
        public List<T> Results { get; set; }
        public PagingLinks _links { get; set; }
    }

    public class PagingLinks
    {
        public string Next { get; set; }
    }

    public class Page
    {
        public string Title { get; set; }
        public List<PageRef> Ancestors { get; set; }
    }

    public class PageRef
    {
        public string Title { get; set; }
    }
}
