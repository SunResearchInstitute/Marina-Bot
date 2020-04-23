using Discord;
using Discord.Commands;
using Discord.Net;
using Marina.Utils;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Marina.Commands.Animals.Dog
{
    public class DogUpload : ModuleBase<SocketCommandContext>
    {
        [Command("Dog")]
        [Summary("Gets a random dog picture.")]
        public async Task UploadDog()
        {
            //This just downloads the json which should be fine
            DogData jsonData;
            using WebClient wc = new WebClient();
            try
            {
                jsonData = JsonConvert.DeserializeObject<DogData>(
                    wc.DownloadString("https://dog.ceo/api/breeds/image/random"));
            }
            catch (WebException e)
            {
                await Error.SendDiscordError(Context, value: $"{e.Message}", e: e);
                return;
            }

            wc.Dispose();
            if (jsonData.Status != "success")
                await Error.SendDiscordError(Context, value: "API Failed!", e: new Exception("Dog API Failed!"));
            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.Teal,
                ImageUrl = jsonData.ImageUrl.OriginalString
            };
            builder.WithCurrentTimestamp();
            builder.WithFooter("Taken from https://dog.ceo/dog-api/");
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