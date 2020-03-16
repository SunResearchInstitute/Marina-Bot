using Discord;
using Discord.Commands;
using Discord.Net;
using Marina.Commands.Animals.Cat;
using Marina.Utils;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Marina.Commands.Animals
{
    public class CatUpload : ModuleBase<SocketCommandContext>
    {
        [Command("Cat")]
        [Summary("Gets a random cat picture.")]
        public async Task UploadCat()
        {
            using WebClient wc = new WebClient();
            CatData jsondata;
            try
            {
                jsondata = JsonConvert.DeserializeObject<CatData[]>(wc.DownloadString("https://api.thecatapi.com/v1/images/search?format=json")).First();
            }
            catch (WebException e)
            {
                await Error.SendDiscordError(Context, Value: $"{e.Message}", e: e);
                return;
            }
            wc.Dispose();

            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.Teal,
                ImageUrl = jsondata.Url.OriginalString
            };
            if (jsondata.Categories != null && jsondata.Categories.First() != null && jsondata.Categories.First().Name != null)
                builder.Title = jsondata.Categories.First().Name;
            builder.WithCurrentTimestamp();
            builder.WithFooter("Taken from https://thecatapi.com/");
            try
            {
                await Context.User.SendMessageAsync(embed: builder.Build());
            }
            catch (HttpException)
            {
                await ReplyAsync("Unable to send DM!");
                return;
            }
        }
    }
}
