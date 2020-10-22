using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Marina.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    class RequireTeamOwnerAttributeManual : PreconditionAttribute
    {
        public ulong[] OwnerIds = new ulong[]{
            130825292292816897,
            223802102684581889,
            125486996750729216
        };

        public override string ErrorMessage { get; set; }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return await Task.Run(() =>
            {
                if (!OwnerIds.Contains(context.User.Id))
                    return PreconditionResult.FromError(ErrorMessage ?? "Command can only be run by the owner of the bot.");

                return PreconditionResult.FromSuccess();
            });
        }
    }
}
