using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Marina.Utils
{
    public static class Fs
    {
        public static void AppendAllText(this FileInfo obj, string contents) =>
            File.AppendAllText(obj.FullName, contents);

        public static void AppendAllText(this FileInfo obj, string contents, Encoding encoding) =>
            File.AppendAllText(obj.FullName, contents, encoding);

        public static Task AppendAllTextAsync(this FileInfo obj, string contents,
            CancellationToken cancellationToken = default) =>
            File.AppendAllTextAsync(obj.FullName, contents, cancellationToken);
    }
}