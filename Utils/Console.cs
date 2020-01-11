using System;

namespace Marina.Utils
{
    public static class Console
    {
        public static void ConsoleWriteLog(string str)
        {
            System.Console.WriteLine(str);
            Program.LogFile.AppendAllText($"[{DateTime.Now}]: {str}\n");
        }
    }
}
