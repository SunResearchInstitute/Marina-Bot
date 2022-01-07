using Discord;
using Discord.Interactions;
using Marina.Utils;
using System.Threading.Tasks;

namespace Marina.Interactions
{
    public class Guild : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("servericon", "Gets a image of the server icon.")]
        public async Task GetServerIcon()
        {
            if (Context.Guild == null)
            {
                await Error.SendDiscordError(Context, value: "You cannot use this command here.");
                return;
            }

            EmbedBuilder builder = new()
            {
                Title = $"Server Icon",
                Color = Color.Teal,
                ImageUrl = Context.Guild.IconUrl
            };
            builder.WithCurrentTimestamp();

            await RespondAsync(embed: builder.Build());
        }
    }
}
