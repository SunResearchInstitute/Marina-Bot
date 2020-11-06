using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Attributes;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class User : ModuleBase<SocketCommandContext>
    {
        [Command("Avatar")]
        [Alias("pfp", "ava")]
        [Summary("Gets an avatar of self or another user.")]
        public async Task GetAvatar([ManualOptionalParameter("Self")] [Name("User")]
            SocketUser user = null)
        {
            user ??= Context.User;
            EmbedBuilder builder = new EmbedBuilder
            {
                Title = $"{user.Username}'s Avatar",
                Color = Color.Teal,
                ImageUrl = user.GetAvatarUrl(ImageFormat.Auto, 2048) ?? user.GetDefaultAvatarUrl()
            };
            builder.WithCurrentTimestamp();

            await ReplyAsync(embed: builder.Build());
        }
    }
}