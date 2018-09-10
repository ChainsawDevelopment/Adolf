using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DocSaw.Confluence;
using DocSaw.Rules;
using DocSaw.Targets;
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

            var reporter = new ErrorReporter();

            int totalPages = 0;

            var rules = new[]
            {
                new PageClassMustBeCamelCase(),
            };

            while (!string.IsNullOrWhiteSpace(pageUrl))
            {
                var response = await rs.ExecuteGetTaskAsync<Paged<Page>>(new RestRequest(pageUrl));

                foreach (var page in response.Data.Results)
                {
                    if (page.Ancestors.ElementAtOrDefault(1)?.Title != config["CheckRoot"])
                    {
                        continue;
                    }

                    foreach (var rule in rules)
                    {
                        rule.Check(page, reporter);
                    }
                }

                Console.WriteLine(response.Data._links?.Next);
                pageUrl = response.Data._links?.Next;
                totalPages += response.Data.Results.Count;
            }

            reporter.SendErrorsTo(new ConsoleErrorTarget());

            Console.WriteLine($"{totalPages} pages checked");
            Console.WriteLine($"{reporter.ErrorsCount} errors reported");
        }
    }
}
