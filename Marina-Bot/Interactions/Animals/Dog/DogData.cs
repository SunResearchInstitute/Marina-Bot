using Newtonsoft.Json;
using System;

namespace Marina.Interactions.Animals.Dog
{
    public class DogData
    {
        [JsonProperty("message")] public Uri ImageUrl { get; set; }

        [JsonProperty("status")] public string Status { get; set; }
    }
}