using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RK800.Utils;
using RK800.Commands;
using Discord.Net;

namespace RK800
{
    class Program
    {
        //API Stuff
        public static DiscordSocketClient Client;
        private CommandService Commands;

        //Config Stuff
        private static Dictionary<string, string> Config = new Dictionary<string, string>();
        private static FileInfo ConfigFile = new FileInfo("Config.txt");

        //private StreamWriter Log = File.AppendText(new FileInfo("Connor.log").FullName);

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
            Client.GuildMemberUpdated += GuildMemberUpdated;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            try
            {
                await Client.LoginAsync(TokenType.Bot, Config["token"]);
            }
            catch (HttpException)
            {
                Error.SendApplicationError("Token is invalid!");
            }
            await Client.StartAsync();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (TrackerContext.Trackers.Keys.Contains(after.Id))
            {
                Tracker.Trackers[after.Id] = DateTime.Now;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task JoinedGuild(SocketGuild arg)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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

            if (Context.Guild != null)
                if (!Message.HasStringPrefix("c.", ref PrefixPos))
                    return;

            if (string.IsNullOrWhiteSpace(Context.Message.Content) || Context.User.IsBot) return;

            //TODO: implement Banned Users

            IResult Result = await Commands.ExecuteAsync(Context, PrefixPos, null);
            if (!Result.IsSuccess)
            {
                await Error.SendDiscordError(Context, Key: Result.ErrorReason);
                //Should we log items?
            }
        }

        private async Task Client_Ready()
        {
            Console.WriteLine("Ready!");

            await Client.SetGameAsync("with Sumo");
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
                Error.SendApplicationError("Config Does not exist, it has been created for you!");
            }
        }
    }
}
