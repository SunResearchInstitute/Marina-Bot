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
        public async Task Ping(string Ip)
        {
            try
            {
                using Ping pinger = new Ping();
                PingReply reply = pinger.Send(Ip);
                if (reply.Status == IPStatus.Success)
                {
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Color = Color.Green
                    };
                    builder.WithCurrentTimestamp();
                    string s;
                    if (reply.Address.ToString() != Ip) s = $"{reply.Address.ToString()} ({Ip})";
                    else s = $"{reply.Address.ToString()}";
                    builder.AddField(s, $"RTT: {reply.RoundtripTime}ms");
                    await Context.Channel.SendMessageAsync(embed: builder.Build());
                }
                else
                {
                    await Error.SendDiscordError(Context, Value: $"Ping failed!\nStatus: {reply.Status.ToString()}");
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