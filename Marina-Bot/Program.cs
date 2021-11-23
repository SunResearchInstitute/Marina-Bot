using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Save;
using Marina.Save.Types;
using Marina.Utils;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Console = Marina.Utils.Console;

namespace Marina
{
    internal static class Program
    {
        //API Stuff
        private static DiscordSocketClient _client;
        public static CommandService Commands { get; private set; }

        public static event EventHandler<DiscordSocketClient> Initialize;

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
                MessageCacheSize = 150,
                LogLevel = LogSeverity.Error
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Error
            });
            SaveHandler.RegisterSaves();
            ConfigFile config = SaveHandler.Config;
            try
            {
                await _client.LoginAsync(TokenType.Bot, config.Data.Token);
                await _client.StartAsync();
            }
            catch (Exception e)
            {
                Error.SendApplicationError(e.Message, 1); ;
                return;
            }
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            Commands.Log += async delegate (LogMessage log)
            {
                if (log.Exception != null && log.Exception is CommandException exception && log.Exception.InnerException != null)
                    await Error.SendDiscordError((SocketCommandContext)exception.Context, value: "A Fatal error has occured. This has been reported.", e: log.Exception.InnerException);
                else
                    await Console.WriteLog($"[{DateTime.Now}]: {log.ToString()}\n");
            };
            Initialize?.Invoke(null, _client);
            _client.MessageReceived += MessageReceived;


            await Console.WriteLog("Initialized!");
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