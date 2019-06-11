using Discord.Commands;
using RK800.Save;
using System;
using System.Threading.Tasks;

namespace RK800.Commands
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
            Console.WriteLine("Shutdown via Commmand!");
            Environment.Exit(0);
        }

        [Command("Source")]
        [Summary("Source Code!")]
        public async Task GetSource() => await ReplyAsync("I was written in C# using Discord.Net: https://github.com/SunTheCourier/Connor");
    }
}
