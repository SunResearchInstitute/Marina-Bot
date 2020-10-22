using Discord;
using Discord.Commands;
using Marina.Save;
using Marina.Utils;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Suggestion : ModuleBase<SocketCommandContext>
    {
        //IDs to my discord and desired suggestions output
        private const ulong GuildId = 506248420525342769;
        private const ulong ChannelId = 670888046480195585;

        [Command("Suggest")]
        [Summary("Send a suggestion for a feature! Please use this command responsibly")]
        public async Task AddSuggestion([Name("Suggestion")] params string[] suggestion)
        {
            if (SaveHandler.BlacklistSave.Contains(Context.User.Id))
            {
                await Error.SendDiscordError(Context, value: "You are banned from using this command");
                return;
            }

            if (suggestion == null || suggestion.Length == 0)
            {
                await Error.SendDiscordError(Context, "The input text has too few parameters.");
                return;
            }

            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.Teal,
                Title = "Suggestion",
                Description = $"From {Context.User.Mention} | {Context.User}:\n{string.Join(" ", suggestion)}"
            };
            builder.WithCurrentTimestamp();

            await Context.Client.GetGuild(GuildId).GetTextChannel(ChannelId).SendMessageAsync(embed: builder.Build());

            await ReplyAsync("Thanks for the suggestion");
        }
    }
}