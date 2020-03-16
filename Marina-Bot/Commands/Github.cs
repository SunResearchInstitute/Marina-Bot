using CommandLine;
using Discord;
using Discord.Commands;
using Marina.Utils;
using Octokit;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Error = Marina.Utils.Error;

namespace Marina.Commands
{
    public class Github : ModuleBase<SocketCommandContext>
    {
        public class Options
        {
            [Option('t', "tag", Required = false)]
            public string Tag { get; set; }

            [Option('l', "tags", Required = false, Default = false)]
            public bool ListTags { get; set; }

            [Option('m', "maxtagdisplaylength", Required = false, Default = 12)]
            public int Length { get; set; }

            [Option('p', "prerelease", Required = false, Default = false)]
            public bool AllowPrerelease { get; set; }

            [Value(0, Required = true)]
            public string User { get; set; }

            [Value(1, Required = true)]
            public string RepositoryName { get; set; }

            [Option('d', "description", Required = false, Default = false)]
            public bool GetDescription { get; set; }
        }

        [Command("GitRelease")]
        [Summary("Gets a release from the specificed Github repository.\nUser and Repository must be included anywhere in the command in that order.\nAvaliable options:\n--tag, -t=string (default: null)\n--description, -d=bool (default: false)\n--prerelease, -p=bool (default: false)\n--tags, -l=bool (default: false)\n--maxtagdisplaylength, -n=int (defualt: 12)")]
        public Task GetRelease(params string[] Arguments)
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("Marina-Bot"));

            Dictionary<string, string> config = Misc.LoadConfig();
            if (config.ContainsKey("gitkey"))
                client.Credentials = new Credentials(config["gitkey"]);

            Parser parser = new Parser(config =>
            {
                config.HelpWriter = null;
                config.AutoHelp = false;
            });
            parser.ParseArguments<Options>(Arguments)
            .WithParsed(async o => await GetReleaseTask(client, o))
            .WithNotParsed(async e => await Error.SendDiscordError(Context, Value: "Invalid arguments"));
            parser.Dispose();
            return Task.CompletedTask;
        }

        private async Task GetReleaseTask(GitHubClient client, Options options)
        {
            IReadOnlyList<Release> releases;
            Repository repo;
            try
            {
                repo = await client.Repository.Get(options.User, options.RepositoryName);
                releases = await client.Repository.Release.GetAll(options.User, options.RepositoryName);
            }
            catch (ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                    await Error.SendDiscordError(Context, Value: "Repository does not exist.");
                else
                    await Error.SendDiscordError(Context, Value: "Command failed: error reported!", e: e);
                return;
            }

            if (releases.Count == 0)
            {
                await Error.SendDiscordError(Context, Value: "No Releases have been made!");
                return;
            }

            if (options.ListTags)
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Color = Color.Teal,
                    Title = "Tags",
                };
                for (int i = 0; i < releases.Count; i++)
                {
                    embed.Description += $"{releases[i].TagName}\n";
                    if (i == options.Length - 1)
                    {
                        embed.Description += $"{releases.Count - options.Length} more...";
                        break;
                    }
                }
                await ReplyAsync(embed: embed.Build());
                return;
            }

            Release tag;
            if (options.Tag != null)
            {
                try
                {
                    tag = releases.Single(x => x.TagName.ToLower() == options.Tag.ToLower());
                }
                catch
                {
                    await Error.SendDiscordError(Context, Value: "A release with that tag was not found!");
                    return;
                }
            }
            else
            {
                if (!options.AllowPrerelease)
                {
                    try
                    {
                        tag = releases.First(x => x.Prerelease == options.AllowPrerelease);
                    }
                    catch
                    {
                        await Error.SendDiscordError(Context, Value: "All releases are Pre-releases!");
                        return;
                    }
                }
                else
                    tag = releases.First();
            }

            EmbedBuilder builder = new EmbedBuilder
            {
                Title = tag.Name,
                Color = Color.Teal,
                Url = tag.HtmlUrl
            };
            if (options.GetDescription)
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
                builder.Description = tag.Body;
            }

            foreach (ReleaseAsset asset in tag.Assets)
                builder.AddField(asset.Name, $"[Download]({asset.BrowserDownloadUrl})");

            await ReplyAsync(embed: builder.Build());
            return;
        }
    }
}