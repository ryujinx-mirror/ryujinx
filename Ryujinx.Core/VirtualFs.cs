using System;
using System.IO;

namespace Ryujinx.Core
{
    class VirtualFs : IDisposable
    {
        private const string BasePath   = "Fs";
        private const string SavesPath  = "Saves";
        private const string SdCardPath = "SdCard";

        public Stream RomFs { get; private set; }

        public void LoadRomFs(string FileName)
        {
            RomFs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        }

        public string GetFullPath(string BasePath, string FileName)
        {
            if (FileName.StartsWith('/'))
            {
                FileName = FileName.Substring(1);
            }

            string FullPath = Path.GetFullPath(Path.Combine(BasePath, FileName));

            if (!FullPath.StartsWith(GetBasePath()))
            {
                return null;
            }

            return FullPath;
        }

        public string GetSdCardPath() => MakeDirAndGetFullPath(SdCardPath);

        public string GetGameSavesPath() => MakeDirAndGetFullPath(SavesPath);

        private static string MakeDirAndGetFullPath(string Dir)
        {
            string FullPath = Path.Combine(GetBasePath(), Dir);

            if (!Directory.Exists(FullPath))
            {
                Directory.CreateDirectory(FullPath);
            }

            return FullPath;
        }

        public static string GetBasePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), BasePath);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RomFs?.Dispose();
            }
        }
    }
}