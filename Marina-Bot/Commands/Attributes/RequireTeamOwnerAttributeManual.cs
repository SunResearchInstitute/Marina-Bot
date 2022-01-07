using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Marina.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    class RequireTeamOwnerAttributeManual : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            IApplication app = (await context.Client.GetApplicationInfoAsync());
            if (app.Team != null)
            {
                if (!app.Team.TeamMembers.Any(x => x.User.Id == context.User.Id))
                    return PreconditionResult.FromError(ErrorMessage ?? "Command can only be run by the owner of the bot.");
            }
            else
            {
                if (app.Owner.Id != context.User.Id)
                    return PreconditionResult.FromError(ErrorMessage ?? "Command can only be run by the owner of the bot.");
            }
            return PreconditionResult.FromSuccess();
        }
    }
}
