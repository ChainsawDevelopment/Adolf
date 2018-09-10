﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DocSaw.Targets
{
    internal class ConsoleErrorTarget : IErrorSender
    {
        public void Send(IEnumerable<ErrorReporter.Error> errors)
        {
            var byPage = errors.GroupBy(x=>x.Page);

            foreach (var pageErrors in byPage)
            {
                var path = pageErrors.Key.GetPath();

                Console.WriteLine(path);

                foreach (var error in pageErrors)
                {
                    Console.Write("\t");
                    Console.WriteLine(error.Message);
                }
            }
        }
    }
}