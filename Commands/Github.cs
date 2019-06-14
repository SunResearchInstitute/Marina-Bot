using CommandLine;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Octokit;
using RK800.Utils;
using System;
using RK800;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Error = RK800.Utils.Error;

namespace RK800.Commands
{
    public class Github : ModuleBase<SocketCommandContext>
    {
        public class Options
        {
            [Option('t', "tag", Required = false)]
            public string Tag { get; set; }

            [Option('l', "listags", Required = false, Default = false)]
            public bool ListTags { get; set; }

            [Option('m', "maxtagdisplaylength", Required = false, Default = 12)]
            public int Length { get; set; }

            [Option('p', "prerelease", Required = false, Default = false)]
            public bool AllowPrerelease { get; set; }

            [Value(0, Required = true)]
            public string User { get; set; }

            [Value(1, Required = true)]
            public string Repo { get; set; }

            [Option('d', "description", Required = false, Default = false)]
            public bool Desc { get; set; }
        }

        [Command("GitRelease")]
        [Summary("Gets a release from the specificed Github repository.\nUser and Repository must be included anywhere in the command in that order.\nAvaliable options:\n--tag, -t=string (default: null)\n--description, -d=bool (default: false)\n--prerelease, -p=bool (default: false)\n--maxtagdisplaylength, -n=int (defualt: 12)")]
        public async Task GetRelease(params string[] Arguments)
        {
            await Context.Channel.TriggerTypingAsync();
            Parser.Default.ParseArguments<Options>(Arguments)
            //Should prevent any exceptions from breaking the bot
            .WithParsed(o => _ = GetReleaseTask(o))
            .WithNotParsed(e => _ = Error.SendDiscordError(Context, Value: "Invalid arguments"));
        }

        private async Task GetReleaseTask(Options o)
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("Github"));
            IReadOnlyList<Release> releases;
            Repository repo;
            try
            {
                releases = await client.Repository.Release.GetAll(o.User, o.Repo);
                repo = await client.Repository.Get(o.User, o.Repo);
            }
            catch (ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound) await Error.SendDiscordError(Context, Value: "Repository does not exist.");
                else await Error.SendDiscordError(Context, Value: "Command failed: error reported!", e: e, et: Error.ExceptionType.Fatal);
                return;
            }

            if (releases.Count == 0)
            {
                await Error.SendDiscordError(Context, Value: "No Releases have been made!");
                return;
            }

            if (o.ListTags)
            {
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithColor(Color.Blue);
                embed.WithTitle("Tags");
                embed.Description = string.Empty;
                for (int i = 0; i < releases.Count; i++)
                {
                    Release release = releases[i];
                    embed.Description += $"{release.TagName}\n";
                    if (i == o.Length - 1)
                    {
                        embed.Description += $"{releases.Count - o.Length} more...";
                        break;
                    }
                }
                await ReplyAsync(embed: embed.Build());
                return;
            }

            Release tag;
            if (o.Tag != null)
            {
                try
                {
                    tag = releases.Single(x => x.TagName.ToLower() == o.Tag.ToLower());
                }
                catch
                {
                    await Error.SendDiscordError(Context, Value: "A release with that tag was not found!");
                    return;
                }
            }
            else
            {
                if (o.AllowPrerelease) tag = releases.First();
                else
                {
                    try
                    {
                        tag = releases.First(x => x.Prerelease == o.AllowPrerelease);
                    }
                    catch
                    {
                        await Error.SendDiscordError(Context, Value: "All releases are Pre-releases!");
                        return;
                    }
                }
            }

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(tag.Name);
            builder.WithColor(Color.Blue);
            builder.WithUrl(tag.HtmlUrl);
            if (o.Desc)
            {
                if (tag.Body.Length > EmbedBuilder.MaxDescriptionLength)
                {
                    string[] msgs = Misc.ConvertToDiscordSendable(tag.Body, EmbedBuilder.MaxDescriptionLength);
                    for (int i = 0; i < msgs.Length; i++)
                    {
                        string msg = msgs[i];
                        builder.Description = msg;
                        if (msgs.Length - 1 == i)
                        {
                            builder.WithCurrentTimestamp();

                            foreach (ReleaseAsset asset in tag.Assets)
                            {
                                builder.AddField(asset.Name, $"[Download]({asset.BrowserDownloadUrl})");
                            }
                        }
                        await ReplyAsync(embed: builder.Build());
                        if (i == 0)
                        {
                            builder.Title = null;
                            builder.Url = null;
                        }
                    }
                    return;
                }
                builder.WithDescription(tag.Body);
            }

            foreach (ReleaseAsset asset in tag.Assets)
            {
                builder.AddField(asset.Name, $"[Download]({asset.BrowserDownloadUrl})");
            }
            await ReplyAsync(embed: builder.Build());
            return;
        }
    }
}