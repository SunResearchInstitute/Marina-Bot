using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace RK800.Utils
{
    public static class FS
    {
        public static FileInfo GetFile(this DirectoryInfo obj, string filename) => new FileInfo($"{obj.FullName}{Path.DirectorySeparatorChar}{filename}");

        public static DirectoryInfo GetDirectory(this DirectoryInfo obj, string foldername) => new DirectoryInfo($"{obj.FullName}{Path.DirectorySeparatorChar}{foldername.Replace('/', Path.DirectorySeparatorChar)}");

        public static long GetSize(this DirectoryInfo obj)
        {
            IEnumerable<FileInfo> files = obj.EnumerateFilesRecursively();
            return files.Sum((f) => f.Length);
        }

        public static void DeleteContents(this DirectoryInfo obj)
        {
            IEnumerable<DirectoryInfo> Directories = obj.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
            IEnumerable<FileInfo> Files = obj.EnumerateFiles("*", SearchOption.TopDirectoryOnly);

            foreach (DirectoryInfo directory in Directories)
                directory.Delete(true);

            foreach (FileInfo file in Files)
                file.Delete();
        }

        public static IEnumerable<FileInfo> EnumerateFilesRecursively(this DirectoryInfo obj) => obj.EnumerateFiles("*", SearchOption.AllDirectories);
    }
}
