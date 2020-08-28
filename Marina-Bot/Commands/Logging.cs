using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Marina.Attributes;
using Marina.Save;
using Marina.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Logging : ModuleBase<SocketCommandContext>
    {
        static Logging()
        {
            Program.Initialize += delegate (object? sender, DiscordSocketClient client)
            {
                client.ChannelDestroyed += delegate (SocketChannel channel)
                {
                    //Only currently needed for Logs at the moment
                    //Will try to remove the pair if it exists in the list
                    SaveHandler.LogSave.Remove(new KeyValuePair<ulong, ulong>(((SocketGuildChannel)channel).Guild.Id,
                        channel.Id));
                    SaveHandler.LockdownSave[((SocketGuildChannel)channel).Guild.Id]
                        .Remove(((SocketGuildChannel)channel).Id);
                    return Task.CompletedTask;
                };

                client.MessageUpdated += async delegate (Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage,
                    ISocketMessageChannel channel)
                {
                    if (!oldMessage.HasValue || newMessage.Author.IsBot)
                        return;

                    SocketGuild guild = ((SocketTextChannel)channel).Guild;
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

                            if (builder.Length > EmbedBuilder.MaxDescriptionLength)
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
                };

                client.UserLeft += async delegate (SocketGuildUser user)
                {
                    if (SaveHandler.LogSave.ContainsKey(user.Guild.Id))
                    {
                        SocketTextChannel logChannel = user.Guild.GetTextChannel(SaveHandler.LogSave[user.Guild.Id]);
                        RestAuditLogEntry lastKick = (await user.Guild.GetAuditLogsAsync(3, actionType: ActionType.Kick).FlattenAsync()).FirstOrDefault(l => (l.Data as KickAuditLogData).Target == user);
                        EmbedBuilder builder = new EmbedBuilder
                        {
                            Color = Color.Teal
                        };

                        if (lastKick != null)
                        {
                            string msg = $"You were kicked from {user.Guild.Name}";
                            if (lastKick.Reason != null) msg += $"\nReason: {lastKick.Reason}";
                            try
                            {
                                await user.SendMessageAsync(msg);
                            }
                            catch { }
                            builder.WithCurrentTimestamp();
                            builder.Title = "User Kicked";
                            builder.Description = $"{lastKick.User.Mention} kicked {user.Mention} | {user}";
                            if (lastKick.Reason != null) builder.Description += $"\n__Reason__: \"{lastKick.Reason}\"";
                            await logChannel.SendMessageAsync(embed: builder.Build());
                        }

                        builder.WithCurrentTimestamp();
                        builder.Title = "User Left";
                        builder.Description = $"{user.Mention} | {user}";
                        await logChannel.SendMessageAsync(embed: builder.Build());
                    }
                };
                //TODO: reimpl
                /*
                client.LeftGuild += delegate (SocketGuild guild)
                {
                    //Removes guild Marina is no longer in
                    foreach (ISaveFile save in Program.SaveController.SaveFiles.Values)
                        save.CleanUp(guild.Id);

                    return Task.CompletedTask;
                };
                */

                client.UserBanned += async delegate (SocketUser user, SocketGuild guild)
                {
                    if (SaveHandler.LogSave.ContainsKey(guild.Id))
                    {
                        RestAuditLogEntry lastBan =
                            (await guild.GetAuditLogsAsync(3, actionType: ActionType.Ban).FlattenAsync()).FirstOrDefault(l => (l.Data as BanAuditLogData).Target == user);
                        if (lastBan != null)
                        {
                            string msg = $"You were banned from {guild.Name}";
                            if (lastBan.Reason != null) msg += $"\nReason: {lastBan.Reason}";
                            try
                            {
                                await user.SendMessageAsync(msg);
                            }
                            catch { }

                            SocketTextChannel logChannel = guild.GetTextChannel(SaveHandler.LogSave[guild.Id]);
                            EmbedBuilder builder = new EmbedBuilder
                            {
                                Color = Color.Teal,
                                Title = "User Banned",
                                Description = $"{lastBan.User.Mention} banned {user.Mention} | {user}"
                            };
                            builder.WithCurrentTimestamp();
                            if (!string.IsNullOrWhiteSpace(lastBan.Reason))
                                builder.Description += $"\n__Reason__: \"{lastBan.Reason}\"";
                            await logChannel.SendMessageAsync(embed: builder.Build());
                        }
                    }
                };
                client.MessageDeleted +=
                    async delegate (Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
                    {
                        if (!message.HasValue || message.Value.Author.IsBot)
                            return;

                        SocketGuild guild = ((SocketGuildChannel)channel).Guild;
                        if (SaveHandler.LogSave.ContainsKey(guild.Id))
                        {
                            SocketTextChannel logChannel = guild.GetTextChannel(SaveHandler.LogSave[guild.Id]);
                            if (logChannel.Id != channel.Id &&
                                (!string.IsNullOrWhiteSpace(message.Value.Content) || message.Value.Attachments.Any()))
                            {
                                EmbedBuilder builder = new EmbedBuilder
                                {
                                    Description = "",
                                    Color = Color.Teal,
                                    Title = "Message Deleted",
                                };
                                RestAuditLogEntry messageDeleted = (await guild.GetAuditLogsAsync(3, actionType: ActionType.MessageDeleted).FlattenAsync()).FirstOrDefault(l => (l.Data as MessageDeleteAuditLogData).AuthorId == message.Value.Author.Id);

                                if (!string.IsNullOrWhiteSpace(message.Value.Content))
                                    builder.Description += $"From {message.Value.Author.Mention}, in <#{channel.Id}>";

                                if (messageDeleted != null)
                                    builder.Description += $", deleted by {messageDeleted.User.Mention}";

                                builder.Description += $":\n{message.Value.Content}";

                                if (message.Value.Attachments.Any())
                                {
                                    builder.Description += "\n\nAttachments:\n";
                                    foreach (IAttachment attachment in message.Value.Attachments)
                                        builder.Description += $"{attachment.Url}\n";
                                }

                                if (builder.Length > EmbedBuilder.MaxDescriptionLength)
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
                    };

                client.GuildMemberUpdated += async delegate (SocketGuildUser before, SocketGuildUser after)
                {
                    if (before.IsBot) return;

                    if (SaveHandler.LogSave.ContainsKey(after.Guild.Id))
                    {
                        SocketTextChannel logChannel = after.Guild.GetTextChannel(SaveHandler.LogSave[after.Guild.Id]);
                        if (logChannel != null)
                        {
                            if (before.Nickname != after.Nickname)
                            {
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
                                    builder.Description =
                                        $"{after.Mention}:\n`{before.Nickname}` -> `{after.Nickname}`";
                                }

                                await logChannel.SendMessageAsync(embed: builder.Build());
                            }
                        }
                    }
                };
            };
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("SetLogs")]
        [Summary("Sets the logging channel.")]
        public async Task SetLogs([Name("Channel")] [ManualOptionalParameter("Current Channel")]
            SocketTextChannel channel = null)
        {
            channel ??= (SocketTextChannel)Context.Channel;

            if (SaveHandler.LogSave.ContainsKey(Context.Guild.Id))
                SaveHandler.LogSave[Context.Guild.Id] = channel.Id;

            else
                SaveHandler.LogSave.Add(Context.Guild.Id, channel.Id);

            await ReplyAsync($"Logs will now be put in {channel.Name}");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("RemoveLogs")]
        [Summary("Removes the logging channel.")]
        public async Task RemoveLogs()
        {
            if (SaveHandler.LogSave.Remove(Context.Guild.Id))
                await ReplyAsync("Logs removed.");
            else
                await ReplyAsync("No logs to remove.");
        }
    }
}