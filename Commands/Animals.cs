﻿using Discord;
using Discord.Commands;
using Discord.Rest;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Discord.Net;

namespace Marina.Commands
{
    public class Animals : ModuleBase<SocketCommandContext>
    {
        [Command("Dog")]
        [Summary("Gets a random dog picture.")]
        public async Task SendDog()
        {
            //This just downloads the json which should be fine
            WebClient wc = new WebClient();
            Dictionary<string, string> jsondata = JsonConvert.DeserializeObject<Dictionary<string, string>>(wc.DownloadString("https://dog.ceo/api/breeds/image/random"));
            wc.Dispose();
            if (jsondata["status"] != "success")
            {
                RestApplication application = await Program.Client.GetApplicationInfoAsync();
                await application.Owner.SendMessageAsync($"Dog API failed!\nStatus: `{jsondata["status"]}`");
                return;
            }
            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.Blue,
                ImageUrl = jsondata["message"]
            };
            builder.WithCurrentTimestamp();
            builder.WithFooter("Taken from https://dog.ceo/dog-api/");
            try
            {
                await Context.User.SendMessageAsync(embed: builder.Build());
            }
            catch (HttpException)
            {
                await ReplyAsync("Unable to send DM!");
                return;
            }
        }

        [Command("Cat")]
        [Summary("Gets a random cat picture.")]
        public async Task SendCat()
        {
            WebClient wc = new WebClient();
            List<Dictionary<string, object>> jsondata = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(wc.DownloadString("https://api.thecatapi.com/v1/images/search?format=json"));
            wc.Dispose();

            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.Blue,
                ImageUrl = jsondata[0]["url"].ToString()
            };
            builder.WithCurrentTimestamp();
            builder.WithFooter("Taken from https://thecatapi.com/");
            try
            {
                await Context.User.SendMessageAsync(embed: builder.Build());
            }
            catch (HttpException)
            {
                await ReplyAsync("Unable to send DM!");
                return;
            }
        }
    }
}
