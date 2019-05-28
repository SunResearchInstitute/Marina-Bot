using Discord.Commands;
using System.Threading.Tasks;
using RK800.Save;

namespace RK800.Commands
{
    public class Save : ModuleBase<SocketCommandContext>
    {
        [Command("Save")]
        [RequireOwner]
        public async Task ForceSaving()
        {
            SaveHandler.SaveAll();
            await ReplyAsync("Saved all data!");
        }
    }
}