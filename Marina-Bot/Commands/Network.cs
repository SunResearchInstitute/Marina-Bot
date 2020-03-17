using Discord;
using Discord.Commands;
using Marina.Utils;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Network : ModuleBase<SocketCommandContext>
    {
        [Command("Ping"), Summary("Pings an IP."), Alias("ddos")]
        public async Task Ping([Name("IP")]string ip)
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
                    string s;
                    if (reply.Address.ToString() != ip) s = $"{reply.Address} ({ip})";
                    else s = $"{reply.Address}";
                    builder.AddField(s, $"RTT: {reply.RoundtripTime}ms");
                    await Context.Channel.SendMessageAsync(embed: builder.Build());
                }
                else
                {
                    await Error.SendDiscordError(Context, Value: $"Ping failed!\nStatus: {reply.Status}");
                    return;
                }
            }
            catch
            {
                await Error.SendDiscordError(Context, Value: $"Ping failed!");
                return;
            }
        }
    }
}