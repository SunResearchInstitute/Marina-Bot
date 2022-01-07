using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace Marina.Interactions
{
    public class User : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("avatar", "Gets an avatar of self or another user.")]
        public async Task GetAvatar(IUser user = null)
        {
            user ??= Context.User;
            EmbedBuilder builder = new()
            {
                Title = $"{user.Username}'s Avatar",
                Color = Color.Teal,
                ImageUrl = user.GetAvatarUrl(ImageFormat.Auto, 2048) ?? user.GetDefaultAvatarUrl()
            };
            builder.WithCurrentTimestamp();

            await RespondAsync(embed: builder.Build());
        }
    }
}