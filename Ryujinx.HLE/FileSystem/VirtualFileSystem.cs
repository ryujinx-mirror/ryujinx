using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS;
using System;
using System.IO;

namespace Ryujinx.HLE.FileSystem
{
    public class VirtualFileSystem : IDisposable
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

        public string GetSdCardPath() => MakeFullPath(SdCardPath);

        public string GetNandPath() => MakeFullPath(NandPath);

        public string GetSystemPath() => MakeFullPath(SystemPath);

        internal string GetSavePath(ServiceCtx context, SaveInfo saveInfo, bool isDirectory = true)
        {
            string saveUserPath   = "";
            string baseSavePath   = NandPath;
            ulong  currentTitleId = saveInfo.TitleId;

            switch (saveInfo.SaveSpaceId)
            {
                case SaveSpaceId.NandUser:   baseSavePath = UserNandPath;                         break;
                case SaveSpaceId.NandSystem: baseSavePath = SystemNandPath;                       break;
                case SaveSpaceId.SdCard:     baseSavePath = Path.Combine(SdCardPath, "Nintendo"); break;
            }

            baseSavePath = Path.Combine(baseSavePath, "save");

            if (saveInfo.TitleId == 0 && saveInfo.SaveDataType == SaveDataType.SaveData)
            {
                currentTitleId = context.Process.TitleId;
            }

            if (saveInfo.SaveSpaceId == SaveSpaceId.NandUser)
            {
                saveUserPath = saveInfo.UserId.IsNull ? "savecommon" : saveInfo.UserId.ToString();
            }

            string savePath = Path.Combine(baseSavePath,
                saveInfo.SaveId.ToString("x16"),
                saveUserPath,
                saveInfo.SaveDataType == SaveDataType.SaveData ? currentTitleId.ToString("x16") : string.Empty);

            return MakeFullPath(savePath, isDirectory);
        }

        public string GetFullPartitionPath(string partitionPath)
        {
            return MakeFullPath(partitionPath);
        }

        public string SwitchPathToSystemPath(string switchPath)
        {
            string[] parts = switchPath.Split(":");

            if (parts.Length != 2)
            {
                return null;
            }

            return GetFullPath(MakeFullPath(parts[0]), parts[1]);
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

        private string MakeFullPath(string path, bool isDirectory = true)
        {
            // Handles Common Switch Content Paths
            switch (path)
            {
                case ContentPath.SdCard:
                case "@Sdcard":
                    path = SdCardPath;
                    break;
                case ContentPath.User:
                    path = UserNandPath;
                    break;
                case ContentPath.System:
                    path = SystemNandPath;
                    break;
                case ContentPath.SdCardContent:
                    path = Path.Combine(SdCardPath, "Nintendo", "Contents");
                    break;
                case ContentPath.UserContent:
                    path = Path.Combine(UserNandPath, "Contents");
                    break;
                case ContentPath.SystemContent:
                    path = Path.Combine(SystemNandPath, "Contents");
                    break;
            }

            string fullPath = Path.Combine(GetBasePath(), path);

            if (isDirectory)
            {
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
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