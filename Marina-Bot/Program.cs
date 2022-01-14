using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Marina.Save;
using Marina.Save.Types;
using Marina.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Console = Marina.Utils.Console;

namespace Marina
{
    internal static class Program
    {
        //API Stuff
        public static event EventHandler<ServiceProvider> Initialize;

        private static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            using var services = ConfigureServices();

            var client = services.GetRequiredService<DiscordSocketClient>();
            var interactions = services.GetRequiredService<InteractionService>();
            SaveHandler.RegisterSaves();
            ConfigFile config = SaveHandler.Config;

            try
            {
                await client.LoginAsync(TokenType.Bot, config.Data.BotToken);
                await client.StartAsync();
            }
            catch (Exception e)
            {
                Error.SendApplicationError(e.Message, 1);
                return;
            }
            client.Ready += async () =>
            {
                try
                {
                    if (!IsDebug())
                    {
                        await interactions.RegisterCommandsGloballyAsync(true);
                        await Console.WriteLog("Registering commands globally");
                    }
                    else
                    {
                        await interactions.RegisterCommandsToGuildAsync(config.Data.Debug_GuildId, true);
                        await Console.WriteLog($"Registering commands to guild {config.Data.Debug_GuildId}");
                    }
                }
                catch (Exception e)
                {
                    Error.SendApplicationError(e.Message, 1);
                    return;
                }

                Initialize?.Invoke(null, services);

                await Console.WriteLog("Initialized!");
            };

            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }


        static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                {
                    //Caching for Moderation
                    MessageCacheSize = 150,
                    LogLevel = LogSeverity.Error
                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }

        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}