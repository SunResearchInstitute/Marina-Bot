using Discord;
using System.Threading.Tasks;
using System;
using Discord.Commands;
using Discord.Rest;

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
                EmbedBuilder errorbuilder = new EmbedBuilder
                {
                    Color = Color.Red,
                    Description = $"```{e.Message}\n\n{e.Source}\n{e.StackTrace}```"
                };
                errorbuilder.WithCurrentTimestamp();
                //should we send anything else?
                RestApplication info = await Context.Client.GetApplicationInfoAsync();
                await info.Owner.SendMessageAsync(embed: errorbuilder.Build());
            }
        }

        public static void SendApplicationError(string ErrorMsg, int code = 0)
        {
            Console.ConsoleWriteLog(ErrorMsg);
            Console.ConsoleWriteLog("Press any key to continue...");
            System.Console.ReadKey();
            Environment.Exit(code);
        }
    }
}
