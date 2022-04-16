using Discord.Interactions;
using Marina.Interactions.Attributes;
using Marina.Utils;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Marina.Interactions
{
    public class Twitch : InteractionModuleBase<SocketInteractionContext>
    {
        [RequireLivingRoomServer]
        [SlashCommand("shipclip", "send a twitch clip to NoobHunter")]
        public async Task ShipClip(string clip)
        {
            if (Save.SaveHandler.ClipsShipped.Contains(clip))
            {
                await Error.SendDiscordError(Context, value: "That clip has already been shipped!");
                return;
            }

            GoogleFormsSubmissionService srv = new("https://docs.google.com/forms/d/1rmOvywHZrlXZDzqHtawhSLQEy3fWkY0DhkXlGJ8g6Oo/formResponse");
            srv.SetFieldValues(new Dictionary<string, string>()
            {
                {"entry.1503696995", clip},
                {"entry.1754633046", "gabehxd" }
            });

            HttpResponseMessage rc = await srv.SubmitAsync();
            if (rc.IsSuccessStatusCode)
            {
                await RespondAsync("Clip was shipped! :thumbsup:");
                Save.SaveHandler.ClipsShipped.Add(clip);
            }
            else
            {
                await Error.SendDiscordError(Context, value: $"Clip was not shipped: {rc.StatusCode}");
            }
        }
    }
}
