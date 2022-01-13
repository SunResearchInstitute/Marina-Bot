using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Marina.Utils;
using System;
using System.Reflection;
using System.Threading.Tasks;
using IResult = Discord.Interactions.IResult;

namespace Marina
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, InteractionService interacts, IServiceProvider services)
        {
            _client = client;
            _interactions = interacts;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results 
            _interactions.SlashCommandExecuted += SlashCommandExecuted;
            _interactions.ContextCommandExecuted += ContextCommandExecuted;
            _interactions.ComponentCommandExecuted += ComponentCommandExecuted;

            _interactions.Log += async delegate (LogMessage log)
            {
                await Utils.Console.WriteLog($"[{DateTime.Now}]: {log.ToString()}\n");
            };
        }

        private async Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                await Error.SendDiscordError(arg2, arg3.ErrorReason);
            }
        }

        private async Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                await Error.SendDiscordError(arg2, arg3.ErrorReason);
            }
        }

        private async Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                await Error.SendDiscordError(arg2, arg3.ErrorReason);
            }
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new SocketInteractionContext(_client, arg);
                await _interactions.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                await Utils.Console.WriteLog(ex.ToString());

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}