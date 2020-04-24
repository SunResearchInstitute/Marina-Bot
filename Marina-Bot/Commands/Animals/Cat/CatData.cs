using Newtonsoft.Json;
using System;

namespace Marina.Commands.Animals.Cat
{
    public class CatData
    {
        [JsonProperty("categories", NullValueHandling = NullValueHandling.Ignore)]
#pragma warning disable 8618
        public Category[] Categories { get; set; }
#pragma warning restore 8618

        [JsonProperty("url")] public Uri Url { get; set; }
    }

    public class Category
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
#pragma warning disable 8618
        public string Name { get; set; }
#pragma warning restore 8618
    }
}