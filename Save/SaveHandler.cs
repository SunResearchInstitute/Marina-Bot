using Marina.Save.Types;
using System.Collections.Generic;
using System.Timers;

namespace Marina.Save
{
    public class SaveHandler
    {
        public static Dictionary<string, ISaveFile> Saves = new Dictionary<string, ISaveFile>()
        {
            {"Logs", new UlongUlongSave("Logs")}
        };

        //Easy Accessors 
        public static UlongUlongSave LogSaveFile => Saves["Logs"] as UlongUlongSave;

        //15 min. timer
        private static readonly Timer _timer = new Timer(900000)
        {
            AutoReset = true
        };
        static SaveHandler() => _timer.Elapsed += Timer_Elapsed;

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e) => SaveAll();

        public static void SaveAll()
        {
            foreach (ISaveFile save in Saves.Values)
                save.Write();
        }
    }
}
