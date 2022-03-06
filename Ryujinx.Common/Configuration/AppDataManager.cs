using Ryujinx.Common.Logging;
using System;
using System.IO;

namespace Ryujinx.Common.Configuration
{
    public static class AppDataManager
    {
        public const string DefaultBaseDir = "Ryujinx";
        public const string DefaultPortableDir = "portable";

        // The following 3 are always part of Base Directory
        private const string GamesDir = "games";
        private const string ProfilesDir = "profiles";
        private const string KeysDir = "system";

        public enum LaunchMode
        {
            UserProfile,
            Portable,
            Custom
        }

        public static LaunchMode Mode { get; private set; }

        public static string BaseDirPath { get; private set; }
        public static string GamesDirPath { get; private set; }
        public static string ProfilesDirPath { get; private set; }
        public static string KeysDirPath { get; private set; }
        public static string KeysDirPathUser { get; }

        public const string DefaultNandDir = "bis";
        public const string DefaultSdcardDir = "sdcard";
        private const string DefaultModsDir = "mods";

        public static string CustomModsPath { get; set; }
        public static string CustomSdModsPath {get; set; }
        public static string CustomNandPath { get; set; } // TODO: Actually implement this into VFS
        public static string CustomSdCardPath { get; set; } // TODO: Actually implement this into VFS

        static AppDataManager()
        {
            KeysDirPathUser = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".switch");
        }

        public static void Initialize(string baseDirPath)
        {
            string userProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DefaultBaseDir);
            string portablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultPortableDir);

            if (Directory.Exists(portablePath))
            {
                BaseDirPath = portablePath;
                Mode = LaunchMode.Portable;
            }
            else
            {
                BaseDirPath = userProfilePath;
                Mode = LaunchMode.UserProfile;
            }

            if (baseDirPath != null && baseDirPath != userProfilePath)
            {
                if (!Directory.Exists(baseDirPath))
                {
                    Logger.Error?.Print(LogClass.Application, $"Custom Data Directory '{baseDirPath}' does not exist. Falling back to {Mode}...");
                }
                else
                {
                    BaseDirPath = baseDirPath;
                    Mode = LaunchMode.Custom;
                }
            }

            BaseDirPath = Path.GetFullPath(BaseDirPath); // convert relative paths

            SetupBasePaths();
        }

        private static void SetupBasePaths()
        {
            Directory.CreateDirectory(BaseDirPath);
            Directory.CreateDirectory(GamesDirPath = Path.Combine(BaseDirPath, GamesDir));
            Directory.CreateDirectory(ProfilesDirPath = Path.Combine(BaseDirPath, ProfilesDir));
            Directory.CreateDirectory(KeysDirPath = Path.Combine(BaseDirPath, KeysDir));
        }

        public static string GetModsPath()   => CustomModsPath ?? Directory.CreateDirectory(Path.Combine(BaseDirPath, DefaultModsDir)).FullName;
        public static string GetSdModsPath() => CustomSdModsPath ?? Directory.CreateDirectory(Path.Combine(BaseDirPath, DefaultSdcardDir, "atmosphere")).FullName;
    }
}