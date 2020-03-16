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
        public async Task BanUser([RequireHierarchy]SocketGuildUser User, params string[] Reason)
        {
            string reasonJoined = Reason.Length != 0 ? string.Join(' ', Reason) : null;
            string msg = $"You were banned from {Context.Guild.Name}";
            if (reasonJoined != null) msg += $"\nReason: {reasonJoined}";
            await User.SendMessageAsync(msg);
            await User.BanAsync(reason: reasonJoined);
            await ReplyAsync($"{User} is now b& :thumbsup:");
            //Bans will automatically logged
        }

        [RequireUserPermission(GuildPermission.KickMembers), RequireBotPermission(GuildPermission.KickMembers)]
        [Command("Kick")]
        public async Task Kickuser([RequireHierarchy]SocketGuildUser User, params string[] Reason)
        {
            string joined = Reason.Length != 0 ? string.Join(' ', Reason) : null;
            string msg = $"You were kicked from {Context.Guild.Name}";
            if (joined != null) msg += $"\nReason: {joined}";
            await User.SendMessageAsync(msg);
            await User.KickAsync(joined);
            await ReplyAsync($"kicked {User} :boot:");

            //Kicks need to be manually logged
            if (SaveHandler.LogSave.ContainsKey(User.Guild.Id))
            {
                SocketTextChannel LogChannel = User.Guild.GetTextChannel(SaveHandler.LogSave[User.Guild.Id]);
                if (LogChannel != null)
                {
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Color = Color.Teal,
                        Title = "**Kicked**",
                        Description = $"{Context.User.Mention} kicked {User.Mention} | {User}"
                    };
                    if (joined != null) builder.Description += $"\n__Reason__: \"{joined}\"";
                    await LogChannel.SendMessageAsync(embed: builder.Build());
                }
                else SaveHandler.LogSave.Remove(User.Guild.Id);
            }
        }

        [Command("Purge")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task Purge(int Count = 50)
        {
            if (Count <= 0)
            {
                await Error.SendDiscordError(Context, Value: "Invalid parameters");
                return;
            }

            int rmCnt = 0;
            foreach (IMessage msg in await Context.Channel.GetMessagesAsync(Count).FlattenAsync())
            {
                try
                {
                    await msg.DeleteAsync();
                    rmCnt++;
                }
                catch { }
            }

            IUserMessage resultMsg = await ReplyAsync($"Removed {rmCnt - 1} messages");
            await Task.Delay(TimeSpan.FromSeconds(3));
            try
            {
                await resultMsg.DeleteAsync();
            }
            catch { }
        }
    }
}
