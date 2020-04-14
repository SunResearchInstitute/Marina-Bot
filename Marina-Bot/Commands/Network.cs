using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Marina.Utils;

namespace Marina.Commands
{
    public class Network : ModuleBase<SocketCommandContext>
    {
        [Command("Ping")]
        [Summary("Pings an IP.")]
        [Alias("ddos")]
        public async Task Ping([Name("IP")] string ip)
        {
            try
            {
                using Ping pinger = new Ping();
                PingReply reply = pinger.Send(ip);
                if (reply.Status == IPStatus.Success)
                {
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Color = Color.Green
                    };
                    builder.WithCurrentTimestamp();
                    string s = reply.Address.ToString() != ip ? $"{reply.Address} ({ip})" : $"{reply.Address}";
                    builder.AddField(s, $"RTT: {reply.RoundtripTime}ms");
                    await Context.Channel.SendMessageAsync(embed: builder.Build());
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
    }
}