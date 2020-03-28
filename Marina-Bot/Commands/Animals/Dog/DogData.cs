namespace Marina.Commands.Animals.Dog
{
    using Newtonsoft.Json;
    using System;

    public class DogData
    {
        [JsonProperty("message")] public Uri ImageUrl { get; set; }

        [JsonProperty("status")] public string Status { get; set; }
    }
}