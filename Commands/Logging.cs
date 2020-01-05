using Discord.Commands;
using Discord.WebSocket;
using Marina.Save;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Logging : ModuleBase<SocketCommandContext>
    {
        [Command("SetLogs")]
        public async Task SetLogs(SocketTextChannel Channel)
        {
            if (SaveHandler.LogSaveFile.Data.ContainsKey(Context.Guild.Id))
            {
                SaveHandler.LogSaveFile.Data[Context.Guild.Id] = Channel.Id;
            }
            else
            {
                SaveHandler.LogSaveFile.Data.TryAdd(Context.Guild.Id, Channel.Id);
            }
            await ReplyAsync($"Logs will now be put in {Channel.Name}");
        }

        [Command("RemoveLogs")]
        public async Task RemoveLogs()
        {
            if (SaveHandler.LogSaveFile.Data.Remove(Context.Guild.Id))
                await ReplyAsync("Logs removed.");
            else
                await ReplyAsync("No logs to remove.");
        }
    }
}
