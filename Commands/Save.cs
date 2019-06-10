using Discord.Commands;
using RK800.Save;
using System.Threading.Tasks;

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
