namespace Marina.Commands.Animals.Cat
{
    using Newtonsoft.Json;
    using System;

    public partial class CatData
    {
        [JsonProperty("categories", NullValueHandling = NullValueHandling.Ignore)]
        public Category[] Categories { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class Category
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }
}
