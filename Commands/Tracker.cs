using Discord;
using Discord.Commands;
using RK800.Save;
using RK800.Utils;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace RK800.Commands
{
    public class Tracker : ModuleBase<SocketCommandContext>
    {
        [Command("TrackMe")]
        [Summary("Starts tracking your online time.")]
        public async Task InitTracker()
        {
            await Context.Channel.TriggerTypingAsync();
            if (!SaveHandler.TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                SaveHandler.TrackersSave.Data.Add(Context.User.Id, new TrackerData(DateTime.Now, TimeSpan.MaxValue, true));
                await ReplyAsync("You are now being monitored!");
            }
            else if (!SaveHandler.TrackersSave.Data[Context.User.Id].IsTrackerEnabled)
            {
                SaveHandler.TrackersSave.Data[Context.User.Id].IsTrackerEnabled = true;
                await ReplyAsync("You are now being monitored!");
            }
            else
            {
                await ReplyAsync("You are already being monitored!");
            }
        }

        [Command("UntrackMe")]
        [Summary("Stops tracking your online time.")]
        public async Task StopTracker()
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.TrackersSave.Data.ContainsKey(Context.User.Id) && SaveHandler.TrackersSave.Data[Context.User.Id].IsTrackerEnabled)
            {
                SaveHandler.TrackersSave.Data[Context.User.Id].IsTrackerEnabled = false;
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
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.TrackersSave.Data.ContainsKey(Context.User.Id) && SaveHandler.TrackersSave.Data[Context.User.Id].IsTrackerEnabled)
            {
                TimeSpan ts = DateTime.Now - SaveHandler.TrackersSave.Data[Context.User.Id].dt;
                string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                await ReplyAsync($"You spent {elapsedTime} in {Context.User.Status} ");
                return;
            }

            await Error.SendDiscordError(Context, Value: "You are not being monitored!");

        }

        [Command("SetAlert")]
        [Summary("Sets the time you want to be alerted at.")]
        public async Task SetTimeAlert(string Time_Span, params string[] Message)
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                if (!TimeSpan.TryParse(Time_Span, out TimeSpan time))
                {
                    await Error.SendDiscordError(Context, Value: "Invalid time interval!");
                    return;
                }

                if (time < new TimeSpan(0, 10, 0))
                {
                    await Error.SendDiscordError(Context, Value: "Time can not be below ten minutes!");
                    return;
                }

                if (time > new TimeSpan(23, 59, 0))
                {
                    await Error.SendDiscordError(Context, Value: "Time can not be above 23:59");
                    return;
                }

                string joined;
                if (Message.Length != 0) joined = string.Join(" ", Message);
                else joined = null;
                EmbedBuilder builder = new EmbedBuilder
                {
                    Color = Color.Blue,
                    Title = "Alert Timer"
                };
                builder.WithCurrentTimestamp();
                string value = joined != null ? $"With message: \"{joined}\"" : "No Message was attached!";
                builder.AddField($"Your alert timer has been set for {string.Format("{0:00}:{1:00}", time.Hours, time.Minutes)}", value);
                builder.WithFooter("Please make sure I can send you DMs.");
                SaveHandler.TrackersSave.Data[Context.User.Id].DmReason = joined;
                SaveHandler.TrackersSave.Data[Context.User.Id].IsAlertEnabled = true;
                SaveHandler.TrackersSave.Data[Context.User.Id].ts = time;
                await ReplyAsync(embed: builder.Build());
            }
            else
            {
                await Error.SendDiscordError(Context, Value: "You are not being monitored!");
            }
        }

        [Command("GetAlert")]
        [Summary("Gets the time you want to be alerted at.")]
        public async Task GetTimeAlert()
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                if (!SaveHandler.TrackersSave.Data[Context.User.Id].IsAlertEnabled)
                {
                    await Error.SendDiscordError(Context, Value: "Your alert timer has not been set!");
                    return;
                }

                string reply = $"You have set your alert timer to {string.Format("{0:00}:{1:00}", SaveHandler.TrackersSave.Data[Context.User.Id].ts.Hours, SaveHandler.TrackersSave.Data[Context.User.Id].ts.Minutes)}";
                if (!string.IsNullOrWhiteSpace(SaveHandler.TrackersSave.Data[Context.User.Id].DmReason)) reply += $" with message \"{SaveHandler.TrackersSave.Data[Context.User.Id].DmReason}\"";
                else reply += ".";
                if (reply.Length > 2000)
                {
                    string[] msgs = Misc.ConvertToDiscordSendable(reply);
                    foreach (string msg in msgs)
                        await ReplyAsync(msg);
                }
                else await ReplyAsync(reply);
            }
            else await Error.SendDiscordError(Context, Value: "You are not being monitored!");
        }

        [Command("StopAlert")]
        [Summary("Removes the time you want to be alerted at.")]
        public async Task RemoveTimeAlert()
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                if (!SaveHandler.TrackersSave.Data[Context.User.Id].IsAlertEnabled)
                {
                    await Error.SendDiscordError(Context, Value: "Your alert timer has not been set!");
                    return;
                }
                SaveHandler.TrackersSave.Data[Context.User.Id].IsAlertEnabled = false;
                await ReplyAsync("Your alert timer has been disabled!");
            }
            else await Error.SendDiscordError(Context, Value: "You are not being monitored!");
        }

        [Command("RemoveAllTrackerData")]
        [Summary("Removes all Tracker and Alarm Data.")]
        public async Task RemoveAllData()
        {
            await Context.Channel.TriggerTypingAsync();
            if (SaveHandler.TrackersSave.Data.ContainsKey(Context.User.Id))
            {
                SaveHandler.TrackersSave.Data.Remove(Context.User.Id);
                await ReplyAsync("All data removed!");
            }
            else
            {
                await Error.SendDiscordError(Context, Value: "You do not have any data!");
            }
        }

        //This will not cause the program to crash if it fails
        public static void CheckTime(object source, ElapsedEventArgs e) => _ = SendMessagesAsync();

        private static async Task SendMessagesAsync()
        {
            foreach (ulong id in SaveHandler.TrackersSave.Data.Keys)
            {
                if (SaveHandler.TrackersSave.Data[id].IsTrackerEnabled && SaveHandler.TrackersSave.Data[id].IsAlertEnabled && SaveHandler.TrackersSave.Data[id].ts <= DateTime.Now - SaveHandler.TrackersSave.Data[id].dt)
                {
                    string msg = $"You have recieved this DM because your self set online time alert of {string.Format("{0:00}:{1:00}", SaveHandler.TrackersSave.Data[id].ts.Hours, SaveHandler.TrackersSave.Data[id].ts.Minutes)} has been reached";
                    if (!string.IsNullOrWhiteSpace(SaveHandler.TrackersSave.Data[id].DmReason)) msg += $" with message \"{SaveHandler.TrackersSave.Data[id].DmReason}\"";
                    else msg += "!";
                    if (msg.Length > 2000)
                    {
                        string[] msgs = Misc.ConvertToDiscordSendable(msg);
                        foreach (string reply in msgs)
                        {
                            await Program.Client.GetUser(id).SendMessageAsync(reply);
                        }
                    }
                    else
                    {
                        await Program.Client.GetUser(id).SendMessageAsync(msg);
                    }
                    await Program.Client.GetUser(id).SendMessageAsync("Your alert time has also been reset!");
                    SaveHandler.TrackersSave.Data[id].IsAlertEnabled = false;
                    SaveHandler.TrackersSave.Data[id].DmReason = null;
                }
            }
        }
    }
}
