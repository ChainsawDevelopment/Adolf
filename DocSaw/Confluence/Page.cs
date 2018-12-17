using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocSaw.Confluence
{
    public class Page
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<PageRef> Ancestors { get; set; }

        public PageBody Body { get; set; }

        public class PageBody
        {
            [JsonProperty("atlas_doc_format")]
            public AtlasDocContent AtlasDocFormat { get; set; }
        }

        public PageLinks _links { get; set; }

        public class PageLinks
        {
            public string Self { get; set; }
            public string Webui { get; set; }
        }

        public string GetPath()
        {
            return string.Join(" -> ", Ancestors.Select(x => x.Title.Trim()).Union(new[] {Title.Trim()}));
        }
    }

    public class AtlasDocContent
    {
        
        public string Value { get; set; }
    }

    public class AtlasItem
    {
        public string Type { get; set; }
        public List<AtlasItem> Content { get; set; } = new List<AtlasItem>();
        public string Text { get; set; }
        public List<AtlasMark> Marks { get; set; } = new List<AtlasMark>();

        public void VisitAll(Action<AtlasItem> action)
        {
            action(this);

            if (Content == null)
            {
                return;
            }

            foreach (var child in Content)
            {
                child.VisitAll(action);
            }
        }

        public IEnumerable<AtlasItem> Descendants()
        {
            if (Content == null)
            {
                yield break;
            }

            foreach (var child in Content)
            {
                yield return child;

                foreach (var grandChild in child.Descendants())
                {
                    yield return grandChild;
                }
            }
        }

        public bool HasMark(string markType) => Marks.Any(x => x.Type == markType);
        public AtlasMark Mark(string markType) => Marks.Single(x => x.Type == markType);

        public override string ToString() => $"Type={Type}";
    }

    public class AtlasMark
    {
        public string Type { get; set; }

        public override string ToString() => $"Mark: {Type}";
        public Dictionary<string, JToken> Attrs { get; set; }
    }
}