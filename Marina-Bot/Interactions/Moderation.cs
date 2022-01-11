using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Marina.Interactions.Attributes;
using Marina.Save;
using Marina.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marina.Interactions
{
    public class Moderation : InteractionModuleBase<SocketInteractionContext>
    {
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [SlashCommand("ban", "Ban someone lole")]
        public async Task BanUser([RequireHierarchy] SocketGuildUser user, string reason = null, int pruneDays = 0)
        {
            await user.BanAsync(pruneDays, reason);
            await RespondAsync($"{user} is now b& :thumbsup:");
        }

        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [SlashCommand("kick", "kick someone lole")]
        public async Task KickUser([RequireHierarchy] SocketGuildUser user, string reason = null)
        {
            await user.KickAsync(reason);
            await RespondAsync($"kicked {user} :boot:");
        }

        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [SlashCommand("purge", "purge messages from the current channel")]
        public async Task Purge(int count = 50)
        {
            if (count <= 0)
            {
                await Error.SendDiscordError(Context, value: "Invalid parameters");
                return;
            }
            if (count > 200)
            {
                await Error.SendDiscordError(Context, value: "Too many messages to remove!");
                return;
            }

            int rmCnt = 0;
            foreach (IMessage msg in await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync())
            {
                try
                {
                    await msg.DeleteAsync();
                    rmCnt++;
                    await Task.Delay(TimeSpan.FromSeconds(0.06));
                }
                catch
                {
                    continue;
                    // ignored
                }
            }

            if (rmCnt == 0)
            {
                await Error.SendDiscordError(Context, value: "Could not remove any messages");
                return;
            }
            await RespondAsync($"Removed {rmCnt - 1} messages", ephemeral: true);
        }

        
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        [SlashCommand("lock", "Denies the Everyone role from sending messages")]
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

            SocketTextChannel channel = (SocketTextChannel)Context.Channel;
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

            await RespondAsync("Channel is now locked down!");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        [SlashCommand("unlock", "Unlocks a channels that was previously locked .")]
        public async Task Unlock()
        {
            if (!SaveHandler.LockdownSave.ContainsKey(Context.Guild.Id) ||
                !SaveHandler.LockdownSave[Context.Guild.Id].Contains(Context.Channel.Id))
            {
                await ReplyAsync("Channel is not in lockdown!");
                return;
            }

            SocketTextChannel channel = (SocketTextChannel)Context.Channel;
            OverwritePermissions? permissions = channel.GetPermissionOverwrite(Context.Guild.EveryoneRole);

            if (permissions.HasValue && permissions.Value.SendMessages == PermValue.Deny)
            {
                await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                    permissions.Value.Modify(sendMessages: PermValue.Inherit));
            }

            SaveHandler.LockdownSave[Context.Guild.Id].Remove(Context.Channel.Id);

            await RespondAsync("Channel is unlocked!");
        }
    }
}