using Discord;
using Discord.Commands;
using Discord.Net;
using Marina.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marina.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private static readonly SortedDictionary<string, string> _commands = new SortedDictionary<string, string>();

        public static void Populate()
        {
            foreach (CommandInfo cmd in Program.Commands.Commands)
            {
                if (cmd.Preconditions.Any(p => p is RequireOwnerAttribute)) continue;

                string s = "";
                bool HasNoArgs = cmd.Parameters.Count == 0;
                if (string.IsNullOrWhiteSpace(cmd.Summary))
                {
                    if (HasNoArgs)
                    {
                        _commands.Add(cmd.Name, "No info available!");
                        continue;
                    }
                    else s = "args: ";
                }
                else
                {
                    if (HasNoArgs)
                    {
                        _commands.Add(cmd.Name, cmd.Summary);
                        continue;
                    }
                    else s = $"{cmd.Summary}\nargs: ";
                }

                foreach (ParameterInfo param in cmd.Parameters)
                {
                    if (param.DefaultValue != null) s += $"[{param.Name.Replace('_', ' ')} = {param.DefaultValue}] ";
                    else s += $"<{param.Name.Replace('_', ' ')}> ";
                }
                _commands.Add(cmd.Name, s);
            }
        }

        [Command("Help")]
        public async Task GetHelp(string Command = null)
        {
            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.Teal,
                Title = "Help Menu"
            };

            if (Command == null)
            {
                foreach (KeyValuePair<string, string> cmd in _commands)
                {
                    builder.AddField(cmd.Key, cmd.Value);
                    //future proofing
                    if (builder.Fields.Count == EmbedBuilder.MaxFieldCount)
                    {
                        if (builder.Fields.Count == _commands.Count)
                        {
                            builder.WithCurrentTimestamp();
                            builder.WithFooter("All commands start with 'm.' unless in DMs.");
                            try
                            {
                                await Context.User.SendMessageAsync(embed: builder.Build());
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
                }
                builder.WithCurrentTimestamp();
                builder.WithFooter("All commands start with 'm.' unless in DMs.");
                try
                {
                    await Context.User.SendMessageAsync(embed: builder.Build());
                }
                catch (HttpException)
                {
                    await ReplyAsync("Unable to send DM!");
                    return;
                }
            }
            else
            {
                KeyValuePair<string, string> cmd;
                try
                {
                    cmd = _commands.First(c => c.Key.Contains(Command, StringComparison.OrdinalIgnoreCase));
                }
                catch (InvalidOperationException)
                {
                    await Error.SendDiscordError(Context, Value: "Command does not exist!");
                    return;
                }
                builder.AddField(cmd.Key, cmd.Value);
                await ReplyAsync(embed: builder.Build());
            }
        }
    }
}
