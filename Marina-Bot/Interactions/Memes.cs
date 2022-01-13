using Discord;
using Discord.Interactions;
using Marina.Utils;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Marina.Interactions.Dog
{
    public class Memes : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("meme", "Gets a random meme.")]
        public async Task UploadDog(bool allowNsfw = false)
        {
            MemeData jsonData;
            using HttpClient client = new();
            try
            {
                do
                {
                    jsonData = JsonConvert.DeserializeObject<MemeData>(await client.GetStringAsync("https://meme-api.herokuapp.com/gimme"));
                }
                while (jsonData.Nsfw && !allowNsfw);
            }
            catch (WebException e)
            {
                await Error.SendDiscordError(Context, value: $"{e.Message}", e: e);
                return;
            }


            EmbedBuilder builder = new()
            {
                Color = Color.Teal,
                ImageUrl = jsonData.Url.ToString()
            };
            builder.WithCurrentTimestamp();
            builder.WithFooter("Taken from https://meme-api.herokuapp.com/gimme");

            await RespondAsync(embed: builder.Build(), ephemeral: jsonData.Nsfw);
        }
    }

    public partial class MemeData
    {
        [JsonProperty("postLink")]
        public Uri PostLink { get; set; }

        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("nsfw")]
        public bool Nsfw { get; set; }

        [JsonProperty("spoiler")]
        public bool Spoiler { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("ups")]
        public long Ups { get; set; }

        [JsonProperty("preview")]
        public Uri[] Preview { get; set; }
    }
}