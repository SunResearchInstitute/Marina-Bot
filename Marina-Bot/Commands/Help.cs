using Discord;
using Discord.Commands;
using Discord.Net;
using Marina.Commands.Attributes;
using Marina.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marina.Commands.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private static readonly SortedDictionary<string, string> Commands = new();

        static Help()
        {
            Program.Initialize += delegate(object? sender, ServiceProvider services)
            {
                var commands = services.GetService<CommandService>();
                foreach (CommandInfo cmd in commands.Commands)
                {
                    if (cmd.Attributes.Any(a => a is HideCommandAttribute) ||
                        cmd.Preconditions.Any(p => p is RequireTeamOwnerAttributeManual))
                        continue;

                    string str = "";
                    bool hasNoArgs = cmd.Parameters.Count == 0;
                    if (string.IsNullOrWhiteSpace(cmd.Summary))
                    {
                        if (hasNoArgs)
                        {
                            Commands.Add(cmd.Name, "No info available!");
                            continue;
                        }

                        str = "args: ";
                    }
                    else
                    {
                        if (hasNoArgs)
                        {
                            Commands.Add(cmd.Name, cmd.Summary);
                            continue;
                        }

                        str = $"{cmd.Summary}\nargs: ";
                    }

                    foreach (ParameterInfo param in cmd.Parameters)
                    {
                        ManualOptionalParameterAttribute attribute =
                            (ManualOptionalParameterAttribute)param.Attributes.SingleOrDefault(x =>
                               x is ManualOptionalParameterAttribute);
                        if (attribute != null)
                            str += $"[{param.Name} = {attribute.ManualDefaultValue}]";
                        else if (param.DefaultValue != null)
                            str += $"[{param.Name} = {param.DefaultValue}]";
                        else
                            str += $"<{param.Name}> ";
                    }

                    Commands.Add(cmd.Name, str);
                }
            };
        }

        [Command("Help")]
        public async Task GetHelp([Name("Command")] string command = null)
        {
            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.Teal,
                Title = "Help Menu"
            };

            if (command == null)
            {
                foreach (KeyValuePair<string, string> cmd in Commands)
                {
                    builder.AddField(cmd.Key, cmd.Value);
                    //future proofing
                    if (builder.Fields.Count != EmbedBuilder.MaxFieldCount) continue;
                    if (builder.Fields.Count == Commands.Count)
                    {
                        builder.WithCurrentTimestamp();
                        builder.WithFooter("All commands start with 'm.' unless in DMs.");
                        try
                        {
                            await Context.User.SendMessageAsync(embed: builder.Build());
                            await ReplyAsync("Sent help menu to DMs!");
                        }
                        catch (HttpException)
                        {
                            await ReplyAsync("Unable to send DM!");
                            return;
                        }

                        return;
                    }

                    await Context.User.SendMessageAsync(embed: builder.Build());
                    //clear fields
                    builder.Fields = new List<EmbedFieldBuilder>();
                    builder.Title = null;
                }

                builder.WithCurrentTimestamp();
                builder.WithFooter("All commands start with 'm.' unless in DMs.");
                try
                {
                    await Context.User.SendMessageAsync(embed: builder.Build());
                    if (Context.Guild != null)
                        await ReplyAsync("Sent help menu to DMs!");
                }
                catch (HttpException)
                {
                    await ReplyAsync("Unable to send DM!");
                }
            }
            else
            {
                KeyValuePair<string, string> cmd;
                try
                {
                    cmd = Commands.First(c => c.Key.Contains(command, StringComparison.OrdinalIgnoreCase));
                }
                catch (InvalidOperationException)
                {
                    await Error.SendDiscordError(Context, value: "Command does not exist!");
                    return;
                }

                builder.AddField(cmd.Key, cmd.Value);
                await ReplyAsync(embed: builder.Build());
            }
        }
    }
}