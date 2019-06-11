using System.Net.NetworkInformation;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using RK800.Utils;

namespace RK800.Commands
{
    public class Network : ModuleBase<SocketCommandContext>
    {
        [Command("Ping"), Summary("Pings an IP.")]
        public async Task Ping(string Ip)
        {
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(Ip);
                if (reply.Status == IPStatus.Success)
                {
                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithColor(Color.Green);
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