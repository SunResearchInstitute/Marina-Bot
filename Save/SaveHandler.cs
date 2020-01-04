using Marina.Save.Types;
using System.Collections.Generic;

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

        public static void SaveAll()
        {
            foreach (ISaveFile save in Saves.Values)
                save.Write();
        }
    }
}
