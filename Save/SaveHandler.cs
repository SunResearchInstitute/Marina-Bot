using RK800.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace RK800.Save
{
    class SaveHandler
    {
        public static readonly DirectoryInfo save = new DirectoryInfo("save");

        public static Dictionary<string, ISaveFile> Saves = new Dictionary<string, ISaveFile>();

        private static readonly string[] PreDefinedSaves = { "Trackers.Tracker", "Filter.UlongStringList", "Warns.Warn" };

        private static ISaveFile OpenSaveFile(FileInfo file)
        {
            switch (file.Extension.ToLower())
            {
                case ".ulong":
                    return new UlongSaveFile(file);
                case ".ulongstring":
                    return new UlongStringSaveFile(file);
                case ".tracker":
                    return new TrackerSaveFile(file);
                case ".ulongstringlist":
                    return new FilterSaveFile(file);
                case ".warn":
                    return new WarnSaveFile(file);
                default:
                    throw new Exception("File not a save!");
            }
        }

        public static void Populate()
        {
            save.Create();
            foreach (string str in PreDefinedSaves)
            {
                FileInfo info = save.GetFile(str);
                if (!info.Exists)
                    info.Create().Close();
            }

            foreach (FileInfo file in save.EnumerateFiles())
                Saves[Path.GetFileNameWithoutExtension(file.FullName)] = OpenSaveFile(file);

            foreach (ISaveFile file in Saves.Values)
                file.Read();
        }

        public static void SaveAll()
        {
            foreach (ISaveFile save in Saves.Values)
                save.Write();
        }
    }
}
