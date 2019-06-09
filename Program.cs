using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using RK800.Commands;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RK800.Utils;
using RK800.Save;
using Discord.Net;
using System.Timers;

namespace RK800
{
    class Program
    {
        //API Stuff
        public static DiscordSocketClient Client;
        public static CommandService Commands;

        //Config Stuff
        private static Dictionary<string, string> Config = new Dictionary<string, string>();
        private static FileInfo ConfigFile = new FileInfo("Config.txt");

        private static readonly System.Timers.Timer Timer = new System.Timers.Timer(60000)
        {
            AutoReset = true,
            Enabled = true,
        };

        static void Main()
        {
            LoadConfig();
            Timer.Elapsed += Tracker.CheckTimeAsync;
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
                DefaultRunMode = RunMode.Async
            });
            Client.Ready += Client_Ready;
            Client.MessageReceived += MessageReceived;
            //TODO: Moderation
            //Client.MessageDeleted += MessageDeleted;
            //Client.MessageUpdated += MessageUpdated;
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
            //Workaround until we have a save that starts earlier
            SaveHandler.Populate();
            //a Static Method Starts too early
            Help.Populate();
            await Client.StartAsync();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (before.Status != after.Status)
            {
                if (Tracker.TrackersSave.Data.Keys.Contains(after.Id) && Tracker.TrackersSave.Data[after.Id].IsTrackerEnabled)
                {
                    Tracker.TrackersSave.Data[after.Id].dt = DateTime.Now;
                }
            }
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            SocketUserMessage Message = arg as SocketUserMessage;
            //Welcomes are considered a msg and are null
            if (Message == null) return;
            SocketCommandContext Context = new SocketCommandContext(Client, Message);
            int PrefixPos = 0;

            if (string.IsNullOrWhiteSpace(Context.Message.Content) || Context.User.IsBot) return;

            if (Context.Guild != null)
            {
                if (Moderation.FilterSave.Data.ContainsKey(Context.Guild.Id) && Moderation.FilterSave.Data[Context.Guild.Id].IsEnabled && Moderation.MessageContainsFilteredWord(Context.Guild.Id, Context.Message.Content))
                {
                    await Context.Channel.TriggerTypingAsync();
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} Your language is highly uncalled for...");
                    await Task.Delay(150);
                    await Context.Message.DeleteAsync();
                    await Context.Channel.TriggerTypingAsync();
                    await Task.Delay(500);
                    await Context.Channel.SendMessageAsync("Thank you in advance for your cooperation.");
                    return;
                }
                if (!Message.HasStringPrefix("c.", ref PrefixPos)) return;
            }

            IResult Result = await Commands.ExecuteAsync(Context, PrefixPos, null);
            if (!Result.IsSuccess) await Error.SendDiscordError(Context, Key: Result.ErrorReason);
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
                Error.SendApplicationError("Config does not exist, it has been created for you!");
            }
        }
    }
}
