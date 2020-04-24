using Newtonsoft.Json;
using System;

namespace Marina.Commands.Animals.Dog
{
    public class DogData
    {
#pragma warning disable 8618
        [JsonProperty("message")] public Uri ImageUrl { get; set; }
#pragma warning restore 8618

#pragma warning disable 8618
        [JsonProperty("status")] public string Status { get; set; }
#pragma warning restore 8618
    }
}