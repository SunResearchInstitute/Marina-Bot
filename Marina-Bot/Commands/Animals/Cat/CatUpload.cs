using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Marina.Utils;
using Newtonsoft.Json;

namespace Marina.Commands.Animals.Cat
{
    public class CatUpload : ModuleBase<SocketCommandContext>
    {
        [Command("Cat")]
        [Summary("Gets a random cat picture.")]
        public async Task UploadCat()
        {
            using WebClient wc = new WebClient();
            CatData jsonData;
            try
            {
                jsonData = JsonConvert
                    .DeserializeObject<CatData[]>(
                        wc.DownloadString("https://api.thecatapi.com/v1/images/search?format=json")).First();
            }
            catch (WebException e)
            {
                await Error.SendDiscordError(Context, value: $"{e.Message}", e: e);
                return;
            }

            wc.Dispose();

            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.Teal,
                ImageUrl = jsonData.Url.OriginalString
            };
            if (jsonData.Categories?.First() != null)
                builder.Title = jsonData.Categories.First().Name;
            builder.WithCurrentTimestamp();
            builder.WithFooter("Taken from https://thecatapi.com/");
            try
            {
                await Context.User.SendMessageAsync(embed: builder.Build());
            }
            catch (HttpException)
            {
                await ReplyAsync("Unable to send DM!");
            }
        }
    }
}