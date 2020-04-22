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
        [Summary("For when you need to help someone out with an explanation.")]
        public async Task Lmddgtfy([Name("SEARCH_TERM")] string searchTerm)
        {
            try
            {
                string link = "https://lmddgtfy.net/?q=" + searchTerm.Replace(" ", "%20");
                await Context.Channel.SendMessageAsync(link);
            }
            catch
            {
                await Error.SendDiscordError(Context, value: "OWO sumtwing went wong!");
            }
        }
    }
}