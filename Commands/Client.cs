using Discord.Commands;
using System.Threading.Tasks;
using RK800.Save;
using System;

namespace RK800.Commands
{
    public class Client : ModuleBase<SocketCommandContext>
    {
        [Command("Shutdown"), Alias("Quit")]
        [RequireOwner]
        public async Task Shutdown()
        {
            await ReplyAsync("Shutting down!");
            await Context.Client.LogoutAsync();
            SaveHandler.SaveAll();
            Environment.Exit(0);
        }
    }
}