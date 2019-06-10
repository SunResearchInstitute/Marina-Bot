using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace RK800.Utils
{
    //Taken from: https://docs.stillu.cc/api/Discord.Commands.ParameterPreconditionAttribute.html
    public class RequireHierarchyAttribute : ParameterPreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            ParameterInfo parameter, object value, IServiceProvider services)
        {
            // Hierarchy is only available under the socket variant of the user.
            if (!(context.User is SocketGuildUser guildUser))
                return PreconditionResult.FromError("This command cannot be used outside of a guild.");

            SocketGuildUser targetUser;
            switch (value)
            {
                case SocketGuildUser targetGuildUser:
                    targetUser = targetGuildUser;
                    break;
                case ulong userId:
                    targetUser = await context.Guild.GetUserAsync(userId).ConfigureAwait(false) as SocketGuildUser;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (targetUser == null)
                return PreconditionResult.FromError("Target user not found.");

            if (guildUser.Hierarchy <= targetUser.Hierarchy)
                return PreconditionResult.FromError("You cannot target anyone else whose roles are higher than yours.");

            SocketGuildUser currentUser = await context.Guild.GetCurrentUserAsync().ConfigureAwait(false) as SocketGuildUser;
            if (currentUser?.Hierarchy <= targetUser.Hierarchy)
                return PreconditionResult.FromError("The bot's role is lower than the targeted user.");

            return PreconditionResult.FromSuccess();
        }
    }
}
