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
        const ulong GuildID = 506248420525342769;
        const ulong ChannelID = 670888046480195585;

        [Command("Suggest")]
        [Summary("Send a suggestion for a feature! Please use this command responsibly")]
        public async Task AddSuggestion(params string[] Suggestion)
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.BlacklistSave.Contains(Context.User.Id))
            {
                await Error.SendDiscordError(Context, Value: "You are banned from using this command");
                return;
            }
            if (Suggestion.Length == 0)
            {
                await Error.SendDiscordError(Context, "The input text has too few parameters.");
                return;
            }

            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.Teal,
                Title = $"Suggestion",
                Description = $"From {Context.User.Mention}:\n{string.Join(" ", Suggestion)}"
            };
            builder.WithCurrentTimestamp();

            await Context.Client.GetGuild(GuildID).GetTextChannel(ChannelID).SendMessageAsync(embed: builder.Build());

            await ReplyAsync("Thanks for the suggestion");
        }
    }
}
