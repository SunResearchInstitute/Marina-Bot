using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Marina.Utils;

namespace Marina.Commands
{
    public class User : ModuleBase<SocketCommandContext>
    {
        [Command("Avatar")]
        [Alias("pfp", "ava")]
        [Summary("Gets a avatar of self or another user.")]
        public async Task GetAvatar([ManualOptionalParameter("Self")] [Name("User")]
            IUser user = null)
        {
            if (user == null)
                user = Context.User;

            EmbedBuilder builder = new EmbedBuilder
            {
                Title = $"{user.Username}'s Avatar",
                Color = Color.Teal,
                ImageUrl = user.GetAvatarUrl(ImageFormat.Auto, 2048)
            };
            builder.WithCurrentTimestamp();

            await ReplyAsync(embed: builder.Build());
        }
    }
}