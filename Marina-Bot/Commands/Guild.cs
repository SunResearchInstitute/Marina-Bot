using Discord;
using Discord.Commands;
using Marina.Utils;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Guild : ModuleBase<SocketCommandContext>
    {
        [Command("ServerIcon")]
        [Alias("Server")]
        [Summary("Gets a image of the server icon.")]
        public async Task GetServerIcon()
        {
            if (Context.Guild == null)
            {
                await Error.SendDiscordError(Context, value: "You cannot use this command here.");
                return;
            }

            EmbedBuilder builder = new EmbedBuilder
            {
                Title = $"Server Icon",
                Color = Color.Teal,
                ImageUrl = Context.Guild.IconUrl
            };
            builder.WithCurrentTimestamp();

            await ReplyAsync(embed: builder.Build());
        }
    }
}
