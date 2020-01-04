using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Marina.Commands;
using Marina.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Marina
{
    class Program
    {
        //API Stuff
        public static DiscordSocketClient Client;
        public static CommandService Commands;

        //Config Stuff
        private static readonly Dictionary<string, string> Config = new Dictionary<string, string>();
        private static readonly DirectoryInfo BaseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        private static readonly FileInfo ConfigFile = BaseDirectory.GetFile("Config.txt");
        public static readonly FileInfo LogFile = BaseDirectory.GetFile("Marina.log");

        static void Main()
        {
            LoadConfig();
            Program program = new Program();
            program.MainAsync().GetAwaiter().GetResult();
            Thread.Sleep(-1);
        }

        private async Task MainAsync()
        {
            //This just configures how we want to handle our client and commands
            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                //Chaching for Moderation
                MessageCacheSize = 100,
                LogLevel = LogSeverity.Error
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Error
            });

            Client.Ready += Client_Ready;
            Client.MessageReceived += MessageReceived;
            Client.MessageDeleted += MessageDeleted;
            Client.UserLeft += UserLeft;
            Client.UserBanned += UserBanned;
            Client.MessageUpdated += MessageUpdated;
            Client.GuildMemberUpdated += GuildMemberUpdated;
            Client.Log += Log;
            Commands.Log += Log;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            try
            {
                await Client.LoginAsync(TokenType.Bot, Config["token"]);
            }
            catch (HttpException)
            {
                Error.SendApplicationError("Token is invalid!", -1);
            }
            //a Static Method Starts too early
            Help.Populate();
            await Client.StartAsync();
        }

        private Task Log(LogMessage log)
        {
            using (StreamWriter writer = File.AppendText(LogFile.FullName))
            {
                writer.WriteLine($"{log.Source} {log.Exception.Message}: {log.Exception.Message} {log.Exception.StackTrace}");
            }

            return Task.CompletedTask;
        }

        private async Task UserBanned(SocketUser User, SocketGuild Guild)
        { 

        }

        private async Task UserLeft(SocketGuildUser User)
        {

        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> OldMessage, SocketMessage NewMessage, ISocketMessageChannel Channel)
        {

        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> Message, ISocketMessageChannel Channel)
        {
            
        }

        private async Task GuildMemberUpdated(SocketGuildUser Before, SocketGuildUser After)
        {
            if (Before.IsBot) return;

            
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            //Welcomes are considered a msg and are null
            if (!(arg is SocketUserMessage Message)) return;
            SocketCommandContext Context = new SocketCommandContext(Client, Message);
            int PrefixPos = 0;

            if (string.IsNullOrWhiteSpace(Context.Message.Content) || Context.User.IsBot) return;

            if (Context.Guild != null)
            {
#if DEBUG
                if (!Message.HasStringPrefix("d.", ref PrefixPos)) return;
#else
                if (!Message.HasStringPrefix("m.", ref PrefixPos)) return;
#endif
            }

            IResult Result = await Commands.ExecuteAsync(Context, PrefixPos, null);
            if (!Result.IsSuccess) await Error.SendDiscordError(Context, Result.ErrorReason);
        }

        private async Task Client_Ready()
        {
            Utils.Console.ConsoleWriteLog("Ready!");
            while (true)
            {
                if (Client.Guilds.Count > 1) await Client.SetGameAsync($"on {Client.Guilds.Count} servers | c.help");
                else await Client.SetGameAsync($"on {Client.Guilds.Count} server | c.help");
                await Task.Delay(600000);
            }
        }

        private static void LoadConfig()
        {
            if (ConfigFile.Exists)
            {
                foreach (string line in File.ReadAllLines(ConfigFile.FullName))
                {
                    //Even though we do not verify any Config items we should be fine
                    string[] configitems = line.Split('=');
                    Config.Add(configitems[0].ToLower(), configitems[1]);
                }
            }
            else
            {
                string[] configtemplate = new string[]
                {
                    "Token={token}"
                };
                File.WriteAllLines(ConfigFile.FullName, configtemplate);
                Error.SendApplicationError("Config does not exist, it has been created for you!");
            }
        }
    }
}
