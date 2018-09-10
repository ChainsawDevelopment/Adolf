using System.Collections.Generic;

namespace DocSaw.Confluence
{
    public class Paged<T>
    {
        public List<T> Results { get; set; }
        public PagingLinks _links { get; set; }
    }
}