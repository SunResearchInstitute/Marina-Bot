using System.Collections.Generic;
using System.Timers;
using LibSave;
using LibSave.Types;
using Marina.Utils;

namespace Marina.Save
{
    public class SaveHandler
    {
        public static readonly Dictionary<string, ISaveFile> Saves = new Dictionary<string, ISaveFile>
        {
            {"Logs", new DictionarySaveFile<ulong, ulong>("Logs")},
            {"BlackList", new ListSaveFile<ulong>("BlackList")}
        };

        //30 min. timer
        private static readonly Timer Timer = new Timer(1.8e+6)
        {
            AutoReset = true,
            Enabled = true
        };

        static SaveHandler()
        {
            Timer.Elapsed += delegate { SaveAll(false); };
        }

        //Easy Accessors 
        public static DictionarySaveFile<ulong, ulong> LogSave => Saves["Logs"] as DictionarySaveFile<ulong, ulong>;
        public static ListSaveFile<ulong> BlacklistSave => Saves["BlackList"] as ListSaveFile<ulong>;

        public static void SaveAll(bool restartTimer = true)
        {
            if (restartTimer)
                Timer.Reset();

            foreach (ISaveFile save in Saves.Values)
                save.Write();
        }
    }
}