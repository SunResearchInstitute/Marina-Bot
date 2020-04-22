using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Save;
using Marina.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable PossibleInvalidOperationException

namespace Marina.Commands
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [Command("Ban")]
        public async Task BanUser([Name("User")] [RequireHierarchy] SocketGuildUser user,
            [Name("Reason")] params string[] reason)
        {
            string reasonJoined = (reason != null ? string.Join(' ', reason) : null)!;
            string msg = $"You were banned from {Context.Guild.Name}";
            if (reasonJoined != null) msg += $"\nReason: {reasonJoined}";
            await user.SendMessageAsync(msg);
            await user.BanAsync(reason: reasonJoined);
            await ReplyAsync($"{user} is now b& :thumbsup:");
            //Bans will automatically logged
        }

        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [Command("Kick")]
        public async Task KickUser([Name("User")] [RequireHierarchy] SocketGuildUser user,
            [Name("Reason")] params string[] reason)
        {
            string joined = (reason != null ? string.Join(' ', reason) : null)!;
            string msg = $"You were kicked from {Context.Guild.Name}";
            if (joined != null) msg += $"\nReason: {joined}";
            await user.SendMessageAsync(msg);
            await user.KickAsync(joined);
            await ReplyAsync($"kicked {user} :boot:");

            //Kicks need to be manually logged
            if (SaveHandler.LogSave.ContainsKey(user.Guild.Id))
            {
                SocketTextChannel logChannel = user.Guild.GetTextChannel(SaveHandler.LogSave[user.Guild.Id]);
                EmbedBuilder builder = new EmbedBuilder
                {
                    Color = Color.Teal,
                    Title = "**Kicked**",
                    Description = $"{Context.User.Mention} kicked {user.Mention} | {user}"
                };
                if (joined != null) builder.Description += $"\n__Reason__: \"{joined}\"";
                await logChannel.SendMessageAsync(embed: builder.Build());
            }
        }

        [Command("Purge")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task Purge([Name("Count")] int count = 50)
        {
            if (count <= 0)
            {
                await Error.SendDiscordError(Context, value: "Invalid parameters");
                return;
            }

            int rmCnt = 0;
            foreach (IMessage msg in await Context.Channel.GetMessagesAsync(count).FlattenAsync())
                try
                {
                    await msg.DeleteAsync();
                    rmCnt++;
                }
                catch
                {
                    // ignored
                }

            IUserMessage resultMsg = await ReplyAsync($"Removed {rmCnt - 1} messages");
            await Task.Delay(TimeSpan.FromSeconds(3));
            try
            {
                await resultMsg.DeleteAsync();
            }
            catch
            {
                // ignored
            }
        }

        [Command("Lock")]
        [Summary("Denies the Everyone role from sending messages")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(ChannelPermission.ManageChannels)]
        public async Task Lockdown()
        {
            if (!SaveHandler.LockdownSave.ContainsKey(Context.Guild.Id))
            {
                SaveHandler.LockdownSave.Add(Context.Guild.Id, new List<ulong>());
            }
            else if (SaveHandler.LockdownSave[Context.Guild.Id].Contains(Context.Channel.Id))
            {
                await Error.SendDiscordError(Context, value: "Channel is already in lockdown!");
                return;
            }

            SocketTextChannel channel = (SocketTextChannel) Context.Channel;
            OverwritePermissions? permissions = channel.GetPermissionOverwrite(Context.Guild.EveryoneRole);
            if (permissions.HasValue)
            {
                if (permissions.Value.SendMessages == PermValue.Deny)
                {
                    await Error.SendDiscordError(Context,
                        value: "Send Messages permission for everyone is already denied!");
                    return;
                }

                await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                    permissions.Value.Modify(sendMessages: PermValue.Deny));
            }
            else
            {
                await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                    new OverwritePermissions(sendMessages: PermValue.Deny));
            }

            SaveHandler.LockdownSave[Context.Guild.Id].Add(Context.Channel.Id);

            await ReplyAsync("Channel is now locked down!");
        }

        [Command("Unlock")]
        [Summary("Unlocks a channels that was previously locked in which did not allow for the Everyone role to send messages in.")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(ChannelPermission.ManageChannels)]
        public async Task Unlock()
        {
            if (!SaveHandler.LockdownSave.ContainsKey(Context.Guild.Id) ||
                !SaveHandler.LockdownSave[Context.Guild.Id].Contains(Context.Channel.Id))
            {
                await ReplyAsync("Channel is not in lockdown!");
                return;
            }

            SocketTextChannel channel = (SocketTextChannel) Context.Channel;
            OverwritePermissions? permissions = channel.GetPermissionOverwrite(Context.Guild.EveryoneRole);

            if (permissions.HasValue && permissions.Value.SendMessages == PermValue.Deny)
            {
                await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                    permissions.Value.Modify(sendMessages: PermValue.Inherit));
            }

            SaveHandler.LockdownSave[Context.Guild.Id].Remove(Context.Channel.Id);

            await ReplyAsync("Channel is unlocked!");
        }
    }
}