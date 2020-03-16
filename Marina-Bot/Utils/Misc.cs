using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace Marina.Utils
{
    public static class Misc
    {
        public static string[] ConvertToDiscordSendable(string s, int size = 2048)
        {
            List<string> readable = new List<string>();
            for (int i = 0; i < s.Length; i += size)
            {
                int length = Math.Min(s.Length - i, size);
                readable.Add(s.Substring(i, length));
            }
            return readable.ToArray();
        }

        public static void Reset(this Timer obj)
        {
            obj.Stop();
            obj.Start();
        }

        public static Dictionary<string, string> LoadConfig()
        {
            FileInfo configFile = new FileInfo("Config.txt");
            if (configFile.Exists)
            {
                Dictionary<string, string> config = new Dictionary<string, string>();
                foreach (string line in File.ReadAllLines(configFile.FullName))
                {
                    //Even though we do not verify any Config items we should be fine
                    string[] configitems = line.Split('=');
                    config.Add(configitems[0].ToLower(), configitems[1]);
                }
                return config;
            }
            else
            {
                File.WriteAllLines(configFile.FullName, new string[]
                {
                    "Token={token}"
                });
                Error.SendApplicationError($"Config does not exist, it has been created for you at {configFile.FullName}!", 1);
                return null;
            }
        }
    }
}
