using Discord.Commands;
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

        public static FileInfo filterdefaults = new FileInfo("FilterDefaults.txt");

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
            if (!FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                List<string> List = new List<string>();
                if (Use_Default_Filter_Values) List.AddRange(File.ReadAllLines(filterdefaults.FullName));
                FilterSave.Data.Add(Context.Guild.Id, new FilterData(List));
                await ReplyAsync("Filter Initialize!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter has already been initialize!");
        }

        [RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("DisableFilter")]
        public async Task DisableFilter()
        {
            if (!FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                FilterSave.Data[Context.Guild.Id].IsEnabled = false;
                await ReplyAsync("Filter disabled!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter is already disabled!");
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
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialize!");
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
            else await Error.SendDiscordError(Context, Value: "Filiter has not been initialize!");
        }
    }
}