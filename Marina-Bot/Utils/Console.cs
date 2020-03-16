using System;
using System.Threading.Tasks;

namespace Marina.Utils
{
    public static class Console
    {
        public static async Task WriteLog(string str)
        {
            System.Console.WriteLine(str);
            await Program.LogFile.AppendAllTextAsync($"[{DateTime.Now}]: {str}\n");
        }
    }
}
