using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using RK800.Save;
using System.Linq;
using System.Collections.Generic;
using RK800.Utils;
using System;
using Discord;
using System.IO;

namespace RK800.Commands
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        public static FilterSaveFile FilterSave => SaveHandler.Saves["Filter"] as FilterSaveFile;

        public static WarnSaveFile WarnsSave => SaveHandler.Saves["Warns"] as WarnSaveFile;

        public static FileInfo FilterDefaults = new FileInfo("FilterDefaults.txt");

        [RequireBotPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.KickMembers), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("Warn")]
        public async Task Warn(IGuildUser User, params string[] Reason)
        {
            string joined;
            if (Reason.Length != 0) joined = string.Join(' ', Reason);
            else joined = null;

            if (!WarnsSave.Data.ContainsKey(Context.Guild.Id))
            {
                WarnsSave.Data.Add(Context.Guild.Id, new Dictionary<ulong, WarnData>() { { User.Id, new WarnData(string.Join(' ', joined)) } });
            }
            else
            {
                WarnsSave.Data[Context.Guild.Id].Add(User.Id, new WarnData(string.Join(' ', joined)));
            }
            switch (WarnsSave.Data[Context.Guild.Id].Count)
            {
                //Based off of Komet
                case 1:
                    await User.SendMessageAsync($"You were warned on {Context.Guild.Name} and now have a warning!");
                    break;
                case 2:
                    await User.SendMessageAsync($"You were warned on {Context.Guild.Name} and now have 2 warnings! The next warn will automatically kick!");
                    break;
                case 3:
                    await User.SendMessageAsync($"You were warned on {Context.Guild.Name} and now have 3 warnings! For having 3 warnings you have been kicked, the next warning will result in a kick!");
                    await User.KickAsync();
                    break;
                case 4:
                    await User.SendMessageAsync($"You were warned on {Context.Guild.Name} and now have 4 warnings! For having 4 warnings you have been kicked again, the next warning will result in a ban from the server!");
                    await User.KickAsync();
                    break;
                case 5:
                    await User.SendMessageAsync($"You were warned on {Context.Guild.Name} and now have 5 warnings! For having 5 warnings you have been banned from the server!");
                    await User.BanAsync();
                    break;
                    //over 5
                default:
                    await User.BanAsync();
                    break;
            }
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
        [Command("InitializeFilter"), Alias("InitFilter")]
        public async Task InitFilter(bool Use_Default_Filter_Values = true)
        {
            if (FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (FilterSave.Data[Context.Guild.Id].IsEnabled)
                {
                    List<string> List = new List<string>();
                    if (Use_Default_Filter_Values) List.AddRange(File.ReadAllLines(FilterDefaults.FullName));
                    FilterSave.Data.Add(Context.Guild.Id, new FilterData(List));
                    await ReplyAsync("Filter Initialized!");
                }
                else await Error.SendDiscordError(Context, Value: "Filter has already been disabled!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialized!");
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("RemoveFilterData")]
        public async Task RemoveFilterData()
        {
            if (FilterSave.Data.Remove(Context.Guild.Id)) await ReplyAsync("Filter data removed!");
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialized!");
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("DisableFilter")]
        public async Task DisableFilter()
        {
            if (FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (FilterSave.Data[Context.Guild.Id].IsEnabled)
                {
                    FilterSave.Data[Context.Guild.Id].IsEnabled = false;
                    await ReplyAsync("Filter disabled!");
                }
            }
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialized!");
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("AddFilteredWord")]
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
        [Command("ListFilteredWords")]
        public async Task ListBadsWords()
        {
            if (FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                if (FilterSave.Data.Count > 0)
                {
                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithColor(Color.Blue);
                    builder.WithTitle("Filtered words");
                    builder.WithCurrentTimestamp();
                    builder.WithDescription(string.Join("\n", FilterSave.Data[Context.Guild.Id]));
                    await Context.User.SendMessageAsync(embed: builder.Build());
                }
                else await Error.SendDiscordError(Context, Value: "Filter contains no words!");
            }
            else await Error.SendDiscordError(Context, Value: "Filiter has not been initialized!");
        }
    }
}