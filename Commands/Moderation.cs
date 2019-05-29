using Discord.Commands;
using System.Threading.Tasks;
using RK800.Save;
using System.Linq;
using System.Collections.Generic;
using RK800.Utils;
using Discord;
using System.IO;

namespace RK800.Commands
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        public static UlongStringListSaveFile FilterSave => SaveHandler.Saves["Filter"] as UlongStringListSaveFile;

        public static FileInfo filterdefaults = new FileInfo("FilterDefaults.txt");

        public static bool MessageContainsFilteredWord(ulong server, string s)
        {
            if (FilterSave.Data.ContainsKey(server))
            {
                foreach (string BadWord in FilterSave.Data[server])
                    if (s.ToLower().Split(' ').Contains(BadWord)) return true;
            }
            return false;
        }

        [Command("InitializeFilter"), Alias("InitFilter")]
        public async Task InitFilter(bool Use_Default_Filter_Values = true)
        {
            if (!FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                List<string> List = new List<string>();
                if (Use_Default_Filter_Values) List.AddRange(File.ReadAllLines(filterdefaults.FullName));
                FilterSave.Data.Add(Context.Guild.Id, List);
                await ReplyAsync("Filter Initialize!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter has already been initialize!");

        }

        [Command("AddFilteredWord")]
        public async Task AddBadWord(string Word)
        {
            
            if (FilterSave.Data.ContainsKey(Context.Guild.Id))
            {
                FilterSave.Data[Context.Guild.Id].Add(Word);
                await ReplyAsync("Word added!");
            }
            else await Error.SendDiscordError(Context, Value: "Filter has not been initialize!");
        }

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
                    await ReplyAsync(embed: builder.Build());
                }
                else await Error.SendDiscordError(Context, Value: "Filter contains no words!");
            }
            else await Error.SendDiscordError(Context, Value: "Filiter has not been initialize!");
        }
    }
}