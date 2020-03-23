using System;
using System.IO;
using System.Threading.Tasks;

namespace Marina.Utils
{
    public static class Console
    {
        public static readonly FileInfo LogFile = new FileInfo("Marina.log");

        public static async Task WriteLog(string str)
        {
            System.Console.WriteLine(str);
            await LogFile.AppendAllTextAsync($"[{DateTime.Now}]: {str}\n");
        }
    }
}
