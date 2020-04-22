using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                return File.ReadAllLines(configFile.FullName).Select(line => line.Split('='))
                    .ToDictionary(configItems => configItems[0].ToLower(), configItems => configItems[1]);
            }

            File.WriteAllLines(configFile.FullName, new[]
            {
                "Token={token}"
            });
            Error.SendApplicationError(
                $"Config does not exist, it has been created for you at {configFile.FullName}!", 1);
            return null!;
        }
    }
}