using Discord;
using Discord.Interactions;
using Marina.Utils;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Web;

namespace Marina.Interactions
{
    public class Network : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "Pings an IP.")]
        public async Task Ping([Summary("ip")] string ip)
        {
            try
            {
                using Ping pinger = new();
                PingReply reply = pinger.Send(ip);
                if (reply.Status == IPStatus.Success)
                {
                    EmbedBuilder builder = new()
                    {
                        Color = Color.Green
                    };
                    builder.WithCurrentTimestamp();
                    string s = reply.Address.ToString() != ip ? $"{reply.Address} ({ip})" : $"{reply.Address}";
                    builder.AddField(s, $"RTT: {reply.RoundtripTime}ms");
                    await RespondAsync(embed: builder.Build());
                }
                else
                {
                    await Error.SendDiscordError(Context, value: $"Ping failed!\nStatus: {reply.Status}");
                }
            }
            catch
            {
                await Error.SendDiscordError(Context, value: "Ping failed!");
            }
        }

        [SlashCommand("lmddgtfy", "For when you need to help someone out with an explanation.")]
        public async Task Lmddgtfy(string searchTerm) => await RespondAsync($"https://lmddgtfy.net/?q={HttpUtility.UrlEncode(searchTerm)}");
    }
}