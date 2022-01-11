using Discord;
using Discord.Interactions;
using Marina.Utils;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Marina.Interactions.Animals.Cat
{
    public class CatUpload : InteractionModuleBase<SocketInteractionContext>
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
}