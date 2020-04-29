using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Utils;
using Console = Marina.Utils.Console;

namespace Marina
{
    internal static class Program
    {
#pragma warning disable 8618
        //API Stuff
        private static DiscordSocketClient _client;
        public static CommandService Commands { get; private set; }

        public static event EventHandler<DiscordSocketClient> Initialize;
#pragma warning restore 8618

        private static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
            Thread.Sleep(-1);
        }

        private static async Task MainAsync()
        {
            //This just configures how we want to handle our client and commands
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                //Caching for Moderation
                MessageCacheSize = 250,
                LogLevel = LogSeverity.Error
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Error
            });

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            Commands.Log += async delegate (LogMessage log)
            {
                await Console.WriteLog($"[{DateTime.Now}]: {log.ToString()}\n");
            };
            _client.MessageReceived += MessageReceived;
            _client.Connected += async delegate { await Console.WriteLog("Initialized!"); };
            Initialize?.Invoke(null, _client);

            Dictionary<string, string> config = Misc.LoadConfig();
            try
            {
                await _client.LoginAsync(TokenType.Bot, config["token"]);
                await _client.StartAsync();
            }
            catch
            {
                Error.SendApplicationError("Something went wrong, check if your Token is valid!", 1);
            }
        }

        private static async Task MessageReceived(SocketMessage arg)
        {
            //Welcomes are considered a message and are null
            if (!(arg is SocketUserMessage message))
                return;

            SocketCommandContext context = new SocketCommandContext(_client, message);
            int prefixPos = 0;

            if (string.IsNullOrWhiteSpace(context.Message.Content) || context.User.IsBot)
                return;

            if (context.Guild != null)
            {
#if DEBUG
                if (!message.HasStringPrefix("d.", ref prefixPos))
#else
                if (!message.HasStringPrefix("m.", ref prefixPos))
#endif
                    return;
            }


            await context.Channel.TriggerTypingAsync();
            IResult result = await Commands.ExecuteAsync(context, prefixPos, null);
            if (!result.IsSuccess)
                await Error.SendDiscordError(context, result.ErrorReason);
            else
                await Console.WriteLog($"{context.User} ({context.User.Id}) executed command: {context.Message}");
        }
    }
}