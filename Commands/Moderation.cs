using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Save;
using Marina.Utils;
using System.Threading.Tasks;

namespace Marina.Commands
{
    class Moderation : ModuleBase<SocketCommandContext>
    {
        [RequireUserPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.BanMembers)]
        [Command("Ban")]
        public async Task BanUser([RequireHierarchy]SocketGuildUser User, params string[] Reason)
        {
            await Context.Channel.TriggerTypingAsync();
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
            await Context.Channel.TriggerTypingAsync();
            string joined = Reason.Length != 0 ? string.Join(' ', Reason) : null;
            string msg = $"You were kicked from {Context.Guild.Name}";
            if (joined != null) msg += $"\nReason: {joined}";
            await User.SendMessageAsync(msg);
            await User.KickAsync(joined);
            await ReplyAsync($"kicked {User} :boot:");

            //Kicks need to be manually logged
            if (SaveHandler.LogSave.Data.ContainsKey(User.Guild.Id))
            {
                SocketTextChannel LogChannel = User.Guild.GetTextChannel(SaveHandler.LogSave.Data[User.Guild.Id]);
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
                else SaveHandler.LogSave.Data.Remove(User.Guild.Id);
            }
        }
    }
}
