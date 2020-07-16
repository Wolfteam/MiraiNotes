using System;
using System.IO;
using System.Linq;

namespace MiraiNotes.Shared.Utils
{
    public static class FileUtils
    {
        public static void DeleteFilesInDirectory(string dir, DateTime lastAccessTime)
        {
            var files = new DirectoryInfo(dir)
                .GetFiles()
                .Where(f => f.LastAccessTime < lastAccessTime)
                .ToList();
            foreach (var file in files)
            {
                file.Delete();
            }
        }
    }
}
