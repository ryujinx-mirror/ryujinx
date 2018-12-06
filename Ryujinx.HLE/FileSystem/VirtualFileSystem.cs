using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS;
using System;
using System.IO;

namespace Ryujinx.HLE.FileSystem
{
    class VirtualFileSystem : IDisposable
    {
        public const string BasePath   = "RyuFs";
        public const string NandPath   = "nand";
        public const string SdCardPath = "sdmc";
        public const string SystemPath = "system";

        public static string SafeNandPath   = Path.Combine(NandPath, "safe");
        public static string SystemNandPath = Path.Combine(NandPath, "system");
        public static string UserNandPath   = Path.Combine(NandPath, "user");

        public Stream RomFs { get; private set; }

        public void LoadRomFs(string fileName)
        {
            RomFs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        public void SetRomFs(Stream romfsStream)
        {
            RomFs?.Close();
            RomFs = romfsStream;
        }

        public string GetFullPath(string basePath, string fileName)
        {
            if (fileName.StartsWith("//"))
            {
                fileName = fileName.Substring(2);
            }
            else if (fileName.StartsWith('/'))
            {
                fileName = fileName.Substring(1);
            }
            else
            {
                return null;
            }

            string fullPath = Path.GetFullPath(Path.Combine(basePath, fileName));

            if (!fullPath.StartsWith(GetBasePath()))
            {
                return null;
            }

            return fullPath;
        }

        public string GetSdCardPath() => MakeDirAndGetFullPath(SdCardPath);

        public string GetNandPath() => MakeDirAndGetFullPath(NandPath);

        public string GetSystemPath() => MakeDirAndGetFullPath(SystemPath);

        public string GetGameSavePath(SaveInfo save, ServiceCtx context)
        {
            return MakeDirAndGetFullPath(SaveHelper.GetSavePath(save, context));
        }

        public string GetFullPartitionPath(string partitionPath)
        {
            return MakeDirAndGetFullPath(partitionPath);
        }

        public string SwitchPathToSystemPath(string switchPath)
        {
            string[] parts = switchPath.Split(":");

            if (parts.Length != 2)
            {
                return null;
            }

            return GetFullPath(MakeDirAndGetFullPath(parts[0]), parts[1]);
        }

        public string SystemPathToSwitchPath(string systemPath)
        {
            string baseSystemPath = GetBasePath() + Path.DirectorySeparatorChar;

            if (systemPath.StartsWith(baseSystemPath))
            {
                string rawPath              = systemPath.Replace(baseSystemPath, "");
                int    firstSeparatorOffset = rawPath.IndexOf(Path.DirectorySeparatorChar);

                if (firstSeparatorOffset == -1)
                {
                    return $"{rawPath}:/";
                }

                string basePath = rawPath.Substring(0, firstSeparatorOffset);
                string fileName = rawPath.Substring(firstSeparatorOffset + 1);

                return $"{basePath}:/{fileName}";
            }
            return null;
        }

        private string MakeDirAndGetFullPath(string dir)
        {
            // Handles Common Switch Content Paths
            switch (dir)
            {
                case ContentPath.SdCard:
                case "@Sdcard":
                    dir = SdCardPath;
                    break;
                case ContentPath.User:
                    dir = UserNandPath;
                    break;
                case ContentPath.System:
                    dir = SystemNandPath;
                    break;
                case ContentPath.SdCardContent:
                    dir = Path.Combine(SdCardPath, "Nintendo", "Contents");
                    break;
                case ContentPath.UserContent:
                    dir = Path.Combine(UserNandPath, "Contents");
                    break;
                case ContentPath.SystemContent:
                    dir = Path.Combine(SystemNandPath, "Contents");
                    break;
            }

            string fullPath = Path.Combine(GetBasePath(), dir);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return fullPath;
        }

        public DriveInfo GetDrive()
        {
            return new DriveInfo(Path.GetPathRoot(GetBasePath()));
        }

        public string GetBasePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            return Path.Combine(appDataPath, BasePath);
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