using System;
using System.IO;

using static Ryujinx.HLE.FileSystem.VirtualFileSystem;

namespace Ryujinx.HLE.FileSystem.Content
{
    internal static class LocationHelper
    {
        public static string GetRealPath(VirtualFileSystem FileSystem, string SwitchContentPath)
        {
            string BasePath = FileSystem.GetBasePath();

            switch (SwitchContentPath)
            {
                case ContentPath.SystemContent:
                    return Path.Combine(FileSystem.GetBasePath(), SystemNandPath, "Contents");
                case ContentPath.UserContent:
                    return Path.Combine(FileSystem.GetBasePath(), UserNandPath, "Contents");
                case ContentPath.SdCardContent:
                    return Path.Combine(FileSystem.GetSdCardPath(), "Nintendo", "Contents");
                case ContentPath.System:
                    return Path.Combine(BasePath, SystemNandPath);
                case ContentPath.User:
                    return Path.Combine(BasePath, UserNandPath);
                default:
                    throw new NotSupportedException($"Content Path `{SwitchContentPath}` is not supported.");
            }
        }

        public static string GetContentPath(ContentStorageId ContentStorageId)
        {
            switch (ContentStorageId)
            {
                case ContentStorageId.NandSystem:
                    return ContentPath.SystemContent;
                case ContentStorageId.NandUser:
                    return ContentPath.UserContent;
                case ContentStorageId.SdCard:
                    return ContentPath.SdCardContent;
                default:
                    throw new NotSupportedException($"Content Storage `{ContentStorageId}` is not supported.");
            }
        }

        public static string GetContentRoot(StorageId StorageId)
        {
            switch (StorageId)
            {
                case StorageId.NandSystem:
                    return ContentPath.SystemContent;
                case StorageId.NandUser:
                    return ContentPath.UserContent;
                case StorageId.SdCard:
                    return ContentPath.SdCardContent;
                default:
                    throw new NotSupportedException($"Storage Id `{StorageId}` is not supported.");
            }
        }

        public static StorageId GetStorageId(string ContentPathString)
        {
            string CleanedPath = ContentPathString.Split(':')[0];

            switch (CleanedPath)
            {
                case ContentPath.SystemContent:
                case ContentPath.System:
                    return StorageId.NandSystem;

                case ContentPath.UserContent:
                case ContentPath.User:
                    return StorageId.NandUser;

                case ContentPath.SdCardContent:
                    return StorageId.SdCard;

                case ContentPath.Host:
                    return StorageId.Host;

                case ContentPath.GamecardApp:
                case ContentPath.GamecardContents:
                case ContentPath.GamecardUpdate:
                    return StorageId.GameCard;

                default:
                    return StorageId.None;
            }
        }
    }
}
