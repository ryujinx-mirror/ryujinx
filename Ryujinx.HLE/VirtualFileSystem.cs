using System;
using System.IO;

namespace Ryujinx.HLE
{
    class VirtualFileSystem : IDisposable
    {
        private const string BasePath   = "RyuFs";
        private const string NandPath   = "nand";
        private const string SdCardPath = "sdmc";
        private const string SystemPath = "system";

        public Stream RomFs { get; private set; }

        public void LoadRomFs(string FileName)
        {
            RomFs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        }

        public void SetRomFs(Stream RomfsStream)
        {
            RomFs?.Close();
            RomFs = RomfsStream;
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

        public string GetSystemPath() => MakeDirAndGetFullPath(SystemPath);

        public string SwitchPathToSystemPath(string SwitchPath)
        {
            string[] Parts = SwitchPath.Split(":");
            if (Parts.Length != 2)
            {
                return null;
            }
            return GetFullPath(MakeDirAndGetFullPath(Parts[0]), Parts[1]);
        }

        public string SystemPathToSwitchPath(string SystemPath)
        {
            string BaseSystemPath = GetBasePath() + Path.DirectorySeparatorChar;
            if (SystemPath.StartsWith(BaseSystemPath))
            {
                string RawPath = SystemPath.Replace(BaseSystemPath, "");
                int FirstSeparatorOffset = RawPath.IndexOf(Path.DirectorySeparatorChar);
                if (FirstSeparatorOffset == -1)
                {
                    return $"{RawPath}:/";
                }

                string BasePath = RawPath.Substring(0, FirstSeparatorOffset);
                string FileName = RawPath.Substring(FirstSeparatorOffset + 1);
                return $"{BasePath}:/{FileName}";
            }
            return null;
        }

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