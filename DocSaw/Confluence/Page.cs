using System.Collections.Generic;

namespace DocSaw.Confluence
{
    public class Page
    {
        public string Title { get; set; }
        public List<PageRef> Ancestors { get; set; }
    }
}