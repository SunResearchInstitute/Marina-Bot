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
            OverwatchProfile data;
            try
            {
                data = JsonConvert.DeserializeObject<OverwatchProfile>(wc.DownloadString($"https://ow-api.com/v1/stats/{Platform}/us/{Username}/profile"));
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

            //we cant access the `private` bool so we will check like this
            if (data.gamesWon == 0 && data.rating == 0)
            {
                await Error.SendDiscordError(Context, Value: "Profile is private/No Data!");
                return;
            }
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.Orange);
            builder.WithTitle($"{data.name}'s Stats");
            builder.WithThumbnailUrl(data.icon.AbsoluteUri);
            if (data.rating != 0) builder.AddField($"Competitive:", $"{data.rating} SR, Rank: {GetOverwatchRankName(data.rating)}");
            else builder.AddField("Competetive Rank:", "Not placed!");
            builder.AddField("Endorsment Level:", data.endorsement);
            builder.AddField("Level", (data.prestige * 100) + data.level);
            builder.AddField("Games Won:", data.gamesWon);
            builder.AddField("Games Played in Quick Play and Competitive Play:", data.competitiveStats.games.played + data.quickPlayStats.games.played);
            builder.AddField("Total Amount of Cards Recieved in Quick Play and Competitive Play:", data.competitiveStats.awards.cards + data.quickPlayStats.awards.cards);
            builder.AddField("Total Amount of Medals Accquired in Quick Play and Competitive Play:", data.competitiveStats.awards.medals + data.quickPlayStats.awards.medals);
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
        public uint played;
        public uint won;
    }

    public class Awards
    {
        public ulong cards;
        public ulong medals;
        public ulong medalsBronze;
        public ulong medalsSilver;
        public ulong medalsGold;
    }

    public class OverwatchStats
    {
        public GameTotal games;
        public Awards awards;
    }

    public class OverwatchProfile
    {
        public ushort endorsement;
        public Uri endorsementIcon;
        public Uri icon;
        public string name;
        public byte level;
        public Uri LevelIcon;
        public ushort prestige;
        public Uri prestigeIcon;
        public ushort rating;
        public Uri ratingIcon;
        public ulong gamesWon;
        public OverwatchStats quickPlayStats;
        public OverwatchStats competitiveStats;
    }
}
