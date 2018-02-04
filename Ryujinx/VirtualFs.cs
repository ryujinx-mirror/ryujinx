using System;
using System.IO;

namespace Ryujinx
{
    class VirtualFs : IDisposable
    {
        private const string BasePath  = "Fs";
        private const string SavesPath = "Saves";

        public Stream RomFs { get; private set; }

        public void LoadRomFs(string FileName)
        {
            RomFs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        }

        internal string GetFullPath(string BasePath, string FileName)
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

        internal string GetGameSavesPath()
        {
            string SavesDir = Path.Combine(GetBasePath(), SavesPath);

            if (!Directory.Exists(SavesDir))
            {
                Directory.CreateDirectory(SavesDir);
            }

            return SavesDir;
        }

        internal string GetBasePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), BasePath);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && RomFs != null)
            {
                RomFs.Dispose();
            }
        }
    }
}