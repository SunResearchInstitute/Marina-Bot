using System.IO;

namespace RK800.Save
{
    public abstract class ISaveFile
    {
        public abstract void Read();
        public abstract void Write();
        protected FileInfo File;
        protected Stream Open() => File.Open(FileMode.Open, FileAccess.ReadWrite);
        
        public ISaveFile(FileInfo file) => File = file;
        
    }
}