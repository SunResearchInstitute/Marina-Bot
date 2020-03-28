using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using LibSave;
using Marina.Commands;
using Marina.Save;
using Marina.Utils;
using Console = Marina.Utils.Console;

namespace Marina
{
    internal class Program
    {
        //API Stuff
        private static DiscordSocketClient _client;
        public static CommandService Commands { get; private set; }

        private static void Main()
        {
            Program program = new Program();
            program.MainAsync().GetAwaiter().GetResult();
            Thread.Sleep(-1);
        }

        private async Task MainAsync()
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

            _client.MessageReceived += MessageReceived;
            _client.MessageDeleted += MessageDeleted;
            _client.UserLeft += UserLeft;
            _client.UserBanned += UserBanned;
            _client.MessageUpdated += MessageUpdated;
            _client.GuildMemberUpdated += GuildMemberUpdated;
            _client.Ready += ClientReady;
            _client.LeftGuild += Client_LeftGuild;
            _client.ChannelDestroyed += Client_ChannelDestroyed;

            _client.Log += Log;
            Commands.Log += Log;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            //a Static Method Starts too early
            Help.Populate();
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

        private Task Client_ChannelDestroyed(SocketChannel channel)
        {
            //Only currently needed for Logs at the moment
            //Will try to remove the pair if it exists in the list
            SaveHandler.LogSave.Remove(new KeyValuePair<ulong, ulong>(((SocketGuildChannel) channel).Guild.Id,
                channel.Id));
            return Task.CompletedTask;
        }

        private static Task Client_LeftGuild(SocketGuild guild)
        {
            //Removes guild Marina is no longer in
            foreach (ISaveFile save in SaveHandler.Saves.Values) save.CleanUp(guild.Id);
            return Task.CompletedTask;
        }

        private static async Task ClientReady()
        {
            await Console.WriteLog("Initialized!");
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (_client.Guilds.Count > 1)
                        await _client.SetGameAsync($"on {_client.Guilds.Count} servers | m.help");
                    else await _client.SetGameAsync($"on {_client.Guilds.Count} server | m.help");
                    await Task.Delay(TimeSpan.FromHours(1));
                }
            });
        }

        private static async Task UserBanned(SocketUser user, SocketGuild guild)
        {
            if (SaveHandler.LogSave.ContainsKey(guild.Id))
            {
                SocketTextChannel logChannel = guild.GetTextChannel(SaveHandler.LogSave[guild.Id]);
                RestAuditLogEntry lastBan =
                    (await guild.GetAuditLogsAsync(3).FlattenAsync()).First(l => l.Action == ActionType.Ban);
                EmbedBuilder builder = new EmbedBuilder
                {
                    Color = Color.Teal,
                    Title = "**Banned**",
                    Description = $"{lastBan.User.Mention} banned {user.Mention} | {user}"
                };
                builder.WithCurrentTimestamp();
                if (!string.IsNullOrWhiteSpace(lastBan.Reason))
                    builder.Description += $"\n__Reason__: \"{lastBan.Reason}\"";
                await logChannel.SendMessageAsync(embed: builder.Build());
            }
        }

        private static async Task UserLeft(SocketGuildUser user)
        {
            if (SaveHandler.LogSave.ContainsKey(user.Guild.Id))
            {
                SocketTextChannel logChannel = user.Guild.GetTextChannel(SaveHandler.LogSave[user.Guild.Id]);
                EmbedBuilder builder = new EmbedBuilder
                {
                    Color = Color.Teal,
                    Title = "User Left",
                    Description = $"{user.Mention} | {user.Username}"
                };
                builder.WithCurrentTimestamp();
                await logChannel.SendMessageAsync(embed: builder.Build());
            }
        }

        private static async Task MessageUpdated(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage,
            ISocketMessageChannel channel)
        {
            if (!oldMessage.HasValue || newMessage.Author.IsBot)
                return;

            SocketGuild guild = ((SocketTextChannel) channel).Guild;
            if (SaveHandler.LogSave.ContainsKey(guild.Id))
            {
                SocketTextChannel logChannel = guild.GetTextChannel(SaveHandler.LogSave[guild.Id]);
                if (oldMessage.Value.Content != newMessage.Content)
                {
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Color = Color.Teal,
                        Title = "Message Edited",
                        Description =
                            $"From {newMessage.Author.Mention} in <#{channel.Id}>:\n**Before:**\n{oldMessage.Value.Content}\n**After:**\n{newMessage.Content}"
                    };

                    if (builder.Description.Length > EmbedBuilder.MaxDescriptionLength)
                    {
                        string[] msgs = Misc.ConvertToDiscordSendable(builder.Description);
                        for (int i = 0; i < msgs.Length; i++)
                        {
                            string msg = msgs[i];
                            builder.Description = msg;
                            if (msgs.Length - 1 == i)
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
        }

        private static async Task MessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (!message.HasValue || message.Value.Author.IsBot)
                return;

            SocketGuild guild = ((SocketGuildChannel) channel).Guild;
            if (SaveHandler.LogSave.ContainsKey(guild.Id))
            {
                SocketTextChannel logChannel = guild.GetTextChannel(SaveHandler.LogSave[guild.Id]);
                if (logChannel.Id != channel.Id && !string.IsNullOrWhiteSpace(message.Value.Content))
                {
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Color = Color.Teal,
                        Title = "Message Deleted",
                        Description =
                            $"From {message.Value.Author.Mention} in <#{channel.Id}>:\n{message.Value.Content}"
                    };

                    if (builder.Description.Length > EmbedBuilder.MaxDescriptionLength)
                    {
                        string[] msgs = Misc.ConvertToDiscordSendable(builder.Description);
                        for (int i = 0; i < msgs.Length; i++)
                        {
                            string msg = msgs[i];
                            builder.Description = msg;
                            if (msgs.Length - 1 == i)
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
        }

        private static async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            if (before.IsBot) return;

            if (SaveHandler.LogSave.ContainsKey(after.Guild.Id))
            {
                SocketGuildChannel guildChannel = after.Guild.GetChannel(SaveHandler.LogSave[after.Guild.Id]);
                if (guildChannel != null)
                {
                    if (before.Nickname != after.Nickname)
                    {
                        ISocketMessageChannel logChannel = guildChannel as ISocketMessageChannel;
                        EmbedBuilder builder = new EmbedBuilder
                        {
                            Color = Color.Teal
                        };
                        builder.WithCurrentTimestamp();
                        if (string.IsNullOrWhiteSpace(after.Nickname))
                        {
                            builder.Title = "Nickname Removal";
                            builder.Description = $"{after.Mention}:\n`{before.Nickname}` -> `None`";
                        }
                        else if (string.IsNullOrWhiteSpace(before.Nickname))
                        {
                            builder.Title = "Nickname Changed";
                            builder.Description = $"{after.Mention}:\n`None` -> `{after.Nickname}`";
                        }
                        else
                        {
                            builder.Title = "Nickname Changed";
                            builder.Description = $"{after.Mention}:\n`{before.Nickname}` -> `{after.Nickname}`";
                        }

                        await logChannel.SendMessageAsync(embed: builder.Build());
                    }
                }
                else
                {
                    SaveHandler.LogSave.Remove(after.Guild.Id);
                }
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

        private static async Task Log(LogMessage log) => await Console.WriteLog($"[{DateTime.Now}]: {log.ToString()}\n");
    }
}