using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Save;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Logging : ModuleBase<SocketCommandContext>
    {
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("SetLogs"), Summary("Sets the logging channel.")]
        public async Task SetLogs(SocketTextChannel Channel)
        {
            if (SaveHandler.LogSave.Data.ContainsKey(Context.Guild.Id))
                SaveHandler.LogSave.Data[Context.Guild.Id] = Channel.Id;

            else
                SaveHandler.LogSave.Data.Add(Context.Guild.Id, Channel.Id);

            await ReplyAsync($"Logs will now be put in {Channel.Name}");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("RemoveLogs"), Summary("Removes the logging channel.")]
        public async Task RemoveLogs()
        {
            if (SaveHandler.LogSave.Data.Remove(Context.Guild.Id))
                await ReplyAsync("Logs removed.");
            else
                await ReplyAsync("No logs to remove.");
        }
    }
}
