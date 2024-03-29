﻿using Discord;
using Discord.Interactions;
using Marina.Save;
using Marina.Utils;
using System.Threading.Tasks;

namespace Marina.Interactions
{
    public class Suggestion : InteractionModuleBase<SocketInteractionContext>
    {
        //IDs to my discord and desired suggestions output
        private readonly ulong GuildId = SaveHandler.Config.Data.Suggestions_GuildId;
        private readonly ulong ChannelId = SaveHandler.Config.Data.Suggestions_ChannelId;

        [SlashCommand("suggest", "Send a suggestion for a feature! Please use this command responsibly")]
        public async Task AddSuggestion(string suggestion)
        {
            if (SaveHandler.Config.Data.Suggestions_GuildId == 0L || SaveHandler.Config.Data.Suggestions_ChannelId == 0L)
            {
                await Error.SendDiscordError(Context, value: "This command has not been set up!");
                return;
            }
            if (SaveHandler.BlacklistSave.Contains(Context.User.Id))
            {
                await Error.SendDiscordError(Context, value: "You are banned from using this command");
                return;
            }

            EmbedBuilder builder = new()
            {
                Color = Color.Teal,
                Title = "Suggestion",
                Description = $"From {Context.User.Mention} | {Context.User}:\n{suggestion}"
            };
            builder.WithCurrentTimestamp();

            await Context.Client.GetGuild(GuildId).GetTextChannel(ChannelId).SendMessageAsync(embed: builder.Build());

            await RespondAsync("Thanks for the suggestion");
        }
    }
}
