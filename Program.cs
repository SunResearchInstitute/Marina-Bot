using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Marina.Commands;
using Marina.Save;
using Marina.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static readonly FileInfo ConfigFile = new FileInfo("Config.txt");
        public static readonly FileInfo LogFile = new FileInfo("Marina.log");

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
                MessageCacheSize = 60,
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
                Error.SendApplicationError($"Token is invalid! check you config at {ConfigFile.FullName}", 1);
            }
            //a Static Method Starts too early
            Help.Populate();
            await Client.StartAsync();
        }

        private Task Log(LogMessage log)
        {
            LogFile.AppendAllText($"[{DateTime.Now}]: {log.Source} {log.Message}: {log.Exception.StackTrace}\n");

            return Task.CompletedTask;
        }

        private async Task UserBanned(SocketUser User, SocketGuild Guild)
        {
            if (SaveHandler.LogSave.Data.ContainsKey(Guild.Id))
            {
                SocketTextChannel logChannel = Guild.GetTextChannel(SaveHandler.LogSave.Data[Guild.Id]);
                if (logChannel != null)
                {
                    RestAuditLogEntry lastBan = (await Guild.GetAuditLogsAsync(5).FlattenAsync()).First(l => l.Action == ActionType.Ban);
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Color = Color.Teal,
                        Title = "**Banned**",
                        Description = $"{lastBan.User.Mention} banned {User.Mention} | {User}",
                    };
                    builder.WithCurrentTimestamp();
                    if (!string.IsNullOrWhiteSpace(lastBan.Reason)) builder.Description += $"\n__Reason__: \"{lastBan.Reason}\"";
                    await logChannel.SendMessageAsync(embed: builder.Build());
                }
                else SaveHandler.LogSave.Data.Remove(Guild.Id);
            }
        }

        private async Task UserLeft(SocketGuildUser User)
        {
            if (SaveHandler.LogSave.Data.ContainsKey(User.Guild.Id))
            {
                SocketTextChannel logChannel = User.Guild.GetTextChannel(SaveHandler.LogSave.Data[User.Guild.Id]);
                if (logChannel != null)
                {
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Color = Color.Teal,
                        Title = "User Left",
                        Description = $"{User.Mention} | {User.Username}"
                    };
                    builder.WithCurrentTimestamp();
                    await logChannel.SendMessageAsync(embed: builder.Build());
                }
                else SaveHandler.LogSave.Data.Remove(User.Guild.Id);
            }
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> OldMessage, SocketMessage NewMessage, ISocketMessageChannel Channel)
        {
            if (!OldMessage.HasValue || NewMessage.Author.IsBot)
                return;

            SocketGuild guild = (Channel as SocketTextChannel).Guild;
            if (SaveHandler.LogSave.Data.ContainsKey(guild.Id))
            {
                SocketTextChannel LogChannel = guild.GetTextChannel(SaveHandler.LogSave.Data[guild.Id]);
                if (LogChannel != null)
                {
                    if (OldMessage.Value.Content != NewMessage.Content)
                    {
                        EmbedBuilder builder = new EmbedBuilder
                        {
                            Color = Color.Teal,
                            Title = "Message Edited",
                            Description = $"From {NewMessage.Author.Mention} in <#{Channel.Id}>:\n**Before:**\n{OldMessage.Value.Content}\n**After:**\n{NewMessage.Content}"
                        };

                        if (builder.Description.Length > EmbedBuilder.MaxDescriptionLength)
                        {
                            string[] Msgs = Misc.ConvertToDiscordSendable(builder.Description, EmbedBuilder.MaxDescriptionLength);
                            for (int i = 0; i < Msgs.Length; i++)
                            {
                                string msg = Msgs[i];
                                builder.Description = msg;
                                if (Msgs.Length - 1 == i)
                                    builder.WithCurrentTimestamp();

                                await LogChannel.SendMessageAsync(embed: builder.Build());
                                if (i == 0)
                                    builder.Title = null;
                            }
                        }
                        else
                        {
                            builder.WithCurrentTimestamp();
                            await LogChannel.SendMessageAsync(embed: builder.Build());
                        }
                    }
                }
                else
                    SaveHandler.LogSave.Data.Remove(guild.Id);
            }
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> Message, ISocketMessageChannel Channel)
        {
            if (!Message.HasValue || Message.Value.Author.IsBot)
                return;

            SocketGuild guild = (Channel as SocketGuildChannel).Guild;
            if (SaveHandler.LogSave.Data.ContainsKey(guild.Id))
            {
                SocketTextChannel logChannel = guild.GetTextChannel(SaveHandler.LogSave.Data[guild.Id]);
                if (logChannel != null)
                {
                    if (logChannel.Id != Channel.Id && !string.IsNullOrWhiteSpace(Message.Value.Content))
                    {
                        EmbedBuilder builder = new EmbedBuilder
                        {
                            Color = Color.Teal,
                            Title = "Message Deleted",
                            Description = $"From {Message.Value.Author.Mention} in <#{Channel.Id}>:\n{Message.Value.Content}"
                        };

                        if (builder.Description.Length > EmbedBuilder.MaxDescriptionLength)
                        {
                            string[] Msgs = Misc.ConvertToDiscordSendable(builder.Description, EmbedBuilder.MaxDescriptionLength);
                            for (int i = 0; i < Msgs.Length; i++)
                            {
                                string msg = Msgs[i];
                                builder.Description = msg;
                                if (Msgs.Length - 1 == i)
                                    builder.WithCurrentTimestamp();

                                await logChannel.SendMessageAsync(embed: builder.Build());
                                if (i == 0)
                                    builder.Title = null;
                            }
                        }
                        else
                        {
                            builder.WithCurrentTimestamp();
                            await logChannel.SendMessageAsync(embed: builder.Build());
                        }
                    }
                }
                else
                    SaveHandler.LogSave.Data.Remove(Channel.Id);
            }
        }

        private async Task GuildMemberUpdated(SocketGuildUser Before, SocketGuildUser After)
        {
            if (Before.IsBot) return;

            if (SaveHandler.LogSave.Data.ContainsKey(After.Guild.Id))
            {
                SocketGuildChannel GuildChannel = After.Guild.GetChannel(SaveHandler.LogSave.Data[After.Guild.Id]);
                if (GuildChannel != null)
                {
                    if (Before.Nickname != After.Nickname)
                    {
                        ISocketMessageChannel LogChannel = GuildChannel as ISocketMessageChannel;
                        EmbedBuilder builder = new EmbedBuilder
                        {
                            Color = Color.Teal
                        };
                        builder.WithCurrentTimestamp();
                        if (string.IsNullOrWhiteSpace(After.Nickname))
                        {
                            builder.Title = "Nickname Removal";
                            builder.Description = $"{After.Mention}:\n`{Before.Nickname}` -> `None`";
                        }
                        else if (string.IsNullOrWhiteSpace(Before.Nickname))
                        {
                            builder.Title = "Nickname Changed";
                            builder.Description = $"{After.Mention}:\n`None` -> `{After.Nickname}`";
                        }
                        else
                        {
                            builder.Title = "Nickname Changed";
                            builder.Description = $"{After.Mention}:\n`{Before.Nickname}` -> `{After.Nickname}`";
                        }
                        await LogChannel.SendMessageAsync(embed: builder.Build());
                    }
                }
                else SaveHandler.LogSave.Data.Remove(After.Guild.Id);
            }
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            //Welcomes are considered a message and are null
            if (!(arg is SocketUserMessage Message))
                return;

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
                if (Client.Guilds.Count > 1) await Client.SetGameAsync($"on {Client.Guilds.Count} servers | m.help");
                else await Client.SetGameAsync($"on {Client.Guilds.Count} server | m.help");
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
                File.WriteAllLines(ConfigFile.FullName, new string[]
                {
                    "Token={token}"
                });
                Error.SendApplicationError($"Config does not exist, it has been created for you at {ConfigFile.FullName}!", 1);
            }
        }
    }
}
