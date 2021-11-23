using LibSave;
using LibSave.Types;
using Marina.Save.Types;
using Marina.Utils;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace Marina.Save
{
    public static class SaveHandler
    {
        public static SaveController SaveController { get; private set; } = new SaveController("Save");
        public static DictionarySaveFile<ulong, ulong> LogSave => SaveController.GetSave<DictionarySaveFile<ulong, ulong>>("Logs");
        public static ListSaveFile<ulong> BlacklistSave => SaveController.GetSave<ListSaveFile<ulong>>("Blacklist");
        public static DictionarySaveFile<ulong, List<ulong>> LockdownSave => SaveController.GetSave<DictionarySaveFile<ulong, List<ulong>>>("Lockdowns");
        public static ConfigFile Config => SaveController.GetSave<ConfigFile>("Config");
        public static ListSaveFile<ulong> Owners => SaveController.GetSave<ListSaveFile<ulong>>("Owners");

        public static void RegisterSaves()
        {
            SaveController.RegisterDictionarySave<ulong, ulong>("Logs");
            SaveController.RegisterListSave<ulong>("Blacklist");
            SaveController.RegisterDictionarySave<ulong, List<ulong>>("Lockdowns");
            FileInfo f = SaveController.GetSavePath("Config");
            SaveController.RegisterCustomSave("Config", new ConfigFile(f));
            SaveController.RegisterListSave("Owners", new List<ulong>() {
            130825292292816897,
            223802102684581889,
            125486996750729216,
            128656451236397056
            });
            if (!f.Exists)
            {
                Error.SendApplicationError($"Config does not exist, it has been created for you at {f.FullName}!", 1);
            }
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
