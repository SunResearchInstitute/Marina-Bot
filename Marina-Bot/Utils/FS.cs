using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Marina.Utils
{
    public static class FS
    {
        public static FileInfo GetFile(this DirectoryInfo obj, string filename) => new FileInfo($"{obj.FullName}{Path.DirectorySeparatorChar}{filename}");

        public static DirectoryInfo GetDirectory(this DirectoryInfo obj, string foldername) => new DirectoryInfo($"{obj.FullName}{Path.DirectorySeparatorChar}{foldername.Replace('/', Path.DirectorySeparatorChar)}");

        public static IEnumerable<FileInfo> EnumerateFilesRecursively(this DirectoryInfo obj) => obj.EnumerateFiles("*", SearchOption.AllDirectories);

        public static void AppendAllText(this FileInfo obj, string contents) => File.AppendAllText(obj.FullName, contents);
        public static void AppendAllText(this FileInfo obj, string contents, Encoding encoding) => File.AppendAllText(obj.FullName, contents, encoding);
        public static Task AppendAllTextAsync(this FileInfo obj, string contents, CancellationToken cancellationToken = default) => File.AppendAllTextAsync(obj.FullName, contents, cancellationToken);

        public static string ReadAllText(this FileInfo obj) => File.ReadAllText(obj.FullName);
        public static string ReadAllText(this FileInfo obj, Encoding encoding) => File.ReadAllText(obj.FullName, encoding);

        public static void WriteAllText(this FileInfo obj, string contents) => File.WriteAllText(obj.FullName, contents);
        public static void WriteAllText(this FileInfo obj, string contents, Encoding encoding) => File.WriteAllText(obj.FullName, contents, encoding);
    }
}
