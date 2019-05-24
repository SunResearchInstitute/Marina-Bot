using Discord.Commands;
using System;
using Discord.Rest;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RK800.Commands
{
    public class Dogs : ModuleBase<SocketCommandContext>
    {
        private static Timer timer;
        static Dogs()
        {
            SetUpTimer(new TimeSpan(12, 0, 0));
        }

        private static void SetUpTimer(TimeSpan alertTime)
        {
            DateTime current = DateTime.Now;
            TimeSpan timeToGo = alertTime - current.TimeOfDay;
            if (timeToGo < TimeSpan.Zero)
            {
                return;//time already passed
            }
            timer = new Timer(x =>
            {
                SendMessages();
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        private static void SendMessages()
        {
            foreach (ulong id in SaveHandler.DogMsgList.SaveData)
            {
                WebClient wc = new WebClient();
                Dictionary<string, string> jsondata = JsonConvert.DeserializeObject<Dictionary<string, string>>(wc.DownloadString("https://dog.ceo/api/breeds/image/random"));
                wc.Dispose();
                if (jsondata["status"] != "success")
                {
                    Task<RestApplication> task = Program.Client.GetApplicationInfoAsync();
                    task.Wait();
                    RestApplication application = task.Result;
                    application.Owner.SendMessageAsync($"Dog API failed!\nStatus: `{jsondata["status"]}`");
                    return;
                }
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithColor(Color.Blue);
                builder.WithImageUrl(jsondata["message"]);
                Program.Client.GetUser(id).SendMessageAsync(embed: builder.Build());
            }
        }

        [Command("Dog")]
        public async Task SendDog()
        {
            WebClient wc = new WebClient();
            Dictionary<string, string> jsondata = JsonConvert.DeserializeObject<Dictionary<string, string>>(wc.DownloadString("https://dog.ceo/api/breeds/image/random"));
            wc.Dispose();
            if (jsondata["status"] != "success")
            {
                RestApplication application = await Program.Client.GetApplicationInfoAsync();
                await application.Owner.SendMessageAsync($"Dog API failed!\nStatus: `{jsondata["status"]}`");
                return;
            }
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.Blue);
            builder.WithImageUrl(jsondata["message"]);
            builder.WithCurrentTimestamp();
            builder.WithFooter("Taken from https://dog.ceo/dog-api/");
            await Context.User.SendMessageAsync(embed: builder.Build());
        }
    }
}
