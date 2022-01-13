using Discord;
using Discord.Interactions;
using Marina.Utils;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Marina.Interactions.Dog
{
    public class Dog : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("dog", "Gets a random dog picture.")]
        public async Task UploadDog()
        {
            //This just downloads the json which should be fine
            DogData jsonData;
            using HttpClient client = new();
            try
            {
                jsonData = JsonConvert.DeserializeObject<DogData>(
                    await client.GetStringAsync("https://dog.ceo/api/breeds/image/random"));
            }
            catch (WebException e)
            {
                await Error.SendDiscordError(Context, value: $"{e.Message}", e: e);
                return;
            }

            if (jsonData.Status != "success")
                await Error.SendDiscordError(Context, value: "API Failed!", e: new Exception("Dog API Failed!"));
            EmbedBuilder builder = new()
            {
                Color = Color.Teal,
                ImageUrl = jsonData.ImageUrl.OriginalString
            };
            builder.WithCurrentTimestamp();
            builder.WithFooter("Taken from https://dog.ceo/dog-api/");

            await RespondAsync(embed: builder.Build());
        }
    }

    public partial class DogData
    {
        [JsonProperty("message")] public Uri ImageUrl { get; set; }

        [JsonProperty("status")] public string Status { get; set; }
    }
}