using Discord;
using Discord.Commands;
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
        private readonly InteractionService _interacts;
        public readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, InteractionService interacts, CommandService commands, IServiceProvider services)
        {
            _client = client;
            _interacts = interacts;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _interacts.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results 
            _interacts.SlashCommandExecuted += SlashCommandExecuted;
            _interacts.ContextCommandExecuted += ContextCommandExecuted;
            _interacts.ComponentCommandExecuted += ComponentCommandExecuted;

            _interacts.Log += async delegate (LogMessage log)
            {
                if (log.Exception != null && log.Exception is CommandException exception && log.Exception.InnerException != null)
                    await Error.SendDiscordError((SocketCommandContext)exception.Context, value: "A Fatal error has occured. This has been reported.", e: log.Exception.InnerException);
                else
                    await Utils.Console.WriteLog($"[{DateTime.Now}]: {log.ToString()}\n");
            };
            _commands.Log += async delegate (LogMessage log)
            {
                if (log.Exception != null && log.Exception is CommandException exception && log.Exception.InnerException != null)
                    await Error.SendDiscordError((SocketCommandContext)exception.Context, value: "A Fatal error has occured. This has been reported.", e: log.Exception.InnerException);
                else
                    await Utils.Console.WriteLog($"[{DateTime.Now}]: {log.ToString()}\n");
            };

            _client.MessageReceived += MessageReceived;
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            //Welcomes are considered a message and are null
            if (arg is not SocketUserMessage message)
                return;

            SocketCommandContext context = new(_client, message);
            int prefixPos = 0;

            if (string.IsNullOrWhiteSpace(context.Message.Content) || context.User.IsBot)
                return;

            if (context.Guild != null)
            {
                if (IsDebug())
                {
                    if (!message.HasStringPrefix("d.", ref prefixPos))
                        return;
                }
                else
                {
                    if (!message.HasStringPrefix("m.", ref prefixPos))
                        return;
                }
            }


            await context.Channel.TriggerTypingAsync();
            Discord.Commands.IResult result = await _commands.ExecuteAsync(context, prefixPos, null);
            if (!result.IsSuccess)
                await Error.SendDiscordError(context, result.ErrorReason);
            else
                await Utils.Console.WriteLog($"{context.User} ({context.User.Id}) executed command: {context.Message}");
        }

        private async Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                await Error.SendDiscordError(arg2, arg3.ErrorReason);
            }
        }

        private async Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                await Error.SendDiscordError(arg2, arg3.ErrorReason);
            }
        }

        private async Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
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
                await _interacts.ExecuteCommandAsync(ctx, _services);
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

        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
                return false;
#endif
        }
    }
}