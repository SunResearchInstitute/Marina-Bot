using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Marina.Utils;

namespace Marina.Commands
{
    public class Lmddgtfy : ModuleBase<SocketCommandContext>
    {
        [Command("lmddgtfy")]
        [Alias("real_help")]
        public async Task Ping([Name("UWU")] string uwu)
        {
            try
            {
                string owo = "https://lmddgtfy.net/?q=" + uwu.Replace(" ", "%20");
                await Context.Channel.SendMessageAsync(owo);
            }
            catch
            {
                await Error.SendDiscordError(Context, value: "OWO sumtwing went wong!");
            }
        }
    }
}