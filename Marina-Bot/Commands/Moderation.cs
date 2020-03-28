using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Save;
using Marina.Utils;
using System;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        [RequireUserPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.BanMembers)]
        [Command("Ban")]
        public async Task BanUser([Name("User")][RequireHierarchy]SocketGuildUser user, [Name("Reason")]params string[] reason)
        {
            string reasonJoined = reason.Length != 0 ? string.Join(' ', reason) : null;
            string msg = $"You were banned from {Context.Guild.Name}";
            if (reasonJoined != null) msg += $"\nReason: {reasonJoined}";
            await user.SendMessageAsync(msg);
            await user.BanAsync(reason: reasonJoined);
            await ReplyAsync($"{user} is now b& :thumbsup:");
            //Bans will automatically logged
        }

        [RequireUserPermission(GuildPermission.KickMembers), RequireBotPermission(GuildPermission.KickMembers)]
        [Command("Kick")]
        public async Task KickUser([Name("User")][RequireHierarchy]SocketGuildUser user, [Name("Reason")]params string[] reason)
        {
            string joined = reason.Length != 0 ? string.Join(' ', reason) : null;
            string msg = $"You were kicked from {Context.Guild.Name}";
            if (joined != null) msg += $"\nReason: {joined}";
            await user.SendMessageAsync(msg);
            await user.KickAsync(joined);
            await ReplyAsync($"kicked {user} :boot:");

            //Kicks need to be manually logged
            if (SaveHandler.LogSave.ContainsKey(user.Guild.Id))
            {
                SocketTextChannel logChannel = user.Guild.GetTextChannel(SaveHandler.LogSave[user.Guild.Id]);
                EmbedBuilder builder = new EmbedBuilder
                {
                    Color = Color.Teal,
                    Title = "**Kicked**",
                    Description = $"{Context.User.Mention} kicked {user.Mention} | {user}"
                };
                if (joined != null) builder.Description += $"\n__Reason__: \"{joined}\"";
                await logChannel.SendMessageAsync(embed: builder.Build());
            }
        }

        [Command("Purge")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task Purge([Name("Count")]int count = 50)
        {
            if (count <= 0)
            {
                await Error.SendDiscordError(Context, value: "Invalid parameters");
                return;
            }

            int rmCnt = 0;
            foreach (IMessage msg in await Context.Channel.GetMessagesAsync(count).FlattenAsync())
            {
                try
                {
                    await msg.DeleteAsync();
                    rmCnt++;
                }
                catch
                {
                    // ignored
                }
            }

            IUserMessage resultMsg = await ReplyAsync($"Removed {rmCnt - 1} messages");
            await Task.Delay(TimeSpan.FromSeconds(3));
            try
            {
                await resultMsg.DeleteAsync();
            }
            catch
            {
                // ignored
            }
        }
    }
}
