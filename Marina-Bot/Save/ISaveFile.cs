using System.IO;

namespace Marina.Save
{
    public abstract class ISaveFile
    {
        public static DirectoryInfo SaveDirectory = new DirectoryInfo("Save");

        public FileInfo FileInfo;

        static ISaveFile()
        {
            if (!SaveDirectory.Exists)
                SaveDirectory.Create();
        }

        public abstract void Write();

        public abstract void CleanUp(ulong id);
    }
}
