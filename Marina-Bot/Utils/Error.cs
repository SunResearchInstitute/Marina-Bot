using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Marina.Utils
{
    public static class Error
    {
        public static async Task SendDiscordError(IInteractionContext context, string key = "An error has occured.",
            string value = "View the help menu for help.", Exception e = null)
        {
            EmbedBuilder builder = new()
            {
                Title = "Error",
                Color = Color.Red
            };
            builder.AddField(key, value);
            builder.WithCurrentTimestamp();
            await context.Interaction.RespondAsync(embed: builder.Build());
            if (e != null)
            {
                builder.Title = string.Empty;
                builder.Fields.Clear();

                builder.Description = $"```{e.Message}\n\n{e.Source}\n{e.StackTrace}```";
                builder.WithCurrentTimestamp();
                //should we send anything else?
                await (await context.Client.GetApplicationInfoAsync()).Owner.SendMessageAsync(embed: builder.Build());
            }
        }

        public static async Task SendDiscordError(ICommandContext context, string key = "An error has occured.",
            string value = "View the help menu for help.", Exception e = null)
        {
            EmbedBuilder builder = new()
            {
                Title = "Error",
                Color = Color.Red
            };
            builder.AddField(key, value);
            builder.WithCurrentTimestamp();
            await context.Channel.SendMessageAsync(embed: builder.Build());
            if (e != null)
            {
                builder.Title = string.Empty;
                builder.Fields.Clear();

                builder.Description = $"```{e.Message}\n\n{e.Source}\n{e.StackTrace}```";
                builder.WithCurrentTimestamp();
                //should we send anything else?
                await (await context.Client.GetApplicationInfoAsync()).Owner.SendMessageAsync(embed: builder.Build());
            }
        }

        public static void SendApplicationError(string errorMsg, int code = 0)
        {
            _ = Console.WriteLog($"EXCEPTION/ERROR: {errorMsg}");
            _ = Console.WriteLog("Press any key to continue...");
            System.Console.ReadKey();
            Environment.Exit(code);
        }
    }
}