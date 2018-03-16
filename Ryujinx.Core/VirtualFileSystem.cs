using System;
using System.IO;

namespace Ryujinx.Core
{
    class VirtualFileSystem : IDisposable
    {
        private const string BasePath   = "RyuFs";
        private const string NandPath   = "nand";
        private const string SdCardPath = "sdmc";

        public Stream RomFs { get; private set; }

        public void LoadRomFs(string FileName)
        {
            RomFs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        }

        public string GetFullPath(string BasePath, string FileName)
        {
            if (FileName.StartsWith("//"))
            {
                FileName = FileName.Substring(2);
            }
            else if (FileName.StartsWith('/'))
            {
                FileName = FileName.Substring(1);
            }
            else
            {
                return null;
            }

            string FullPath = Path.GetFullPath(Path.Combine(BasePath, FileName));

            if (!FullPath.StartsWith(GetBasePath()))
            {
                return null;
            }

            return FullPath;
        }

        public string GetSdCardPath() => MakeDirAndGetFullPath(SdCardPath);

        public string GetGameSavesPath() => MakeDirAndGetFullPath(NandPath);

        private string MakeDirAndGetFullPath(string Dir)
        {
            string FullPath = Path.Combine(GetBasePath(), Dir);

            if (!Directory.Exists(FullPath))
            {
                Directory.CreateDirectory(FullPath);
            }

            return FullPath;
        }

        public DriveInfo GetDrive()
        {
            return new DriveInfo(Path.GetPathRoot(GetBasePath()));
        }

        public string GetBasePath()
        {
            string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            return Path.Combine(AppDataPath, BasePath);
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