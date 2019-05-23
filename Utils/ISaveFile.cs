using System.IO;

namespace RK800.Utils
{
    public abstract class ISaveFile
    {
        public abstract void Read();
        public abstract void Write();
        protected FileInfo File;
        protected Stream Open()
        {
            return File.Open(FileMode.Open, FileAccess.ReadWrite);
        }
        public ISaveFile(FileInfo file)
        {
            File = file;
        }
    }
}