using Discord;
using System.Threading.Tasks;
using System;
using Discord.Commands;
using Discord.Rest;

namespace RK800.Utils
{
    public class Error
    {
        public enum ExceptionType { User, Fatal }
        public static async Task SendDiscordError(SocketCommandContext Context, string Key = "An error has occured.", string Value = "View the help menu for help.", Exception e = null, ExceptionType et = ExceptionType.User)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Error");
            builder.AddField(Key, Value);
            builder.WithColor(Color.Red);
            builder.WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync(embed: builder.Build());
            if (et == ExceptionType.Fatal)
            {
                EmbedBuilder errorbuilder = new EmbedBuilder();
                errorbuilder.WithCurrentTimestamp();
                errorbuilder.WithColor(Color.Red);
                errorbuilder.WithDescription($"```{e.Message}\n\n{e.Source}\n{e.StackTrace}```");
                //should we send anything else?
                RestApplication info = await Context.Client.GetApplicationInfoAsync();
                await info.Owner.SendMessageAsync(embed: errorbuilder.Build());
            }
        }

        public static void SendApplicationError(string ErrorMsg, int code = 0)
        {
            Console.WriteLine(ErrorMsg);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Environment.Exit(code);
        }
    }
}
