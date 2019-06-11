using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using RK800.Utils;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RK800.Commands
{
    public class Games : ModuleBase<SocketCommandContext>
    {
        [Command("OwStats")]
        [Summary("Display information of an Overwatch player.")]
        public async Task GetOverwatchStats(string Username, string Platform)
        {
            await Context.Channel.TriggerTypingAsync();
            Platform = Platform.ToLower();

            if (Platform != "xbl" && Platform != "psn" && Platform != "pc")
            {
                await Error.SendDiscordError(Context, Value: "Invalid Platform, must be \"pc\", \"xbl\" or \"psn\"");
                return;
            }
            if (Username[Username.Length - 6] == '#')
            {
                char[] user = Username.ToArray();
                user[Username.Length - 6] = '-';
                Username = string.Join(null, user);
            }
            WebClient wc = new WebClient();
            //region is auto-detected but 'us' is in there because of compatibility
            OverwatchProfile Data;
            try
            {
                Data = JsonConvert.DeserializeObject<OverwatchProfile>(wc.DownloadString($"https://ow-api.com/v1/stats/{Platform.ToLower()}/us/{Username}/profile"));
            }
            catch (WebException e)
            {
                //ehhhhh
                if (e.Message.Contains("404"))
                {
                    await Error.SendDiscordError(Context, Value: "User not found!");
                }
                else
                {
                    await Error.SendDiscordError(Context, Value: "API has failed, please try again later!", e: e, et: Error.ExceptionType.Fatal);
                }
                return;
            }

            if (Data.Private)
            {
                await Error.SendDiscordError(Context, Value: "Profile is private!");
                return;
            }
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.Orange);
            builder.WithTitle($"{Data.Name}'s Stats");
            builder.WithThumbnailUrl(Data.Icon.AbsoluteUri);
            if (Data.Rating != 0) builder.AddField($"Competitive:", $"{Data.Rating} SR, Rank: {GetOverwatchRankName(Data.Rating)}");
            else builder.AddField("Competetive Rank:", "Not placed!");
            builder.AddField("Endorsment Level:", Data.Endorsement);
            builder.AddField("Level", (Data.Prestige * 100) + Data.Level);
            builder.AddField("Games Won:", Data.GamesWon);
            builder.AddField("Games Played in Quick Play and Competitive Play:", Data.CompetitiveStats.Games.Played + Data.QuickPlayStats.Games.Played);
            builder.AddField("Total Amount of Cards Recieved in Quick Play and Competitive Play:", Data.CompetitiveStats.Awards.Cards + Data.QuickPlayStats.Awards.Cards);
            builder.AddField("Total Amount of Medals Accquired in Quick Play and Competitive Play:", Data.CompetitiveStats.Awards.Medals + Data.QuickPlayStats.Awards.Medals);
            builder.WithCurrentTimestamp();
            await ReplyAsync(embed: builder.Build());
        }

        private string GetOverwatchRankName(int SR)
        {
            switch (SR)
            {
                case int n when n >= 1 && n <= 1499:
                    return "Bronze";

                case int n when n >= 1500 && n <= 1999:
                    return "Silver";

                case int n when n >= 2000 && n <= 2499:
                    return "Gold";

                case int n when n >= 2500 && n <= 2999:
                    return "Platinum";

                case int n when n >= 3000 && n <= 3499:
                    return "Diamond";

                case int n when n >= 3500 && n <= 3999:
                    return "Master";

                case int n when n >= 4000:
                    return "Grandmaster";
                default:
                    return null;
            }
        }
    }

    public class GameTotal
    {
        public uint Played;
        public uint Won;
    }

    public class Awards
    {
        public ulong Cards;
        public ulong Medals;
        public ulong MedalsBronze;
        public ulong MedalsSilver;
        public ulong MedalsGold;
    }

    public class OverwatchStats
    {
        public GameTotal Games;
        public Awards Awards;
    }

    public class OverwatchProfile
    {
        public ushort Endorsement;
        public Uri EndorsementIcon;
        public Uri Icon;
        public string Name;
        public byte Level;
        public Uri LevelIcon;
        public ushort Prestige;
        public Uri PrestigeIcon;
        public ushort Rating;
        public Uri RatingIcon;
        public ulong GamesWon;
        public OverwatchStats QuickPlayStats;
        public OverwatchStats CompetitiveStats;
        public bool Private;
    }
}
