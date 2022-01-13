using Discord;
using Discord.Interactions;
using Marina.Utils;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Marina.Animals.Cat
{
    public class Cat : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("cat", "Gets a random cat picture.")]
        public async Task UploadCat()
        {
            using HttpClient client = new();
            CatData jsonData;
            try
            {
                jsonData = JsonConvert
                    .DeserializeObject<CatData[]>(
                        await client.GetStringAsync("https://api.thecatapi.com/v1/images/search?format=json")).First();
            }
            catch (WebException e)
            {
                await Error.SendDiscordError(Context, value: $"{e.Message}", e: e);
                return;
            }

            EmbedBuilder builder = new()
            {
                Color = Color.Teal,
                ImageUrl = jsonData.Url.OriginalString
            };
            if (jsonData.Categories?.First() != null)
                builder.Title = jsonData.Categories.First().Name;
            builder.WithCurrentTimestamp();
            builder.WithFooter("Taken from https://thecatapi.com/");

            await RespondAsync(embed: builder.Build());
        }
    }

    public partial class CatData
    {
        [JsonProperty("breeds")]
        public object[] Breeds { get; set; }

        [JsonProperty("categories", NullValueHandling = NullValueHandling.Ignore)]
        public Category[] Categories { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }
    }

    public partial class Category
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}