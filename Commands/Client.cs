using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Save;
using Marina.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Client : ModuleBase<SocketCommandContext>
    {
        [Command("BanUser")]
        [RequireOwner]
        public async Task AddUserToBlacklist(IUser user)
        {
            await Context.Channel.TriggerTypingAsync();
            if (!SaveHandler.BlacklistSave.Data.Contains(user.Id))
            {
                SaveHandler.BlacklistSave.Data.Add(user.Id);
                await ReplyAsync("User added to blacklist!");
            }
            else
                await ReplyAsync("User already in blacklist!");
        }

        [Command("UnbanUser")]
        [RequireOwner]
        public async Task RemoveUserFromBlacklist(IUser user)
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.BlacklistSave.Data.Remove(user.Id))
                await ReplyAsync("User removed from blacklist");
            else
                await ReplyAsync("User not added to blacklist!");
        }


        [Command("Announce")]
        [RequireOwner]
        public async Task Announce(params string[] announcement)
        {
            await Context.Channel.TriggerTypingAsync();
            foreach (SocketGuild guild in Context.Client.Guilds)
            {
                try
                {
                    await (guild.Owner as SocketUser).SendMessageAsync(string.Join(" ", announcement));
                }
                catch { }
            }
            await ReplyAsync("Announcement sent to all Guild Owners! :ok_hand:");
        }

        [Command("Shutdown"), Alias("Quit", "Shutoff")]
        [RequireOwner]
        public async Task Shutdown()
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync("Shutting down!");
            await Context.Client.LogoutAsync();
            SaveHandler.SaveAll();
            Utils.Console.ConsoleWriteLog("Shutdown via Commmand!");
            Environment.Exit(0);
        }

        [Command("Say")]
        [RequireOwner]
        public async Task ForceSay(string str)
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }
            await ReplyAsync(str);
        }

        [Command("Source")]
        [Summary("Source code!")]
        public async Task GetSource()
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync("I was written in C# using Discord.Net!: https://github.com/SunTheCourier/Marina-Bot");
        }

        [Command("invite")]
        [Summary("Invite link!")]
        public async Task GetInvite()
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync("You can invite me using this link!: https://sunthecourier.net/marina-bot");
        }

        [Command("Servers")]
        [RequireOwner]
        public async Task Getservers()
        {
            EmbedBuilder builder = new EmbedBuilder();
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
                    {
                        builder.Description += $"{guild.Name}\n";
                    }
                    builder.Description += "```";
                    break;
            }

            if (builder.Description.Length > EmbedBuilder.MaxDescriptionLength)
            {
                string[] Msgs = Misc.ConvertToDiscordSendable(builder.Description, EmbedBuilder.MaxDescriptionLength);
                for (int i = 0; i < Msgs.Length; i++)
                {
                    builder.Description = Msgs[i];
                    if (Msgs.Length - 1 == i)
                        builder.WithCurrentTimestamp();

                    await ReplyAsync(embed: builder.Build());
                    if (i == 0)
                        builder.Title = null;
                }
            }
            else
            {
                builder.WithCurrentTimestamp();
                await ReplyAsync(embed: builder.Build());
            }
        }

        [Command("Save")]
        [RequireOwner]
        public async Task ForceSaving()
        {
            await Context.Channel.TriggerTypingAsync();
            SaveHandler.SaveAll();
            await ReplyAsync("Saved all data!");
        }
    }
}
