using Marina.Utils;
using System;
using System.IO;

namespace Marina.Save
{
    public abstract class ISaveFile
    {
        public static DirectoryInfo SaveDirectory = new DirectoryInfo("Save");

        static ISaveFile()
        {
            if (!SaveDirectory.Exists)
                SaveDirectory.Create();
        }

        public FileInfo FileInfo;
        public abstract void Write();
    }
}
