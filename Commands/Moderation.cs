using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RK800.Save;
using RK800.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RK800.Commands
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        public static FilterSaveFile FilterSave => SaveHandler.Saves["Filter"] as FilterSaveFile;

        public static WarnSaveFile WarnsSave => SaveHandler.Saves["Warns"] as WarnSaveFile;

        //Should we convert this file to a Readonly Array?
        public static FileInfo FilterDefaults = new FileInfo("FilterDefaults.txt");

        [RequireUserPermission(GuildPermission.BanMembers)]
        [Command("Ban")]
        public async Task Banuser([RequireHierarchyAttribute]SocketGuildUser User, params string[] Reason)
        {
            string joined;
            if (Reason.Length != 0) joined = string.Join(' ', Reason);
            else joined = null;
            string msg = $"You were banned from {Context.Guild.Name}\n";
            if (joined != null) msg += $"Reason: {joined}";
            await User.SendMessageAsync(msg);
            await User.BanAsync(reason: joined);
            await ReplyAsync($"{User} is now b& :thumbsup:");
        }

        [RequireUserPermission(GuildPermission.KickMembers)]
        [Command("Kick")]
        public async Task Kickuser([RequireHierarchyAttribute]SocketGuildUser User, params string[] Reason)
        {
            string joined;
            if (Reason.Length != 0) joined = string.Join(' ', Reason);
            else joined = null;
            string msg = $"You were kicked from {Context.Guild.Name}\n";
            if (joined != null) msg += $"Reason: {joined}";
            await User.SendMessageAsync(msg);
            await User.KickAsync(joined);
            await ReplyAsync($"kicked {User} :thumbsup:");
        }


        [RequireUserPermission(GuildPermission.BanMembers), RequireUserPermission(GuildPermission.KickMembers)]
        [Command("ClearWarns")]
        public async Task ClearWarns(SocketGuildUser User)
        {
            if (WarnsSave.Data.ContainsKey(Context.Guild.Id) && WarnsSave.Data[Context.Guild.Id].ContainsKey(User.Id) && WarnsSave.Data[Context.Guild.Id][User.Id].Count != 0)
            {
                WarnsSave.Data[Context.Guild.Id][User.Id] = new List<WarnData>();
                await ReplyAsync($"Warns have been cleared for {User.Mention}");
            }
            else await Error.SendDiscordError(Context, Value: "There are no warns for that user!");
        }
        [Command("Warns")]
        public async Task GetWarns(SocketGuildUser User = null)
        {
            if (User == null)
            {
                User = Context.User as SocketGuildUser;
            }
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"Warnings for {User.Username}");
            builder.WithColor(Color.Blue);
            if (!WarnsSave.Data[Context.Guild.Id].ContainsKey(User.Id) || WarnsSave.Data[Context.Guild.Id][User.Id].Count == 0)
            {
                builder.WithDescription("There are none! Good for you!");
                await ReplyAsync(embed: builder.Build());
                return;
            }

            for (int i = 0; i < WarnsSave.Data[Context.Guild.Id][User.Id].Count; i++)
            {
                WarnData warn = WarnsSave.Data[Context.Guild.Id][User.Id][i];
                string value = $"Issuer: {Context.Guild.GetUser(warn.Issuer).Mention} ({warn.Issuer})\n";
                if (warn.Reason != null) value += $"Reason: {warn.Reason}";
                else value += "No reason given";
                builder.AddField($"Warn {i + 1}: {warn.Time.ToUniversalTime()}", value);
            }
            await ReplyAsync(embed: builder.Build());
        }
        [RequireBotPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.KickMembers), RequireUserPermission(GuildPermission.BanMembers), RequireUserPermission(GuildPermission.KickMembers)]
        [Command("Warn")]
        public async Task Warn([RequireHierarchyAttribute]SocketGuildUser User, params string[] Reason)
        {
            if (User == Context.User as IGuildUser)
            {
                await Error.SendDiscordError(Context, Value: "You can't do mod actions on yourself.");
                return;
            }
            string joined;
            if (Reason.Length != 0) joined = string.Join(' ', Reason);
            else joined = null;

            if (WarnsSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (WarnsSave.Data[Context.Guild.Id].ContainsKey(User.Id))
                {
                    WarnsSave.Data[Context.Guild.Id][User.Id].Add(new WarnData(DateTime.Now, joined, Context.User.Id));
                }
                else
                {
                    WarnsSave.Data[Context.Guild.Id].Add(User.Id, new List<WarnData>() { new WarnData(DateTime.Now, joined, Context.User.Id) });
                }
            }
            else
            {
                WarnsSave.Data.Add(Context.Guild.Id, new Dictionary<ulong, List<WarnData>>() { { User.Id, new List<WarnData>() { new WarnData(DateTime.Now, joined, Context.User.Id) } } });
            }

            string dmmsg = $"You were warned on {Context.Guild.Name} ";
            switch (WarnsSave.Data[Context.Guild.Id][User.Id].Count)
            {
                //Based off of Komet
                case 1:
                    dmmsg += "and now have a warning!";
                    if (WarnsSave.Data[Context.Guild.Id][User.Id][0].Reason != null) dmmsg += $"The given reason is: {WarnsSave.Data[Context.Guild.Id][User.Id][0].Reason}";
                    await User.SendMessageAsync(dmmsg);
                    break;
                case 2:
                    dmmsg += "and now have 2 warnings! The next warn will automatically kick!";
                    if (WarnsSave.Data[Context.Guild.Id][User.Id][1].Reason != null) dmmsg += $"The given reason is: {WarnsSave.Data[Context.Guild.Id][User.Id][1].Reason}";
                    await User.SendMessageAsync(dmmsg);
                    break;
                case 3:
                    dmmsg += "and now have 3 warnings! For having 3 warnings you have been kicked, the next warning will also result in a kick!";
                    if (WarnsSave.Data[Context.Guild.Id][User.Id][2].Reason != null) dmmsg += $"The given reason is: {WarnsSave.Data[Context.Guild.Id][User.Id][2].Reason}";
                    await User.SendMessageAsync(dmmsg);
                    await User.KickAsync();
                    break;
                case 4:
                    dmmsg += "and now have 4 warnings! For having 4 warnings you have been kicked again, the next warning will result in a ban from the server!";
                    if (WarnsSave.Data[Context.Guild.Id][User.Id][3].Reason != null) dmmsg += $"The given reason is: {WarnsSave.Data[Context.Guild.Id][User.Id][3].Reason}";
                    await User.SendMessageAsync(dmmsg);
                    await User.KickAsync();
                    break;
                case 5:
                    dmmsg += "and now have 5 warnings! For having 5 warnings you have been banned from the server!";
                    if (WarnsSave.Data[Context.Guild.Id][User.Id][4].Reason != null) dmmsg += $"The given reason is: {WarnsSave.Data[Context.Guild.Id][User.Id][4].Reason}";
                    await User.SendMessageAsync(dmmsg);
                    await User.BanAsync();
                    break;
                //over 5
                default:
                    await User.BanAsync();
                    break;
            }
            string warningmsg = $"{User.Mention} warned. User has {WarnsSave.Data[Context.Guild.Id][User.Id].Count} warning";
            if (WarnsSave.Data[Context.Guild.Id].Count > 1) warningmsg += "s.";
            else warningmsg += ".";
            await ReplyAsync(warningmsg);
        }

        public static bool MessageContainsFilteredWord(ulong server, string s)
        {
            if (FilterSave.Data.ContainsKey(server))
            {
                foreach (string BadWord in FilterSave.Data[server].Words)
                    if (s.Split(' ').Contains(BadWord, StringComparer.OrdinalIgnoreCase) || s.Contains(BadWord, StringComparison.OrdinalIgnoreCase)) return true;

            }
            return false;
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("InitializeFilter"), Alias("InitFilter"), Summary("Starts a word filter.")]
        public async Task InitFilter(bool Use_Default_Filter_Values = true)
        {
            if (!FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                List<string> List = new List<string>();
                if (Use_Default_Filter_Values) List.AddRange(File.ReadAllLines(FilterDefaults.FullName));
                FilterSave.Data.Add(Context.Guild.Id, new FilterData(List));
                await ReplyAsync("Filter Initialized!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter has been initialized already!");
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("RemoveFilterData"), Summary("Removes all filter data.")]
        public async Task RemoveFilterData()
        {
            if (FilterSave.Data.Remove(Context.Guild.Id)) await ReplyAsync("Filter data removed!");
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialized!");
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("EnableFilter"), Summary("Enables word filtering.")]
        public async Task EnableFilter()
        {
            if (FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (!FilterSave.Data[Context.Guild.Id].IsEnabled)
                {
                    FilterSave.Data[Context.Guild.Id].IsEnabled = true;
                    await ReplyAsync("Filter disabled!");
                }
                else await Error.SendDiscordError(Context, Value: "Filter is already enabled!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialized!");
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("DisableFilter"), Summary("Disables word filtering.")]
        public async Task DisableFilter()
        {
            if (FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (FilterSave.Data[Context.Guild.Id].IsEnabled)
                {
                    FilterSave.Data[Context.Guild.Id].IsEnabled = false;
                    await ReplyAsync("Filter disabled!");
                }
                else await Error.SendDiscordError(Context, Value: "Filter is already disabled!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialized!");
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("AddFilteredWord"), Summary("Adds a word to the filter.")]
        public async Task AddBadWord(string Word)
        {

            if (FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                FilterSave.Data[Context.Guild.Id].Words.Add(Word);
                await ReplyAsync("Word added!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialized!");
        }

        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("ListFilteredWords"), Summary("Sends a DM of all filtered words.")]
        public async Task ListBadsWords()
        {
            if (FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (FilterSave.Data.Count > 0)
                {
                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithColor(Color.Blue);
                    builder.WithTitle("Filtered words");
                    string words = string.Join("\n", FilterSave.Data[Context.Guild.Id]);
                    if (EmbedBuilder.MaxDescriptionLength < words.Length)
                    {
                        string[] msgs = Misc.ConvertToDiscordSendable(words, EmbedBuilder.MaxDescriptionLength);
                        for (int i = 0; i < msgs.Length; i++)
                        {
                            string msg = msgs[i];
                            builder.WithDescription(msg);
                            if (i == msgs.Length) builder.WithCurrentTimestamp();
                            await Context.User.SendMessageAsync(embed: builder.Build());
                            if (i == 0) builder.Title = null;
                        }
                        return;
                    }
                    else builder.WithCurrentTimestamp();
                    builder.WithDescription(words);
                    await Context.User.SendMessageAsync(embed: builder.Build());
                }
                else await Error.SendDiscordError(Context, Value: "Filter contains no words!");
            }
            else await Error.SendDiscordError(Context, Value: "Filiter has not been initialized!");
        }
    }
}
