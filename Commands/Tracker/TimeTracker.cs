using Discord.Commands;
using System;
using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using RK800.Save;
using System.Timers;
using RK800.Utils;

namespace RK800.Commands.Tracker
{
    public class TimeTracker : ModuleBase<SocketCommandContext>
    {
        public static TrackerSaveFile TrackersSave => SaveHandler.Saves["Trackers"] as TrackerSaveFile;
        public static UlongTimeSpanSaveFile AlertIntervals => SaveHandler.Saves["AlertIntervals"] as UlongTimeSpanSaveFile;

        [Command("TrackMe")]
        public async Task InitTracker()
        {
            if (TrackersSave.SaveData.TryAdd(Context.User.Id, DateTime.Now))
            {
                await ReplyAsync("You are now being monitored!");
            }
            else
            {
                await ReplyAsync("You are already being monitored!");
            }
        }

        [Command("UntrackMe")]
        public async Task StopTracker()
        {
            if (TrackersSave.SaveData.Remove(Context.User.Id))
            {
                await ReplyAsync("Your are no longer being monitored!");
            }
            else
            {
                await ReplyAsync("You are not being monitored!");
            }
        }

        [Command("Time")]
        public async Task GetTime()
        {
            if (TrackersSave.SaveData.Keys.Contains(Context.User.Id))
            {
                TimeSpan ts = DateTime.Now - TrackersSave.SaveData[Context.User.Id];
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                await ReplyAsync($"You spent {elapsedTime} in {Context.User.Status} ");
            }
            else
            {
                await Error.SendDiscordError(Context, Value: "You are not being monitored!");
            }
        }

        [Command("SetAlertTime")]
        public async Task SetTimeAlert(string s)
        {
            TimeSpan time;
            if (!TimeSpan.TryParse(s, out time))
            {
                await Error.SendDiscordError(Context, Value: "Invald time interval!");
                return;
            }

            if (time < new TimeSpan(0, 10, 0))
            {
                await Error.SendDiscordError(Context, Value: "Time can not be below ten minutes!");
                return;
            }

            await ReplyAsync($"Your alert timer has been set to {time}");
            if (AlertIntervals.SaveData.ContainsKey(Context.User.Id))
            {
                AlertIntervals.SaveData[Context.User.Id] = time;

            }
            else
            {
                AlertIntervals.SaveData.Add(Context.User.Id, time);
            }
        }

        public static void CheckTime(Object source, ElapsedEventArgs e)
        {
            foreach (ulong id in TrackersSave.SaveData.Keys)
            {
                if (AlertIntervals.SaveData.ContainsKey(id))
                {
                    if (AlertIntervals.SaveData[id] <= DateTime.Now - TrackersSave.SaveData[id])
                    {
                        Program.Client.GetUser(id).SendMessageAsync($"You have recieved this DM because your self set online time alert of {AlertIntervals.SaveData[id]} has been reached!");
                    }
                }
            }
        }
    }
}
