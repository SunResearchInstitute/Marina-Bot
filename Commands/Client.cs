using Discord.Commands;
using Marina.Save;
using System;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Client : ModuleBase<SocketCommandContext>
    {
        [Command("Shutdown"), Alias("Quit")]
        [RequireOwner]
        public async Task Shutdown()
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync("Shutting down!");
            await Context.Client.LogoutAsync();
            SaveHandler.SaveAll();
            Utils.Console.ConsoleWriteLog("Shutdown via Commmand!");
            Environment.Exit(0);
        }
    }
}
