using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Marina.Properties;
using Marina.Save;
using Marina.Utils;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Client : ModuleBase<SocketCommandContext>
    {
        [Command("BanUser")]
        [RequireOwner]
        public async Task AddUserToBlacklist(IUser user)
        {
            if (!SaveHandler.BlacklistSave.Contains(user.Id))
            {
                SaveHandler.BlacklistSave.Add(user.Id);
                await ReplyAsync("User added to blacklist!");
            }
            else
                await ReplyAsync("User already in blacklist!");
        }

        [Command("UnbanUser")]
        [RequireOwner]
        public async Task RemoveUserFromBlacklist(IUser user)
        {
            if (SaveHandler.BlacklistSave.Remove(user.Id))
                await ReplyAsync("User removed from blacklist");
            else
                await ReplyAsync("User not added to blacklist!");
        }


        [Command("Announce")]
        [RequireOwner]
        public async Task Announce(params string[] announcement)
        {
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

        [Command("Shutdown"), Alias("Quit", "Shutoff", "Stop")]
        [RequireOwner]
        public async Task Shutdown()
        {
            await ReplyAsync("Shutting down!");
            await Context.Client.LogoutAsync();
            SaveHandler.SaveAll();
            await Utils.Console.WriteLog("Shutdown via Commmand!");
            Environment.Exit(0);
        }

        [Command("Say")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [HideCommand]
        public async Task ForceSay(params string[] str)
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }
            await ReplyAsync(string.Join(' ', str));
        }


        [Command("CSay")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [HideCommand]
        public async Task ChannelForceSay(SocketTextChannel channel, params string[] str)
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }
            await channel.SendMessageAsync(string.Join(' ', str));
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
            SaveHandler.SaveAll();
            await ReplyAsync("Saved all data!");
        }

        [Command("Version"), Alias("Commit")]
        [Summary("Revision Number")]
        public async Task GetVersion() => await ReplyAsync($"Git Commit: {Encoding.UTF8.GetString(Resources.CurrentCommit)}");

        [Command("Source")]
        [Summary("Source code!")]
        public async Task GetSource() => await ReplyAsync("I was written in C# using Discord.Net!: https://github.com/SunTheCourier/Marina-Bot");

        [Command("Invite")]
        [Summary("Invite link!")]
        public async Task GetInvite() => await ReplyAsync("You can invite me using this link!: https://sunthecourier.net/marina-bot");

        [Command("Vote")]
        [Summary("Top.gg vote link!")]
        public async Task GetVote() => await ReplyAsync("you can vote for Marina-bot on top.gg: https://top.gg/bot/580901187931603004");
    }
}
