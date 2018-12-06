using System;
using System.IO;

using static Ryujinx.HLE.FileSystem.VirtualFileSystem;

namespace Ryujinx.HLE.FileSystem.Content
{
    internal static class LocationHelper
    {
        public static string GetRealPath(VirtualFileSystem fileSystem, string switchContentPath)
        {
            string basePath = fileSystem.GetBasePath();

            switch (switchContentPath)
            {
                case ContentPath.SystemContent:
                    return Path.Combine(fileSystem.GetBasePath(), SystemNandPath, "Contents");
                case ContentPath.UserContent:
                    return Path.Combine(fileSystem.GetBasePath(), UserNandPath, "Contents");
                case ContentPath.SdCardContent:
                    return Path.Combine(fileSystem.GetSdCardPath(), "Nintendo", "Contents");
                case ContentPath.System:
                    return Path.Combine(basePath, SystemNandPath);
                case ContentPath.User:
                    return Path.Combine(basePath, UserNandPath);
                default:
                    throw new NotSupportedException($"Content Path `{switchContentPath}` is not supported.");
            }
        }

        public static string GetContentPath(ContentStorageId contentStorageId)
        {
            switch (contentStorageId)
            {
                case ContentStorageId.NandSystem:
                    return ContentPath.SystemContent;
                case ContentStorageId.NandUser:
                    return ContentPath.UserContent;
                case ContentStorageId.SdCard:
                    return ContentPath.SdCardContent;
                default:
                    throw new NotSupportedException($"Content Storage `{contentStorageId}` is not supported.");
            }
        }

        public static string GetContentRoot(StorageId storageId)
        {
            switch (storageId)
            {
                case StorageId.NandSystem:
                    return ContentPath.SystemContent;
                case StorageId.NandUser:
                    return ContentPath.UserContent;
                case StorageId.SdCard:
                    return ContentPath.SdCardContent;
                default:
                    throw new NotSupportedException($"Storage Id `{storageId}` is not supported.");
            }
        }

        public static StorageId GetStorageId(string contentPathString)
        {
            string cleanedPath = contentPathString.Split(':')[0];

            switch (cleanedPath)
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
