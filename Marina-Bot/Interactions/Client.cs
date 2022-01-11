using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Marina.Properties;
using Marina.Save;
using Marina.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Console = Marina.Utils.Console;
using RequireOwnerAttribute = Marina.Interactions.Attributes.RequireOwnerAttribute;

namespace Marina.Interactions
{
    [Group("client", "Client specific commands")]
    public class Client : InteractionModuleBase<SocketInteractionContext>
    {
        static Client()
        {
            Program.Initialize += delegate (object? sender, ServiceProvider service)
            {
                var client = service.GetService<DiscordSocketClient>();
                client.Connected += async delegate
                {
                    await UpdatePresence(client);
                };

                client.LeftGuild += arg => UpdatePresence(client);
                client.JoinedGuild += arg => UpdatePresence(client);
                //Clean Up
                client.LeftGuild += async delegate (SocketGuild guild)
                {
                    SaveHandler.LogSave.Remove(guild.Id);
                    foreach (ulong val in SaveHandler.LockdownSave[guild.Id])
                    {
                        SocketTextChannel channel = guild.GetTextChannel(val);
                        OverwritePermissions? permissions = channel.GetPermissionOverwrite(guild.EveryoneRole);

                        if (permissions.HasValue && permissions.Value.SendMessages == PermValue.Deny)
                        {
                            await channel.AddPermissionOverwriteAsync(guild.EveryoneRole,
                                permissions.Value.Modify(sendMessages: PermValue.Inherit));
                        }
                    }
                };
            };
        }

        private static async Task UpdatePresence(DiscordSocketClient client)
        {
            if (string.IsNullOrEmpty(SaveHandler.Config.Data.TwitchName))
            {
                if (client.Guilds.Count > 1)
                    await client.SetGameAsync($"{client.Guilds.Count} servers | m.help", type: ActivityType.Watching);
                else
                    await client.SetGameAsync($"{client.Guilds.Count} server | m.help", type: ActivityType.Watching);
            }
            else
            {
                if (client.Guilds.Count > 1)
                    await client.SetGameAsync($"on {client.Guilds.Count} servers | m.help", $"https://twitch.tv/{SaveHandler.Config.Data.TwitchName}");
                else
                    await client.SetGameAsync($"on {client.Guilds.Count} server | m.help", $"https://twitch.tv/{SaveHandler.Config.Data.TwitchName}");
            }
        }

        [RequireOwner]
        [SlashCommand("banuser", "Ban a user from using some of the commands!")]
        public async Task AddUserToBlacklist(IUser user)
        {
            if (!SaveHandler.BlacklistSave.Contains(user.Id))
            {
                SaveHandler.BlacklistSave.Add(user.Id);
                await RespondAsync("User added to blacklist!", ephemeral: true);
            }
            else
            {
                await RespondAsync("User already in blacklist!", ephemeral: true);
            }
        }

        [RequireOwner]
        [SlashCommand("unbanuser", "Unbans a user from using commands!")]
        public async Task RemoveUserFromBlacklist(IUser user)
        {
            if (SaveHandler.BlacklistSave.Remove(user.Id))
                await RespondAsync("User removed from blacklist", ephemeral: true);
            else
                await RespondAsync("User not added to blacklist!", ephemeral: true);
        }

        [RequireOwner]
        [SlashCommand("announce", "Send server owners important messages!!")]
        public async Task Announce(string announcement)
        {
            foreach (SocketGuild guild in Context.Client.Guilds)
                try
                {
                    await guild.Owner.SendMessageAsync(announcement);
                }
                catch
                {
                    // ignored
                }

            await RespondAsync("Announcement sent to all Guild Owners! :ok_hand:", ephemeral: true);
        }

        [RequireOwner]
        [SlashCommand("stop", "Shut down Marina remotely!")]
        public async Task Shutdown()
        {
            await RespondAsync("Shutting down!", ephemeral: true);
            await Context.Client.StopAsync();
            SaveHandler.SaveAll(false);
            await Console.WriteLog("***********************Shutdown via Command!***********************");
            Environment.Exit(0);
        }

        [RequireOwner]
        [SlashCommand("say", "Say something lole")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task ForceSay(string str)
        {
            await ReplyAsync(str);
            await RespondAsync(":thumbsup:", ephemeral: true);
        }

        [RequireOwner]
        [SlashCommand("csay", "Say something in a specific channel!")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task ChannelForceSay(SocketTextChannel channel, string str)
        {
            await channel.SendMessageAsync(str);
            await RespondAsync(":thumbsup:", ephemeral: true);
        }

        [RequireOwner]
        [SlashCommand("servers", "See what servers the bot is in!")]
        public async Task GetServers()
        {
            EmbedBuilder builder = new();
            builder.WithTitle("Server List");
            builder.WithColor(Color.Teal);

            switch (Context.Client.Guilds.Count)
            {
                case 1:
                    builder.Description += "I am currently only in this server.";
                    break;
                default:
                    builder.Description += $"```{Context.Client.Guilds.Count} total servers:\n";
                    foreach (SocketGuild guild in Context.Client.Guilds)
                        builder.Description += $"{guild.Name}\n";

                    builder.Description += "```";
                    break;
            }

            if (builder.Length > EmbedBuilder.MaxDescriptionLength)
            {
                string[] msgs = Misc.ConvertToDiscordSendable(builder.Description);
                for (int i = 0; i < msgs.Length; i++)
                {
                    builder.Description = msgs[i];
                    if (msgs.Length - 1 == i)
                        builder.WithCurrentTimestamp();

                    await RespondAsync(embed: builder.Build());
                    if (i == 0)
                        builder.Title = null;
                }
            }
            else
            {
                builder.WithCurrentTimestamp();
                await RespondAsync(embed: builder.Build(), ephemeral: true);
            }
        }

        [RequireOwner]
        [SlashCommand("save", "flush all save data to files!")]
        public async Task ForceSave()
        {
            SaveHandler.SaveAll();
            await RespondAsync("Saved all data!", ephemeral: true);
        }

        [RequireOwner]
        [SlashCommand("version", "Revision Number")]
        public async Task GetVersion() =>
            await RespondAsync($"Git Commit: {Encoding.UTF8.GetString(Resources.CurrentCommit)}", ephemeral: true);

        [RequireOwner]
        [SlashCommand("info", "Bot information!", true)]
        public async Task GetInfo() => await RespondAsync("You can find my voting, source, and invite links here: https://top.gg/bot/580901187931603004");

        [RequireOwner]
        [SlashCommand("setpmode", "set the bot's priority!")]
        public async Task SetPriorityMode(ProcessPriorityClass mode)
        {
            using Process p = Process.GetCurrentProcess();
            p.PriorityClass = mode;
            await RespondAsync($"Set priority to `{p.PriorityClass}`", ephemeral: true);
        }

        [RequireOwner]
        [SlashCommand("pmode", "Get the priority that has been set!")]
        public async Task GetPriorityMode()
        {
            using Process p = Process.GetCurrentProcess();
            await RespondAsync($"Priority is set to `{p.PriorityClass}`", ephemeral: true);
        }
    }
}