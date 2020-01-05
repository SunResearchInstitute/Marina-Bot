using Discord.Commands;
using Marina.Save;
using Marina.Utils;
using System;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Client : ModuleBase<SocketCommandContext>
    {
        [Command("Suggest")]
        public async Task AddSuggestion(params string[] Suggestion)
        {
            if (SaveHandler.BlacklistSave.Data.Contains(Context.User.Id))
            {
                await Error.SendDiscordError(Context, Value: "You are banned from using this command");
                return;
            }
            if (Suggestion.Length == 0)
            {
                await Error.SendDiscordError(Context, "The input text has too few parameters.");
                return;
            }

            SaveHandler.SuggestionsSave.Data.Add(Context.User.Id, string.Join(" ", Suggestion));
            await ReplyAsync("Thanks for the suggestion");
        }

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

        [Command("Say")]
        [RequireOwner]
        public async Task ForceSay(string str)
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }
            await ReplyAsync(str);
        }
    }
}
