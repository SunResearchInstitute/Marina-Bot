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
            {"BlackList", new ListSaveFile<ulong>("BlackList")},
            {"Suggestions", new DictionarySaveFile<ulong, List<string>>("Suggestions")}
        };

        //Easy Accessors 
        public static DictionarySaveFile<ulong, ulong> LogSave => Saves["Logs"] as DictionarySaveFile<ulong, ulong>;
        public static ListSaveFile<ulong> BlacklistSave => Saves["BlackList"] as ListSaveFile<ulong>;
        public static DictionarySaveFile<ulong, List<string>> SuggestionsSave => Saves["Suggestions"] as DictionarySaveFile<ulong, List<string>>;

        //15 min. timer
        private static readonly Timer _timer = new Timer(900000)
        {
            AutoReset = true,
            Enabled = true
        };

        static SaveHandler() => _timer.Elapsed += Timer_Elapsed;

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e) => SaveAll();

        public static void SaveAll()
        {
            _timer.Reset();
            foreach (ISaveFile save in Saves.Values)
                save.Write();
        }
    }
}
