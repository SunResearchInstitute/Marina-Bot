using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Save;
using Marina.Utils;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Logging : ModuleBase<SocketCommandContext>
    {
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("SetLogs"), Summary("Sets the logging channel.")]
        public async Task SetLogs([Name("Channel")][ManualOptionalParameter("Current Channel")]SocketTextChannel channel = null)
        {
            if (SaveHandler.LogSave.ContainsKey(Context.Guild.Id))
                SaveHandler.LogSave[Context.Guild.Id] = channel.Id;

            else
                SaveHandler.LogSave.Add(Context.Guild.Id, channel.Id);

            await ReplyAsync($"Logs will now be put in {channel.Name}");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("RemoveLogs"), Summary("Removes the logging channel.")]
        public async Task RemoveLogs()
        {
            if (SaveHandler.LogSave.Remove(Context.Guild.Id))
                await ReplyAsync("Logs removed.");
            else
                await ReplyAsync("No logs to remove.");
        }
    }
}
