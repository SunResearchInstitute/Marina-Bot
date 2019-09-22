using Discord.Commands;
using Marina.Save;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Save : ModuleBase<SocketCommandContext>
    {
        [Command("Save")]
        [RequireOwner]
        public async Task ForceSaving()
        {
            await Context.Channel.TriggerTypingAsync();
            SaveHandler.SaveAll();
            await ReplyAsync("Saved all data!");
        }
    }
}
