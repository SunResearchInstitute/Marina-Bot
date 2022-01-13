using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Marina.Utils
{
    public static class Console
    {
        private static readonly FileInfo LogFile = new("Marina.log");

        static Console()
        {
            Program.Initialize += delegate (object? sender, ServiceProvider services)
            {
                services.GetService<DiscordSocketClient>().Log += async delegate (LogMessage log)
                {
                    await WriteLog($"[{DateTime.Now}]: {log.ToString()}\n");
                };
            };
        }

        public static async Task WriteLog(string str)
        {
            System.Console.WriteLine(str);
            await LogFile.AppendAllTextAsync($"[{DateTime.Now}]: {str}\n");
        }
    }
}