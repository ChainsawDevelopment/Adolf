using System;
using System.Linq;
using DocSaw.Confluence;

namespace DocSaw.Rules
{
    public interface IRule
    {
        void Check(Page page, ErrorReporter reporter);
    }

    public class PageClassMustBeCamelCase : IRule
    {
        public void Check(Page page, ErrorReporter reporter)
        {
            if (!TitleCasing.IsCamelCase(page.Title))
            {
                reporter.Report(page, "Page title is not camel case");
            }
        }
    }
}