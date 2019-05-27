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

        [Command("TrackMe")]
        public async Task InitTracker()
        {
            if (!TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                TrackersSave.Data.Add(Context.User.Id, new TrackerData(DateTime.Now, TimeSpan.MaxValue, tracker: true));
                await ReplyAsync("You are now being monitored!");
            }
            else if (!TrackersSave.Data[Context.User.Id].IsTrackerEnabled)
            {
                TrackersSave.Data[Context.User.Id].IsTrackerEnabled = true;
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
            if (TrackersSave.Data.ContainsKey(Context.User.Id) && TrackersSave.Data[Context.User.Id].IsTrackerEnabled)
            {
                TrackersSave.Data[Context.User.Id].IsTrackerEnabled = false;
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
            if (TrackersSave.Data.ContainsKey(Context.User.Id) && TrackersSave.Data[Context.User.Id].IsTrackerEnabled)
            {
                TimeSpan ts = DateTime.Now - TrackersSave.Data[Context.User.Id].dt;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                await ReplyAsync($"You spent {elapsedTime} in {Context.User.Status} ");
                return;
            }

            await Error.SendDiscordError(Context, Value: "You are not being monitored!");

        }

        [Command("SetAlertTime")]
        public async Task SetTimeAlert(string s)
        {
            if (TrackersSave.Data.ContainsKey(Context.User.Id))
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

                await ReplyAsync($"Your alert timer has been set for {String.Format("{0:00}:{1:00}", time.Hours, time.Minutes)}");
                if (!TrackersSave.Data[Context.User.Id].IsAlertEnabled) TrackersSave.Data[Context.User.Id].IsAlertEnabled = true;
                TrackersSave.Data[Context.User.Id].ts = time;
            }
            else
            {
                await Error.SendDiscordError(Context, Value: "You are not being monitored!");
            }
        }

        public static void CheckTime(Object source, ElapsedEventArgs e)
        {
            foreach (ulong id in TrackersSave.Data.Keys)
            {
                if (TrackersSave.Data[id].ts == DateTime.Now - TrackersSave.Data[id].dt)
                {
                    Program.Client.GetUser(id).SendMessageAsync($"You have recieved this DM because your self set online time alert of {String.Format("{0:00}:{1:00}", TrackersSave.Data[id].ts.Hours, TrackersSave.Data[id].ts.Minutes)} has been reached!");
                }
            }
        }
    }
}
