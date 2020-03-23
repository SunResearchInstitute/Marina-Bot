using LibSave;
using LibSave.Types;
using Marina.Utils;
using System.Collections.Generic;
using System.Timers;

namespace Marina.Save
{
    public class SaveHandler
    {
        public static Dictionary<string, ISaveFile> Saves = new Dictionary<string, ISaveFile>()
        {
            {"Logs", new DictionarySaveFile<ulong, ulong>("Logs")},
            {"BlackList", new ListSaveFile<ulong>("BlackList")}
        };

        //Easy Accessors 
        public static DictionarySaveFile<ulong, ulong> LogSave => Saves["Logs"] as DictionarySaveFile<ulong, ulong>;
        public static ListSaveFile<ulong> BlacklistSave => Saves["BlackList"] as ListSaveFile<ulong>;

        //30 min. timer
        private static readonly Timer _timer = new Timer(1.8e+6)
        {
            AutoReset = true,
            Enabled = true
        };

        static SaveHandler() => _timer.Elapsed += Timer_Elapsed;

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e) => SaveAll(false);

        public static void SaveAll(bool restartTimer = true)
        {
            if (restartTimer)
                _timer.Reset();

            foreach (ISaveFile save in Saves.Values)
                save.Write();
        }
    }
}
