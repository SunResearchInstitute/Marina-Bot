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
        private readonly ulong GuildId = SaveHandler.Config.Data.GuildId;
        private readonly ulong ChannelId = SaveHandler.Config.Data.ChannelId;

        [Command("Suggest")]
        [Summary("Send a suggestion for a feature! Please use this command responsibly")]
        public async Task AddSuggestion([Name("Suggestion")] params string[] suggestion)
        {
            if (SaveHandler.Config.Data.GuildId == 0L || SaveHandler.Config.Data.ChannelId == 0L)
            {
                await Error.SendDiscordError(Context, value: "This command has not been set up!");
                return;
            }
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
