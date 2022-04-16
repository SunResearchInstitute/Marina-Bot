using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace Marina.Interactions.Attributes
{
    public class RequireLivingRoomServerAttribute : PreconditionAttribute
    {
        private readonly ulong livingroomId = 691848892463054908;
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.Guild.Id != livingroomId)
            {
                return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? "This command is not avaliable in this guild."));
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
    }
}
