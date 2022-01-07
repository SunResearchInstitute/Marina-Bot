using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Commands.Attributes;
using Marina.Save;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Logging : ModuleBase<SocketCommandContext>
    {
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("SetLogs")]
        [Summary("Sets the logging channel.")]
        public async Task SetLogs([Name("Channel")] [ManualOptionalParameter("Current Channel")]
            SocketTextChannel channel = null)
        {
            channel ??= (SocketTextChannel)Context.Channel;

            if (SaveHandler.LogSave.ContainsKey(Context.Guild.Id))
                SaveHandler.LogSave[Context.Guild.Id] = channel.Id;

            else
                SaveHandler.LogSave.Add(Context.Guild.Id, channel.Id);

            await ReplyAsync($"Logs will now be put in {channel.Mention}");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("RemoveLogs")]
        [Summary("Removes the logging channel.")]
        public async Task RemoveLogs()
        {
            if (SaveHandler.LogSave.Remove(Context.Guild.Id))
                await ReplyAsync("Logs removed.");
            else
                await ReplyAsync("No logs to remove.");
        }
    }
}