using System;
using System.Collections.Generic;
using System.Linq;

namespace DocSaw.Targets
{
    internal class ConsoleErrorTarget : IErrorSender
    {
        private readonly string _siteBase;

        public ConsoleErrorTarget(string siteBase)
        {
            _siteBase = siteBase;
        }

        public void Send(IEnumerable<ErrorReporter.Error> errors)
        {
            var byPage = errors.GroupBy(x=>x.Page);

            foreach (var pageErrors in byPage)
            {
                var path = pageErrors.Key.GetPath();

                Console.WriteLine(path);
                Console.WriteLine($"\t{_siteBase}{pageErrors.Key._links.Webui}");

                foreach (var error in pageErrors)
                {
                    Console.Write("\t");
                    Console.WriteLine(error.Message);
                }
            }
        }
    }
}