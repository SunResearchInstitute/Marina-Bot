using LibSave;
using LibSave.Types;
using Marina.Utils;
using System.Collections.Generic;
using System.Timers;

namespace Marina.Save
{
    public static class SaveHandler
    {
        public static SaveController SaveController { get; private set; } = new SaveController("Save");
        public static DictionarySaveFile<ulong, ulong> LogSave => SaveController.GetSave<DictionarySaveFile<ulong, ulong>>("Logs");
        public static ListSaveFile<ulong> BlacklistSave => SaveController.GetSave<ListSaveFile<ulong>>("Blacklist");
        public static DictionarySaveFile<ulong, List<ulong>> LockdownSave => SaveController.GetSave<DictionarySaveFile<ulong, List<ulong>>>("Lockdowns");
        public static DictionarySaveFile<string, string> Config => SaveController.GetSave<DictionarySaveFile<string, string>>("Config");

        public static void RegisterSaves()
        {
            SaveController.RegisterDictionarySave<ulong, ulong>("Logs");
            SaveController.RegisterListSave<ulong>("Blacklist");
            SaveController.RegisterDictionarySave<ulong, List<ulong>>("Lockdowns");
            SaveController.RegisterDictionarySave("Config", new Dictionary<string, string> {
                {"token","{token}" }
            });
        }

        private static readonly Timer Timer = new Timer(1.8e+6)
        {
            AutoReset = true,
            Enabled = true
        };

        static SaveHandler()
        {
            Timer.Elapsed += delegate { SaveAll(false); };
        }

        public static void SaveAll(bool restartTimer = true)
        {
            if (restartTimer)
                Timer.Reset();

            SaveController.SaveAllAsync();
        }
    }
}
