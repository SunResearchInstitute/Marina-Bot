using Marina.Utils;
using System;
using System.IO;

namespace Marina.Save
{
    public abstract class ISaveFile
    {
        public static DirectoryInfo SaveDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetDirectory("Save");

        static ISaveFile()
        {
            if (!SaveDirectory.Exists)
                SaveDirectory.Create();
        }

        public FileInfo FileInfo;
        public abstract void Write();
    }
}
