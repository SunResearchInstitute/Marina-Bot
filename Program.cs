using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Discord.Rest;
using RK800.Utils;

namespace RK800
{
    class Program
    {
        //API Stuff
        private DiscordSocketClient Client;
        private CommandService Commands;
        //Should this be avaliable like this?
        public static RestApplication Application;

        //Config Stuff
        private static Dictionary<string, string> Config = new Dictionary<string, string>();
        private static FileInfo ConfigFile = new FileInfo("Config.txt");

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
            Client = new DiscordSocketClient(new DiscordSocketConfig());
            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
            });
            //What we should do when the client is ready and when the client gets a message
            Client.Ready += Client_Ready;
            Client.MessageReceived += MessageReceived;
            Client.JoinedGuild += JoinedGuild;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            await Client.LoginAsync(TokenType.Bot, Config["token"]);
            await Client.StartAsync();
        }

        private async Task JoinedGuild(SocketGuild arg)
        {
            throw new NotImplementedException();
            //TODO: Send our Init info for security stuff
            //await arg.DefaultChannel.SendMessageAsync("");
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            SocketUserMessage Message = arg as SocketUserMessage;
            SocketCommandContext Context = new SocketCommandContext(Client, Message);

            int PrefixPos = 0;

            if (!Message.HasStringPrefix("c.", ref PrefixPos)) return;

            if (string.IsNullOrWhiteSpace(Context.Message.Content) || Context.User.IsBot || Context.Guild == null) return;

            //TODO: implement Banned Users

            IResult Result = await Commands.ExecuteAsync(Context, PrefixPos, null);
            if (!Result.IsSuccess)
            {
                await Error.Send(Message.Channel, Key: Result.ErrorReason);
                //TODO: Log to file
            }
        }

        private async Task Client_Ready()
        {
            Application = await Client.GetApplicationInfoAsync();
            Console.WriteLine("Ready!");

            await Client.SetGameAsync("28 Stab Wounds", type: ActivityType.Watching);
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
                Console.WriteLine("Config Does not exist, it has been created for you!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}
