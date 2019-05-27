using Discord.Commands;
using System;
using System.Threading.Tasks;
using RK800;
using Discord;
using System.Collections.Generic;

namespace RK800.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private static Dictionary<string, string> cmds = new Dictionary<string, string>();

        public static void Populate()
        {
            foreach (CommandInfo cmd in Program.Commands.Commands)
            {
                string s = "";
                if (string.IsNullOrWhiteSpace(cmd.Summary) && !cmd.HasVarArgs)
                {
                    cmds.Add(cmd.Name, "No info avaliable!");
                    continue;
                }
                else if (!string.IsNullOrWhiteSpace(cmd.Summary) && !cmd.HasVarArgs)
                {
                    cmds.Add(cmd.Name, cmd.Summary);
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(cmd.Summary) && cmd.HasVarArgs) s = "args: ";
                else if (!string.IsNullOrWhiteSpace(cmd.Summary) && cmd.HasVarArgs) s = $"{cmd.Summary}\nargs: ";

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
            builder.WithFooter("All commands start with 'c.'");
            builder.WithTitle("Help Menu");
            foreach (KeyValuePair<string, string> cmd in cmds)
            {
                builder.AddField(cmd.Key, cmd.Value);
            }
            await ReplyAsync(embed: builder.Build());
        }
    }
}