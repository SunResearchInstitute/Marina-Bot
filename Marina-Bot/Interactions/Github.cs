using Discord;
using Discord.Interactions;
using Marina.Utils;
using Octokit;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Error = Marina.Utils.Error;

namespace Marina.Interactions
{
    public class Github : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("gitrelease", "Gets a release from the specified Github repository.")]
        public async Task GetRelease(string user, string repositoryName, string tag = "latest", bool getDescription = false, bool getPrerelease = false, bool listTags = false,int maxTagDisplayLength = 12)
        {
            GitHubClient client = new(new ProductHeaderValue("Marina-Bot"));

            string key = Save.SaveHandler.Config.Data.GitKey;
            if (string.IsNullOrEmpty("key"))
                client.Credentials = new Credentials(key);
            await GetReleaseTask(client, user, repositoryName, tag, getDescription, getPrerelease, listTags, maxTagDisplayLength);
        }

        private async Task GetReleaseTask(IGitHubClient client, string user, string repositoryName, string tag, bool getDescription, bool getPrerelease, bool listTags, int maxTagDisplayLength)
        {
            IReadOnlyList<Release> releases;
            try
            {
                releases = await client.Repository.Release.GetAll(user, repositoryName);
            }
            catch (ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                    await Error.SendDiscordError(Context, value: "Repository does not exist.");
                else
                    await Error.SendDiscordError(Context, value: "Command failed: error reported!", e: e);
                return;
            }

            if (releases.Count == 0)
            {
                await Error.SendDiscordError(Context, value: "No Releases have been made!");
                return;
            }

            if (listTags)
            {
                EmbedBuilder embed = new()
                {
                    Color = Color.Teal,
                    Title = "Tags"
                };
                for (int i = 0; i < releases.Count; i++)
                {
                    embed.Description += $"{releases[i].TagName}\n";
                    if (i != maxTagDisplayLength - 1) continue;

                    embed.Description += $"{releases.Count - maxTagDisplayLength} more...";
                }

                await RespondAsync(embed: embed.Build());
                return;
            }

            Release gTag;
            if (tag != "latest")
            {
                try
                {
                    gTag = releases.Single(x => x.TagName.ToLower() == tag.ToLower());
                }
                catch
                {
                    await Error.SendDiscordError(Context, value: "A release with that tag was not found!");
                    return;
                }
            }
            else
            {
                if (!getPrerelease)
                    try
                    {
                        gTag = releases.First(x => x.Prerelease == getPrerelease);
                    }
                    catch
                    {
                        await Error.SendDiscordError(Context, value: "All releases are Pre-releases!");
                        return;
                    }
                else
                    gTag = releases.First();
            }

            EmbedBuilder builder = new EmbedBuilder
            {
                Title = gTag.Name,
                Color = Color.Teal,
                Url = gTag.HtmlUrl
            };
            if (getDescription)
            {
                if (gTag.Body.Length > EmbedBuilder.MaxDescriptionLength)
                {
                    string[] msgs = Misc.ConvertToDiscordSendable(gTag.Body);
                    for (int i = 0; i < msgs.Length; i++)
                    {
                        string msg = msgs[i];
                        builder.Description = msg;
                        if (msgs.Length - 1 == i)
                        {
                            builder.WithCurrentTimestamp();

                            foreach (ReleaseAsset asset in gTag.Assets)
                                builder.AddField(asset.Name, $"[Download]({asset.BrowserDownloadUrl})");
                        }

                        await ReplyAsync(embed: builder.Build());
                        if (i != 0) continue;
                        builder.Title = null;
                        builder.Url = null;
                    }

                    return;
                }

                builder.Description = gTag.Body;
            }

            foreach (ReleaseAsset asset in gTag.Assets)
                builder.AddField(asset.Name, $"[Download]({asset.BrowserDownloadUrl})");

            await RespondAsync(embed: builder.Build());
        }
    }
}