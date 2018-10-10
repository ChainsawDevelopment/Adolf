using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DocSaw.Confluence;
using DocSaw.Rules;
using DocSaw.Targets;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
            rs.ClearHandlers();
            
            var serializer = new JsonSerializer();

            rs.AddHandler("application/json", new NewtonsoftDeserializer(serializer));

            var container = BuildContainer(rs, config);

            var pageUrl = $"/rest/api/content?expand=body,body.atlas_doc_format,container,metadata.properties,ancestors&spaceKey={config["SpaceKey"]}&limit=100";

            var reporter = new ErrorReporter();

            int totalPages = 0;
            int ignoredPages = 0;

            var rules = CreateRuleSet(container);

            var ignoredPaths = config.GetSection("Ignore")
                .GetChildren()
                .Select(x => x.Value)
                .ToList();

            while (!string.IsNullOrWhiteSpace(pageUrl))
            {
                var response = await rs.ExecuteGetTaskAsync<Paged<Page>>(new RestRequest(pageUrl));

                foreach (var page in response.Data.Results)
                {
                    if (page.Ancestors.ElementAtOrDefault(1)?.Title != config["CheckRoot"])
                    {
                        continue;
                    }

                    var path = page.GetPath();

                    if (path.Contains("informacji"))
                    {
                        
                    }

                    if (ignoredPaths.Any(x => path.StartsWith(x, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        Console.WriteLine($"Ignoring {path}");
                        ignoredPages++;
                        continue;
                    }

                    foreach (var rule in rules)
                    {
                        try
                        {
                            rule.Check(page, reporter);
                        }
                        catch (Exception e)
                        {
                            reporter.Report(page, $"Rule {rule.GetType().Name} failed with {e.Message}");
                        }
                    }
                }

                Console.WriteLine(response.Data._links?.Next);
                pageUrl = response.Data._links?.Next;
                totalPages += response.Data.Results.Count;
            }

            reporter.SendErrorsTo(container.Resolve<ConsoleErrorTarget>());

            Console.WriteLine($"{totalPages} pages in total");
            Console.WriteLine($"{totalPages - ignoredPages} pages checked");
            Console.WriteLine($"{ignoredPages} pages ignored");
            Console.WriteLine($"{reporter.ErrorsCount} errors reported");
        }

        private static List<IRule> CreateRuleSet(IContainer container)
        {
            var onlyRules = container.Resolve<IConfigurationRoot>().GetSection("OnlyRules").GetChildren().Select(x => x.Value).ToArray();

            if (onlyRules.Any())
            {
                return onlyRules.Select(container.ResolveNamed<IRule>).ToList();
            }

            return container.Resolve<IEnumerable<IRule>>().ToList();
        }

        private static IContainer BuildContainer(RestClient restClient, IConfigurationRoot config)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(restClient).AsSelf();

            containerBuilder.RegisterInstance(config).As<IConfigurationRoot>();

            containerBuilder.RegisterModule<RulesModule>();

            containerBuilder.RegisterType<ConsoleErrorTarget>().AsSelf()
                .WithParameter(
                    (x, ctx) => x.Name == "siteBase", 
                    (x, ctx) => config["ConfluenceUrl"]
                );

            return containerBuilder.Build();
        }
    }
}
