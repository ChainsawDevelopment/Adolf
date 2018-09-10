using System;
using System.Collections.Generic;
using System.IO;
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
using RestSharp.Deserializers;

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

            var container = BuildContainer(rs);

            var pageUrl = $"/rest/api/content?expand=body,body.atlas_doc_format,container,metadata.properties,ancestors&spaceKey={config["SpaceKey"]}&limit=100";

            var reporter = new ErrorReporter();

            int totalPages = 0;

            var rules = container.Resolve<IEnumerable<IRule>>();

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

            reporter.SendErrorsTo(new ConsoleErrorTarget());

            Console.WriteLine($"{totalPages} pages checked");
            Console.WriteLine($"{reporter.ErrorsCount} errors reported");
        }

        private static IContainer BuildContainer(RestClient restClient)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(restClient).AsSelf();

            containerBuilder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .AssignableTo<IRule>()
                .AsImplementedInterfaces();

            return containerBuilder.Build();
        }
    }

    public class NewtonsoftDeserializer : IDeserializer
    {
        private readonly JsonSerializer _serializer;

        public NewtonsoftDeserializer(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public T Deserialize<T>(IRestResponse response)
        {
            using (var stringReader = new StringReader(response.Content))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return _serializer.Deserialize<T>(jsonReader);
            }
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
    }


}
