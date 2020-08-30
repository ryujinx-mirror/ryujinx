using Ryujinx.Common.Logging;
using System;
using System.IO;

namespace Ryujinx.Common.Configuration
{
    public static class AppDataManager
    {
        private static readonly string _defaultBaseDirPath;

        private const string DefaultBaseDir = "Ryujinx";

        // The following 3 are always part of Base Directory
        private const string GamesDir = "games";
        private const string ProfilesDir = "profiles";
        private const string KeysDir = "system";

        public static bool IsCustomBasePath { get; private set; }
        public static string BaseDirPath { get; private set; }
        public static string GamesDirPath { get; private set; }
        public static string ProfilesDirPath { get; private set; }
        public static string KeysDirPath { get; private set; }
        public static string KeysDirPathAlt { get; }

        public const string DefaultNandDir = "bis";
        public const string DefaultSdcardDir = "sdcard";
        private const string DefaultModsDir = "mods";

        public static string CustomModsPath { get; set; }
        public static string CustomNandPath { get; set; } // TODO: Actually implement this into VFS
        public static string CustomSdCardPath { get; set; } // TODO: Actually implement this into VFS

        static AppDataManager()
        {
            _defaultBaseDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DefaultBaseDir);
            KeysDirPathAlt = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".switch");
        }

        public static void Initialize(string baseDirPath)
        {
            BaseDirPath = _defaultBaseDirPath;

            if (baseDirPath != null && baseDirPath != _defaultBaseDirPath)
            {
                if (!Directory.Exists(baseDirPath))
                {
                    Logger.Error?.Print(LogClass.Application, $"Custom Data Directory '{baseDirPath}' does not exist. Using defaults...");
                }
                else
                {
                    BaseDirPath = baseDirPath;
                    IsCustomBasePath = true;
                }
            }

            SetupBasePaths();
        }

        private static void SetupBasePaths()
        {
            Directory.CreateDirectory(BaseDirPath);
            Directory.CreateDirectory(GamesDirPath = Path.Combine(BaseDirPath, GamesDir));
            Directory.CreateDirectory(ProfilesDirPath = Path.Combine(BaseDirPath, ProfilesDir));
            Directory.CreateDirectory(KeysDirPath = Path.Combine(BaseDirPath, KeysDir));
        }

        public static string GetModsPath() => CustomModsPath ?? Directory.CreateDirectory(Path.Combine(BaseDirPath, DefaultModsDir)).FullName;
    }
}