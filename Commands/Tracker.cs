using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Discord.WebSocket;
using System.Linq;
using RK800.Utils;

namespace RK800.Commands
{
    public class Tracker : ModuleBase<SocketCommandContext>
    {
        [Command("TrackMe")]
        public async Task InitTracker()
        {
            if (TrackerContext.Trackers.TryAdd(Context.User.Id, DateTime.Now))
            {
                await ReplyAsync("Your online time is now being monitored!");
            }
            else
            {
                await ReplyAsync("Your time is already being monitored!");
            }
        }

        [Command("Time")]
        public async Task GetTime()
        {
            if (TrackerContext.Trackers.Keys.Contains(Context.User.Id))
            {
                TimeSpan ts = DateTime.Now - TrackerContext.Trackers[Context.User.Id];
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                await ReplyAsync($"You spent {elapsedTime} in {Context.User.Status} ");
            }
            else
            {
                await Error.SendDiscordError(Context, Value: "You are not being monitored!");
            }
        }


    }
}
