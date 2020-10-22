using LibSave;
using LibSave.Types;
using Marina.Utils;
using System.Collections.Generic;
using System.Timers;

namespace Marina.Save
{
    public static class SaveHandler
    {
        public static DictionarySaveFile<ulong, ulong> LogSave => Program.SaveController.GetSave<DictionarySaveFile<ulong, ulong>>("Logs");
        public static ListSaveFile<ulong> BlacklistSave => Program.SaveController.GetSave<ListSaveFile<ulong>>("Blacklist");
        public static DictionarySaveFile<ulong, List<ulong>> LockdownSave => Program.SaveController.GetSave<DictionarySaveFile<ulong, List<ulong>>>("Lockdowns");

        public static void RegisterSaves(SaveController controller)
        {
            controller.RegisterDictionarySave<ulong, ulong>("Logs");
            controller.RegisterListSave<ulong>("Blacklist");
            controller.RegisterDictionarySave<ulong, List<ulong>>("Lockdowns");
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

            Program.SaveController.SaveAllAsync();
        }
    }
}
