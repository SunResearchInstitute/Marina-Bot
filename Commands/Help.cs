using Discord.Commands;
using System;
using System.Threading.Tasks;
using RK800;
using Discord;
using System.Collections.Generic;
using System.Linq;
using RK800.Utils;

namespace RK800.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private static Dictionary<string, string> cmds = new Dictionary<string, string>();

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
                        cmds.Add(cmd.Name, "No info available!");
                        continue;
                    }
                    else s = "args: ";
                }
                else
                {
                    if (HasNoArgs)
                    {
                        cmds.Add(cmd.Name, cmd.Summary);
                        continue;
                    }
                    else s = $"{cmd.Summary}\nargs: ";
                }

                foreach (ParameterInfo param in cmd.Parameters)
                {
                    if (param.DefaultValue != null) s += $"[{param.Name.Replace('_', ' ')} = {param.DefaultValue}] ";
                    else s += $"<{param.Name.Replace('_', ' ')}> ";
                }
                cmds.Add(cmd.Name, s);
            }
        }

        [Command("Help")]
        public async Task GetHelp(string Command = null)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.Blue);
            builder.WithCurrentTimestamp();
            builder.WithFooter("All commands start with 'c.' unless in DMs.");
            builder.WithTitle("Help Menu");

            if (Command != null)
            {
                foreach (KeyValuePair<string, string> cmd in cmds)
                {
                    if (cmd.Key.Contains(Command))
                    {
                        builder.AddField(cmd.Key, cmd.Value);
                        await ReplyAsync(embed: builder.Build());
                        return;
                    }
                }
                await Error.SendDiscordError(Context, Value: "Command does not exist!");
            }
            else
            {
                foreach (KeyValuePair<string, string> cmd in cmds)
                {
                    builder.AddField(cmd.Key, cmd.Value);
                }
                await Context.User.SendMessageAsync(embed: builder.Build());
            }
        }
    }
}