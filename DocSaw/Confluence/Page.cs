using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocSaw.Confluence
{
    public class Page
    {
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

    public class AtlasFormatConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //if (objectType != typeof(AtlasItem))
            //{
            //    return serializer.
            //}

            var obj = JObject.Load(reader);

            var itemType = obj.Property("type").Value.Value<string>();

            switch (itemType)
            {
                case "doc":
                    return obj.ToObject<AtlasDoc>(serializer);
                default:
                    throw new Exception($"Unrecognized item type {itemType}");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }

    //[JsonConverter(typeof(AtlasFormatConverter))]
    public class AtlasItem
    {
        public string Type { get; set; }
        public List<AtlasItem> Content { get; set; }
        public string Text { get; set; }

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
    }

    public class AtlasDoc : AtlasItem
    {
    }
}