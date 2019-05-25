using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using RK800.Save;
using RK800.Utils;

namespace RK800.Commands
{
    public class Tracker : ModuleBase<SocketCommandContext>
    {
        public static Dictionary<ulong, DateTime> Trackers => TrackerContext.Trackers;

        [Command("TrackMe")]
        public async Task InitTracker()
        {
            if (Trackers.TryAdd(Context.User.Id, DateTime.Now))
            {
                await ReplyAsync("You are now being monitored!");
                SaveHandler.Trackers.SaveData.Add(Context.User.Id);
            }
            else
            {
                await ReplyAsync("You are already being monitored!");
            }
        }

        [Command("UntrackMe")]
        public async Task StopTracker()
        {
            if (Trackers.Remove(Context.User.Id))
            {
                await ReplyAsync("Your are no longer being monitored!");
                SaveHandler.Trackers.SaveData.Remove(Context.User.Id);
            }
            else
            {
                await ReplyAsync("You are not being monitored!");
            }
        }

        [Command("Time")]
        public async Task GetTime()
        {
            if (Trackers.Keys.Contains(Context.User.Id))
            {
                TimeSpan ts = DateTime.Now - Trackers[Context.User.Id];
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
