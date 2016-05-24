using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using NMF.Expressions.Linq.Orleans.Model;
using NMF.Models;

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

            var modelLoader = new AzureModelLoader(targetModelDir);
            ModelLoader.Instance = modelLoader;
        }

        public static void SetupOnLocal(string path, string targetModelDir)
        {
            using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var targetFile = targetModelDir + entry.Name;
                        if (!File.Exists(targetFile))
                            entry.ExtractToFile(targetModelDir + entry.Name);
                    }
                }
            }

            var modelLoader = new AzureModelLoader(targetModelDir);
            ModelLoader.Instance = modelLoader;
        }
    }

    public class AzureModelLoader : IModelLoader
    {
        private readonly string _modelRootPath;

        public AzureModelLoader(string modelRootPath)
        {
            _modelRootPath = modelRootPath;
        }

        public T LoadModel<T>(string modelPath) where T : IResolvableModel
        {
            return ModelUtil.LoadModelFromPath<T>(_modelRootPath + modelPath);
        }
    }
}