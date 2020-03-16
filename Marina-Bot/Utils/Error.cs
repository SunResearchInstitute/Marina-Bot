using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Marina.Utils
{
    public class Error
    {
        public static async Task SendDiscordError(SocketCommandContext Context, string Key = "An error has occured.", string Value = "View the help menu for help.", Exception e = null)
        {
            EmbedBuilder builder = new EmbedBuilder
            {
                Title = "Error",
                Color = Color.Red
            };
            builder.AddField(Key, Value);
            builder.WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync(embed: builder.Build());
            if (e != null)
            {
                builder.Title = string.Empty;
                builder.Fields.Clear();

                builder.Description = $"```{e.Message}\n\n{e.Source}\n{e.StackTrace}```";
                builder.WithCurrentTimestamp();
                //should we send anything else?
                await (await Context.Client.GetApplicationInfoAsync()).Owner.SendMessageAsync(embed: builder.Build());
            }
        }

        public static void SendApplicationError(string ErrorMsg, int code = 0)
        {
            _ = Console.WriteLog(ErrorMsg);
            _ = Console.WriteLog("Press any key to continue...");
            System.Console.ReadKey();
            Environment.Exit(code);
        }
    }
}
