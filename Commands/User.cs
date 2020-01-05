using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class User : ModuleBase<SocketCommandContext>
    {
        [Command("Avatar")]
        [Summary("Gets a avatar of self or another user.")]
        public async Task GetAvatar(IUser User = null)
        {
            await Context.Channel.TriggerTypingAsync();
            if (User == null)
                User = Context.User;

            EmbedBuilder builder = new EmbedBuilder
            {
                Title = $"{User.Username}'s Avatar",
                Color = Color.Teal,
                ImageUrl = User.GetAvatarUrl(ImageFormat.Auto, 2048),
            };
            builder.WithCurrentTimestamp();

            await ReplyAsync(embed: builder.Build());
        }
    }
}
