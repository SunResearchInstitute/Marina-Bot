﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Marina.Interactions.Attributes
{
    public class RequireHierarchyAttribute : ParameterPreconditionAttribute
    {
        //taken from: https://discordnet.dev/api/Discord.Commands.ParameterPreconditionAttribute.html
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, IParameterInfo parameterInfo, object value, IServiceProvider services)
        {
            if (context.User is not SocketGuildUser guildUser)
                return PreconditionResult.FromError("This command cannot be used outside of a guild.");
            SocketGuildUser targetUser = value switch
            {
                SocketGuildUser targetGuildUser => targetGuildUser,
                ulong userId => await context.Guild.GetUserAsync(userId).ConfigureAwait(false) as SocketGuildUser,
                _ => throw new ArgumentOutOfRangeException(),
            };
            if (targetUser == null)
                return PreconditionResult.FromError("Target user not found.");

            if (guildUser.Hierarchy < targetUser.Hierarchy)
                return PreconditionResult.FromError("You cannot target anyone else whose roles are higher than yours.");

            var currentUser = await context.Guild.GetCurrentUserAsync().ConfigureAwait(false) as SocketGuildUser;
            if (currentUser?.Hierarchy < targetUser.Hierarchy)
                return PreconditionResult.FromError("The bot's role is lower than the targeted user.");

            return PreconditionResult.FromSuccess();
        }
    }
}