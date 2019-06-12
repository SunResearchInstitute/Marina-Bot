using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using RK800.Commands;
using RK800.Save;
using RK800.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
        private static FileInfo LogFile = new FileInfo("Connor.log");

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
            //Workaround until we have a save that starts earlier
            SaveHandler.Populate();
            //a Static Method Starts too early
            Help.Populate();
            await Client.StartAsync();
        }

        private Task Log(LogMessage log)
        {
            using (StreamWriter writer = File.AppendText(LogFile.FullName))
            {
                writer.WriteLine($"{log.Source} {log.Message} {log.Source}: {log.Exception.Message} {log.Exception.StackTrace}");
            }

            return Task.CompletedTask;
        }

        private async Task UserLeft(SocketGuildUser User)
        {
            if (SaveHandler.LogChannelsSave.Data.ContainsKey(User.Guild.Id))
            {
                SocketGuildChannel GuildChannel = User.Guild.GetChannel(SaveHandler.LogChannelsSave.Data[User.Guild.Id]);
                if (GuildChannel != null)
                {
                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithColor(Color.Blue);
                    builder.WithTitle("User Left");
                    builder.WithDescription(User.Mention);
                    ISocketMessageChannel LogChannel = GuildChannel as ISocketMessageChannel;
                    await LogChannel.SendMessageAsync(embed: builder.Build());
                }
                else SaveHandler.LogChannelsSave.Data.Remove(User.Guild.Id);
            }
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> OldMessage, SocketMessage NewMessage, ISocketMessageChannel Channel)
        {
            if (OldMessage.HasValue && OldMessage.Value.Content != NewMessage.Content)
            {
                SocketCommandContext Context = new SocketCommandContext(Client, OldMessage.Value as SocketUserMessage);
                if (!string.IsNullOrWhiteSpace(OldMessage.Value.Content) && SaveHandler.LogChannelsSave.Data.ContainsKey(Context.Guild.Id) && SaveHandler.LogChannelsSave.Data[Context.Guild.Id] != Context.Channel.Id)
                {
                    SocketGuildChannel GuildChannel = Context.Guild.GetChannel(SaveHandler.LogChannelsSave.Data[Context.Guild.Id]);
                    if (GuildChannel != null)
                    {
                        ISocketMessageChannel LogChannel = GuildChannel as ISocketMessageChannel;
                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithColor(Color.Blue);
                        builder.WithTitle("Message Edited");
                        builder.WithDescription($"From {NewMessage.Author.Mention} in <#{Channel.Id}>:\n**Before:**\n{OldMessage.Value.Content}\n**After:**\n{NewMessage.Content}");
                        if (builder.Description.Length > EmbedBuilder.MaxDescriptionLength)
                        {
                            string[] Msgs = Misc.ConvertToDiscordSendable(builder.Description, EmbedBuilder.MaxDescriptionLength);
                            for (int i = 0; i < Msgs.Length; i++)
                            {
                                string msg = Msgs[i];
                                builder.WithDescription(msg);
                                await LogChannel.SendMessageAsync(embed: builder.Build());
                                if (i == 0) builder.Title = null;
                            }
                        }
                        else await LogChannel.SendMessageAsync(embed: builder.Build());
                    }
                    else SaveHandler.LogChannelsSave.Data.Remove(Context.Guild.Id);
                }
            }
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> Message, ISocketMessageChannel Channel)
        {
            if (Message.HasValue)
            {
                SocketCommandContext Context = new SocketCommandContext(Client, Message.Value as SocketUserMessage);
                if (!string.IsNullOrWhiteSpace(Message.Value.Content) && SaveHandler.LogChannelsSave.Data.ContainsKey(Context.Guild.Id) && SaveHandler.LogChannelsSave.Data[Context.Guild.Id] != Context.Channel.Id)
                {
                    SocketGuildChannel GuildChannel = Context.Guild.GetChannel(SaveHandler.LogChannelsSave.Data[Context.Guild.Id]);
                    if (GuildChannel != null)
                    {
                        ISocketMessageChannel LogChannel = GuildChannel as ISocketMessageChannel;
                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithColor(Color.Blue);
                        builder.WithTitle("Message Deleted");
                        builder.WithDescription($"From {Message.Value.Author.Mention} in <#{Channel.Id}>:\n{Message.Value.Content}");
                        if (builder.Description.Length > EmbedBuilder.MaxDescriptionLength)
                        {
                            string[] Msgs = Misc.ConvertToDiscordSendable(builder.Description, EmbedBuilder.MaxDescriptionLength);
                            for (int i = 0; i < Msgs.Length; i++)
                            {
                                string msg = Msgs[i];
                                builder.WithDescription(msg);
                                await LogChannel.SendMessageAsync(embed: builder.Build());
                                if (i == 0) builder.Title = null;

                            }
                        }
                        else await LogChannel.SendMessageAsync(embed: builder.Build());
                    }
                    else SaveHandler.LogChannelsSave.Data.Remove(Context.Guild.Id);
                }
            }
        }

        private async Task GuildMemberUpdated(SocketGuildUser Before, SocketGuildUser After)
        {
            if (SaveHandler.TrackersSave.Data.Keys.Contains(After.Id) && SaveHandler.TrackersSave.Data[After.Id].IsTrackerEnabled)
            {
                if (Before.Status != After.Status)
                {
                    SaveHandler.TrackersSave.Data[After.Id].dt = DateTime.Now;
                }
            }

            if (SaveHandler.LogChannelsSave.Data.ContainsKey(After.Guild.Id))
            {
                SocketGuildChannel GuildChannel = After.Guild.GetChannel(SaveHandler.LogChannelsSave.Data[After.Guild.Id]);
                if (GuildChannel != null)
                {
                    if (Before.Nickname != After.Nickname)
                    {
                        ISocketMessageChannel LogChannel = GuildChannel as ISocketMessageChannel;
                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithColor(Color.Blue);

                        if (string.IsNullOrWhiteSpace(After.Nickname))
                        {
                            builder.WithTitle("Nickname Removal");
                            builder.WithDescription($"{After.Mention}:\n`{Before.Nickname}` -> `None`");
                        }
                        else if (string.IsNullOrWhiteSpace(Before.Nickname))
                        {
                            builder.WithTitle("Nickname Changed");
                            builder.WithDescription($"{After.Mention}:\n`None` -> `{After.Nickname}`");
                        }
                        else
                        {
                            builder.WithTitle("Nickname Changed");
                            builder.WithDescription($"{After.Mention}:\n`{Before.Nickname}` -> `{After.Nickname}`");
                        }

                        await LogChannel.SendMessageAsync(embed: builder.Build());
                    }
                }
                else SaveHandler.LogChannelsSave.Data.Remove(After.Guild.Id);
            }
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
                if (SaveHandler.FilterSave.Data.ContainsKey(Context.Guild.Id) && SaveHandler.FilterSave.Data[Context.Guild.Id].IsEnabled && Moderation.MessageContainsFilteredWord(Context.Guild.Id, Context.Message.Content))
                {
                    await Context.Channel.TriggerTypingAsync();
                    await Task.Delay(80);
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} Your language is highly uncalled for...");
                    await Context.Message.DeleteAsync();
                    await Context.Channel.TriggerTypingAsync();
                    await Task.Delay(200);
                    await Context.Channel.SendMessageAsync("Thank you in advance for your cooperation.");
                    return;
                }
#if DEBUG
                if (!Message.HasStringPrefix("d.", ref PrefixPos)) return;
#else
                if (!Message.HasStringPrefix("c.", ref PrefixPos)) return;
#endif
            }

            IResult Result = await Commands.ExecuteAsync(Context, PrefixPos, null);
            if (!Result.IsSuccess) await Error.SendDiscordError(Context, Result.ErrorReason);
        }

        private async Task Client_Ready()
        {
            Console.WriteLine("Ready!");
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
