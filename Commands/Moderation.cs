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
        //Should we convert this file to a Readonly Array?
        public static FileInfo FilterDefaults = new FileInfo("FilterDefaults.txt");

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("Logs")]
        [Summary("Gets the logging channel.")]
        public async Task GetLogChannel()
        {
            if (SaveHandler.LogChannelsSave.Data.ContainsKey(Context.Guild.Id))
            {
                SocketGuildChannel LogChannel = Context.Guild.GetChannel(SaveHandler.LogChannelsSave.Data[Context.Guild.Id]);
                if (LogChannel != null)
                {
                    await ReplyAsync($"The Current logging channel is set to <#{LogChannel.Id}>");
                    return;
                }
            }
            await ReplyAsync("Logging channel is not set.");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("SetLogs")]
        [Summary("Sets the logging channel.")]
        public async Task SetLogChannel(SocketGuildChannel channel)
        {
            await Context.Channel.TriggerTypingAsync();
            if (!SaveHandler.LogChannelsSave.Data.ContainsKey(Context.Guild.Id))
            {
                SaveHandler.LogChannelsSave.Data.Add(Context.Guild.Id, channel.Id);
                await ReplyAsync("Log Channel has been set!");
            }
            else
            {
                SaveHandler.LogChannelsSave.Data[Context.Guild.Id] = channel.Id;
                await ReplyAsync("Log Channel has been changed!");
            }
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("RemoveLogs")]
        [Summary("Remvoes the logging channel.")]
        public async Task RemoveLogChannel()
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.LogChannelsSave.Data.Remove(Context.Guild.Id))
            {
                await ReplyAsync("Log Channel has been removed!");
            }
            else
            {
                await ReplyAsync("Log Channel has not been set!");
            }
        }

        [RequireUserPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.BanMembers)]
        [Command("Ban")]
        public async Task Banuser([RequireHierarchyAttribute]SocketGuildUser User, params string[] Reason)
        {
            await Context.Channel.TriggerTypingAsync();
            string joined;
            if (Reason.Length != 0) joined = string.Join(' ', Reason);
            else joined = null;
            string msg = $"You were banned from {Context.Guild.Name}\n";
            if (joined != null) msg += $"Reason: {joined}";
            await User.SendMessageAsync(msg);
            await User.BanAsync(reason: joined);
            await ReplyAsync($"{User} is now b& :thumbsup:");
        }

        [RequireUserPermission(GuildPermission.KickMembers), RequireBotPermission(GuildPermission.KickMembers)]
        [Command("Kick")]
        public async Task Kickuser([RequireHierarchyAttribute]SocketGuildUser User, params string[] Reason)
        {
            await Context.Channel.TriggerTypingAsync();
            string joined;
            if (Reason.Length != 0) joined = string.Join(' ', Reason);
            else joined = null;
            string msg = $"You were kicked from {Context.Guild.Name}\n";
            if (joined != null) msg += $"Reason: {joined}";
            await User.SendMessageAsync(msg);
            await User.KickAsync(joined);
            await ReplyAsync($"kicked {User} :boot:");

            if (SaveHandler.LogChannelsSave.Data.ContainsKey(User.Guild.Id))
            {
                SocketGuildChannel GuildChannel = User.Guild.GetChannel(SaveHandler.LogChannelsSave.Data[User.Guild.Id]);
                if (GuildChannel != null)
                {
                    ISocketMessageChannel LogChannel = GuildChannel as ISocketMessageChannel;
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Color = Color.Blue,
                        Title = "**Kicked**",
                        Description = $"{Context.User.Mention} kicked {User.Mention} | {User}"
                };
                    if (joined != null) builder.Description += $"\n__Reason__: \"{joined}\"";
                    await LogChannel.SendMessageAsync(embed: builder.Build());
                }
                else SaveHandler.LogChannelsSave.Data.Remove(User.Guild.Id);
            }
        }


        [RequireUserPermission(GuildPermission.BanMembers), RequireUserPermission(GuildPermission.KickMembers)]
        [Command("ClearWarns")]
        public async Task ClearWarns(SocketGuildUser User)
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.WarnsSave.Data.ContainsKey(Context.Guild.Id) && SaveHandler.WarnsSave.Data[Context.Guild.Id].ContainsKey(User.Id) && SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id].Count != 0)
            {
                SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id] = new List<WarnData>();
                await ReplyAsync($"Warns have been cleared for {User.Mention}");
            }
            else await Error.SendDiscordError(Context, Value: "There are no warns for that user!");
        }
        [Command("Warns")]
        public async Task GetWarns(SocketGuildUser User = null)
        {
            await Context.Channel.TriggerTypingAsync();
            if (User == null)
            {
                User = Context.User as SocketGuildUser;
            }
            EmbedBuilder builder = new EmbedBuilder
            {
                Title = $"Warnings for {User.Username}",
                Color = Color.Blue
            };
            if (!SaveHandler.WarnsSave.Data.ContainsKey(Context.Guild.Id) || !SaveHandler.WarnsSave.Data[Context.Guild.Id].ContainsKey(User.Id) || SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id].Count == 0)
            {
                builder.Description = "There are none! Good for you!";
                await ReplyAsync(embed: builder.Build());
                return;
            }

            for (int i = 0; i < SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id].Count; i++)
            {
                WarnData warn = SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id][i];
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
            await Context.Channel.TriggerTypingAsync();
            if (User == Context.User as IGuildUser)
            {
                await Error.SendDiscordError(Context, Value: "You can't do mod actions on yourself.");
                return;
            }
            string joined;
            if (Reason.Length != 0) joined = string.Join(' ', Reason);
            else joined = null;

            if (SaveHandler.WarnsSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (SaveHandler.WarnsSave.Data[Context.Guild.Id].ContainsKey(User.Id))
                {
                    SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id].Add(new WarnData(DateTime.Now, joined, Context.User.Id));
                }
                else
                {
                    SaveHandler.WarnsSave.Data[Context.Guild.Id].Add(User.Id, new List<WarnData>() { new WarnData(DateTime.Now, joined, Context.User.Id) });
                }
            }
            else
            {
                SaveHandler.WarnsSave.Data.Add(Context.Guild.Id, new Dictionary<ulong, List<WarnData>>() { { User.Id, new List<WarnData>() { new WarnData(DateTime.Now, joined, Context.User.Id) } } });
            }

            string reason = SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id].Last().Reason;

            string dmmsg = $"You were warned on {Context.Guild.Name} ";
            switch (SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id].Count)
            {
                //Based off of Komet
                case 1:
                    dmmsg += "and now have a warning!";
                    if (reason != null) dmmsg += $"\nThe given reason is: {reason}";
                    try
                    {
                        await User.SendMessageAsync(dmmsg);
                    }
                    catch { }
                    break;
                case 2:
                    dmmsg += "and now have 2 warnings! The next warn will automatically kick!";
                    if (reason != null) dmmsg += $"\nThe given reason is: {reason}";
                    try
                    {
                        await User.SendMessageAsync(dmmsg);
                    }
                    catch { }
                    break;
                case 3:
                    dmmsg += "and now have 3 warnings! For having 3 warnings you have been kicked, the next warning will also result in a kick!";
                    if (reason != null) dmmsg += $"\nThe given reason is: {reason}";
                    try
                    {
                        await User.SendMessageAsync(dmmsg);
                    }
                    catch { }
                    await User.KickAsync();
                    break;
                case 4:
                    dmmsg += "and now have 4 warnings! For having 4 warnings you have been kicked again, the next warning will result in a ban from the server!";
                    if (reason != null) dmmsg += $"\nThe given reason is: {reason}";
                    try
                    {
                        await User.SendMessageAsync(dmmsg);
                    }
                    catch { }
                    await User.KickAsync();
                    break;
                case 5:
                    dmmsg += "and now have 5 warnings! For having 5 warnings you have been banned from the server!";
                    if (reason != null) dmmsg += $"\nThe given reason is: {reason}";
                    try
                    {
                        await User.SendMessageAsync(dmmsg);
                    }
                    catch { }
                    await User.BanAsync();
                    break;
                //over 5
                default:
                    await User.BanAsync();
                    break;
            }

            if (SaveHandler.LogChannelsSave.Data.ContainsKey(User.Guild.Id))
            {
                SocketGuildChannel GuildChannel = User.Guild.GetChannel(SaveHandler.LogChannelsSave.Data[User.Guild.Id]);
                if (GuildChannel != null)
                {
                    ISocketMessageChannel LogChannel = GuildChannel as ISocketMessageChannel;
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Color = Color.Blue,
                        Title = "**Warned**",
                        Description = $"{Context.User.Mention} warned {User.Mention} (warn #{SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id].Count}) | {User}"
                    };
                    if (reason != null) builder.Description += $"\n__Reason__: \"{reason}\"";
                    await LogChannel.SendMessageAsync(embed: builder.Build());
                }
                else SaveHandler.LogChannelsSave.Data.Remove(User.Guild.Id);
            }

            string warningmsg = $"{User.Mention} warned. User has {SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id].Count} warning";
            if (SaveHandler.WarnsSave.Data[Context.Guild.Id][User.Id].Count > 1) warningmsg += "s.";
            else warningmsg += ".";
            await ReplyAsync(warningmsg);
        }

        public static bool MessageContainsFilteredWord(ulong server, string s)
        {
            if (SaveHandler.FilterSave.Data.ContainsKey(server))
            {
                foreach (string BadWord in SaveHandler.FilterSave.Data[server].Words)
                    if (s.Split(' ').Contains(BadWord, StringComparer.OrdinalIgnoreCase) || s.Contains(BadWord, StringComparison.OrdinalIgnoreCase)) return true;

            }
            return false;
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("InitializeFilter"), Alias("InitFilter"), Summary("Starts a word filter.")]
        public async Task InitFilter(bool Use_Default_Filter_Values = true)
        {
            await Context.Channel.TriggerTypingAsync();
            if (!SaveHandler.FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                List<string> List = new List<string>();
                if (Use_Default_Filter_Values) List.AddRange(File.ReadAllLines(FilterDefaults.FullName));
                SaveHandler.FilterSave.Data.Add(Context.Guild.Id, new FilterData(List));
                await ReplyAsync("Filter Initialized!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter has been initialized already!");
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("RemoveFilterData"), Summary("Removes all filter data.")]
        public async Task RemoveFilterData()
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.FilterSave.Data.Remove(Context.Guild.Id)) await ReplyAsync("Filter data removed!");
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialized!");
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("EnableFilter"), Summary("Enables word filtering.")]
        public async Task EnableFilter()
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (!SaveHandler.FilterSave.Data[Context.Guild.Id].IsEnabled)
                {
                    SaveHandler.FilterSave.Data[Context.Guild.Id].IsEnabled = true;
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
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (SaveHandler.FilterSave.Data[Context.Guild.Id].IsEnabled)
                {
                    SaveHandler.FilterSave.Data[Context.Guild.Id].IsEnabled = false;
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
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                SaveHandler.FilterSave.Data[Context.Guild.Id].Words.Add(Word);
                await ReplyAsync("Word added!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialized!");
        }

        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("ListFilteredWords"), Summary("Sends a DM of all filtered words.")]
        public async Task ListBadsWords()
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (SaveHandler.FilterSave.Data.Count > 0)
                {
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Color = Color.Blue,
                        Title = "Filtered words"
                    };
                    string words = string.Join("\n", SaveHandler.FilterSave.Data[Context.Guild.Id]);
                    if (EmbedBuilder.MaxDescriptionLength < words.Length)
                    {
                        string[] msgs = Misc.ConvertToDiscordSendable(words, EmbedBuilder.MaxDescriptionLength);
                        for (int i = 0; i < msgs.Length; i++)
                        {
                            string msg = msgs[i];
                            builder.Description = msg;
                            if (i == msgs.Length - 1) builder.WithCurrentTimestamp();
                            await Context.User.SendMessageAsync(embed: builder.Build());
                            if (i == 0) builder.Title = null;
                        }
                        return;
                    }
                    builder.WithCurrentTimestamp();
                    builder.Description = words;
                    await Context.User.SendMessageAsync(embed: builder.Build());
                }
                else await Error.SendDiscordError(Context, Value: "Filter contains no words!");
            }
            else await Error.SendDiscordError(Context, Value: "Filiter has not been initialized!");
        }
    }
}
