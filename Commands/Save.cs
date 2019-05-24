using Discord.Commands;
using System;
using Discord.Rest;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using System.Net;
using System.Collections.Generic;

namespace RK800.Commands
{
    public class Save : ModuleBase<SocketCommandContext>
    {
        [Command("Save")]
        public async Task ForceSaving()
        {
            SaveHandler.SaveAll();
            await ReplyAsync("Saved all data!");
        }
    }
}