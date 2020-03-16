using Discord;
using Discord.Commands;
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
using Console = Marina.Utils.Console;

namespace Marina
{
    class Program
    {
        //API Stuff
        private static DiscordSocketClient _client;
        public static CommandService Commands { get; private set; }

        //Config Stuff
        private static readonly FileInfo _configFile = new FileInfo("Config.txt");
        public static readonly FileInfo LogFile = new FileInfo("Marina.log");
        private static readonly Dictionary<string, string> Config = Misc.LoadConfig(_configFile);

        static void Main()
        {
            Program program = new Program();
            program.MainAsync().GetAwaiter().GetResult();
            Thread.Sleep(-1);
        }

        private async Task MainAsync()
        {
            //This just configures how we want to handle our client and commands
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                //Chaching for Moderation
                MessageCacheSize = 250,
                LogLevel = LogSeverity.Error
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Error
            });

            _client.MessageReceived += MessageReceived;
            _client.MessageDeleted += MessageDeleted;
            _client.UserLeft += UserLeft;
            _client.UserBanned += UserBanned;
            _client.MessageUpdated += MessageUpdated;
            _client.GuildMemberUpdated += GuildMemberUpdated;

            _client.Log += Log;
            Commands.Log += Log;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            //a Static Method Starts too early
            Help.Populate();
            try
            {
                await _client.LoginAsync(TokenType.Bot, Config["token"]);
                await _client.StartAsync();
            }
            catch
            {
                Error.SendApplicationError("Something went wrong, check if your Token is valid!", 1);
            }
            
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (_client.Guilds.Count > 1) await _client.SetGameAsync($"on {_client.Guilds.Count} servers | m.help");
                    else await _client.SetGameAsync($"on {_client.Guilds.Count} server | m.help");
                    await Task.Delay(TimeSpan.FromHours(1));
                }
            });
        }

        private async Task UserBanned(SocketUser User, SocketGuild Guild)
        {
            if (SaveHandler.LogSave.ContainsKey(Guild.Id))
            {
                SocketTextChannel logChannel = Guild.GetTextChannel(SaveHandler.LogSave[Guild.Id]);
                if (logChannel != null)
                {
                    RestAuditLogEntry lastBan = (await Guild.GetAuditLogsAsync(3).FlattenAsync()).First(l => l.Action == ActionType.Ban);
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
                else SaveHandler.LogSave.Remove(Guild.Id);
            }
        }

        private async Task UserLeft(SocketGuildUser User)
        {
            if (SaveHandler.LogSave.ContainsKey(User.Guild.Id))
            {
                SocketTextChannel logChannel = User.Guild.GetTextChannel(SaveHandler.LogSave[User.Guild.Id]);
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
                else SaveHandler.LogSave.Remove(User.Guild.Id);
            }
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> OldMessage, SocketMessage NewMessage, ISocketMessageChannel Channel)
        {
            if (!OldMessage.HasValue || NewMessage.Author.IsBot)
                return;

            SocketGuild guild = (Channel as SocketTextChannel).Guild;
            if (SaveHandler.LogSave.ContainsKey(guild.Id))
            {
                SocketTextChannel LogChannel = guild.GetTextChannel(SaveHandler.LogSave[guild.Id]);
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
                    SaveHandler.LogSave.Remove(guild.Id);
            }
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> Message, ISocketMessageChannel Channel)
        {
            if (!Message.HasValue || Message.Value.Author.IsBot)
                return;

            SocketGuild guild = (Channel as SocketGuildChannel).Guild;
            if (SaveHandler.LogSave.ContainsKey(guild.Id))
            {
                SocketTextChannel logChannel = guild.GetTextChannel(SaveHandler.LogSave[guild.Id]);
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
                    SaveHandler.LogSave.Remove(Channel.Id);
            }
        }

        private async Task GuildMemberUpdated(SocketGuildUser Before, SocketGuildUser After)
        {
            if (Before.IsBot) return;

            if (SaveHandler.LogSave.ContainsKey(After.Guild.Id))
            {
                SocketGuildChannel GuildChannel = After.Guild.GetChannel(SaveHandler.LogSave[After.Guild.Id]);
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
                else SaveHandler.LogSave.Remove(After.Guild.Id);
            }
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            //Welcomes are considered a message and are null
            if (!(arg is SocketUserMessage Message))
                return;

            SocketCommandContext Context = new SocketCommandContext(_client, Message);
            int PrefixPos = 0;

            if (string.IsNullOrWhiteSpace(Context.Message.Content) || Context.User.IsBot) return;

            if (Context.Guild != null)
            {
#if DEBUG
                if (!Message.HasStringPrefix("d.", ref PrefixPos))
#else
                if (!Message.HasStringPrefix("m.", ref PrefixPos))
#endif
                    return;
            }


            await Context.Channel.TriggerTypingAsync();
            IResult Result = await Commands.ExecuteAsync(Context, PrefixPos, null);
            if (!Result.IsSuccess)
                await Error.SendDiscordError(Context, Result.ErrorReason);
        }

        private async Task Log(LogMessage log) => await Console.ConsoleWriteLog($"[{DateTime.Now}]: {log.ToString()}\n");
    }
}
