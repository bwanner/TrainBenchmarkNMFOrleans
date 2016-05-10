using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace BenchmarkLibrary
{
    public class BenchmarkSetup
    {
        public static void SetupOnAzure(string targetModelDir, string smbPath, string user, string key)
        {
            Trace.TraceInformation("Mounting network drive");
            PaasDriveMapping.MountShare(smbPath, "z:", user,
                key);
            Trace.TraceInformation("Mounted.");

            using (FileStream fs = new FileStream("Z:\\railway-all.zip", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var targetFile = targetModelDir + entry.Name;
                        if(!File.Exists(targetFile))
                            entry.ExtractToFile(targetModelDir + entry.Name);
                    }
                } 
            }
        }
    }
}