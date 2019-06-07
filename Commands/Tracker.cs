using Discord.Commands;
using System;
using Discord;
using System.Threading.Tasks;
using RK800.Save;
using System.Timers;
using RK800.Utils;

namespace RK800.Commands
{
    public class Tracker : ModuleBase<SocketCommandContext>
    {
        public static TrackerSaveFile TrackersSave => SaveHandler.Saves["Trackers"] as TrackerSaveFile;

        [Command("TrackMe")]
        [Summary("Starts tracking your online time.")]
        public async Task InitTracker()
        {
            if (!TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                TrackersSave.Data.Add(Context.User.Id, new TrackerData(DateTime.Now, TimeSpan.MaxValue, true));
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

        [Command("UnTrackMe")]
        [Summary("Stops tracking your online time.")]
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
        [Summary("Gets the amount of time you have been in one status for.")]
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
        [Summary("Sets the time you want to be alerted at.")]
        public async Task SetTimeAlert(string Time_Span, params string[] Message)
        {
            if (TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                if (!TimeSpan.TryParse(Time_Span, out TimeSpan time))
                {
                    await Error.SendDiscordError(Context, Value: "Invald time interval!");
                    return;
                }

                if (time < new TimeSpan(0, 10, 0))
                {
                    await Error.SendDiscordError(Context, Value: "Time can not be below ten minutes!");
                    return;
                }

                string reply = $"Your alert timer has been set for {string.Format("{0:00}:{1:00}", time.Hours, time.Minutes)}";
                if (!string.IsNullOrWhiteSpace(string.Join(" ", Message))) reply += $" with message \"{string.Join(" ", Message)}\"";
                else reply += ".";
                TrackersSave.Data[Context.User.Id].DmReason = string.Join(" ", Message);
                TrackersSave.Data[Context.User.Id].IsAlertEnabled = true;
                TrackersSave.Data[Context.User.Id].ts = time;
                await ReplyAsync(reply);
            }
            else
            {
                await Error.SendDiscordError(Context, Value: "You are not being monitored!");
            }
        }

        [Command("GetAlertTime")]
        [Summary("Gets the time you want to be alerted at.")]
        public async Task GetTimeAlert()
        {
            if (TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                if (!TrackersSave.Data[Context.User.Id].IsAlertEnabled)
                {
                    await Error.SendDiscordError(Context, Value: "Your alert timer has not been set!");
                    return;
                }

                string reply = $"You have set your alert timer to {string.Format("{0:00}:{1:00}", TrackersSave.Data[Context.User.Id].ts.Hours, TrackersSave.Data[Context.User.Id].ts.Minutes)}";
                if (!string.IsNullOrWhiteSpace(TrackersSave.Data[Context.User.Id].DmReason)) reply += $" with message \"{TrackersSave.Data[Context.User.Id].DmReason}\"";
                else reply += ".";
                await ReplyAsync(reply);
                return;
            }
            else await Error.SendDiscordError(Context, Value: "You are not being monitored!");
        }

        [Command("StopAlertTimer")]
        [Summary("Removes the time you want to be alerted at.")]
        public async Task RemoveTimeAlert()
        {
            if (TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                if (!TrackersSave.Data[Context.User.Id].IsAlertEnabled)
                {
                    await Error.SendDiscordError(Context, Value: "Your alert timer has not been set!");
                    return;
                }
                TrackersSave.Data[Context.User.Id].IsAlertEnabled = false;
                await ReplyAsync("Your alert timer has been disabled!");
            }
            else await Error.SendDiscordError(Context, Value: "You are not being monitored!");
        }

        [Command("RemoveAllTrackerData")]
        [Summary("Removes all Tracker and Alarm Data")]
        public async Task RemoveAllData()
        {
            if (TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                TrackersSave.Data.Remove(Context.User.Id);
                await ReplyAsync("All data removed!");
            }
            else
            {
                await Error.SendDiscordError(Context, Value: "You do not have any data!");
            }
        }

        public static async void CheckTimeAsync(Object source, ElapsedEventArgs e)
        {
            foreach (ulong id in TrackersSave.Data.Keys)
            {
                if (TrackersSave.Data[id].IsTrackerEnabled && TrackersSave.Data[id].IsAlertEnabled && TrackersSave.Data[id].ts <= DateTime.Now - TrackersSave.Data[id].dt)
                {
                    string msg = $"You have recieved this DM because your self set online time alert of {string.Format("{0:00}:{1:00}", TrackersSave.Data[id].ts.Hours, TrackersSave.Data[id].ts.Minutes)} has been reached";
                    if (!string.IsNullOrWhiteSpace(TrackersSave.Data[id].DmReason)) msg += $" with message \"{TrackersSave.Data[id].DmReason}\"";
                    else msg += "!";
                    await Program.Client.GetUser(id).SendMessageAsync(msg);
                    await Program.Client.GetUser(id).SendMessageAsync("Your alert time has also been reset!");
                    TrackersSave.Data[id].IsAlertEnabled = false;
                }
            }
        }
    }
}
