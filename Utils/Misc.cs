using System;
using System.Collections.Generic;
using System.Timers;

namespace Marina.Utils
{
    public static class Misc
    {
        public static string ConvertToReadableSize(double size)
        {
            int unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            return string.Format("{0:0.#} {1}", size, units[unit]);
        }

        public static string[] ConvertToDiscordSendable(string s, int size = 2000)
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
    }
}
