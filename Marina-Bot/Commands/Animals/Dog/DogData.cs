using System;
using Newtonsoft.Json;

namespace Marina.Commands.Animals.Dog
{
    public class DogData
    {
        [JsonProperty("message")] public Uri ImageUrl { get; set; }

        [JsonProperty("status")] public string Status { get; set; }
    }
}