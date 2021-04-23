using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Services.Caps.Types;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Ryujinx.HLE.HOS.Services.Caps
{
    class CaptureManager
    {
        private string _sdCardPath;

        private uint _shimLibraryVersion;

        public CaptureManager(Switch device)
        {
            _sdCardPath = device.FileSystem.GetSdCardPath();
        }

        public ResultCode SetShimLibraryVersion(ServiceCtx context)
        {
            ulong shimLibraryVersion   = context.RequestData.ReadUInt64();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            // TODO: Service checks if the pid is present in an internal list and returns ResultCode.BlacklistedPid if it is.
            //       The list contents needs to be determined.

            ResultCode resultCode = ResultCode.OutOfRange;

            if (shimLibraryVersion != 0)
            {
                if (_shimLibraryVersion == shimLibraryVersion)
                {
                    resultCode = ResultCode.Success;
                }
                else if (_shimLibraryVersion != 0)
                {
                    resultCode = ResultCode.ShimLibraryVersionAlreadySet;
                }
                else if (shimLibraryVersion == 1)
                {
                    resultCode = ResultCode.Success;

                    _shimLibraryVersion = 1;
                }
            }

            return resultCode;
        }

        public ResultCode SaveScreenShot(byte[] screenshotData, ulong appletResourceUserId, ulong titleId, out ApplicationAlbumEntry applicationAlbumEntry)
        {
            applicationAlbumEntry = default;

            if (screenshotData.Length == 0)
            {
                return ResultCode.NullInputBuffer;
            }

            /*
            // NOTE: On our current implementation, appletResourceUserId starts at 0, disable it for now.
            if (appletResourceUserId == 0)
            {
                return ResultCode.InvalidArgument;
            }
            */

            /*
            // Doesn't occur in our case.
            if (applicationAlbumEntry == null)
            {
                return ResultCode.NullOutputBuffer;
            }
            */

            if (screenshotData.Length >= 0x384000)
            {
                DateTime currentDateTime = DateTime.Now;

                applicationAlbumEntry = new ApplicationAlbumEntry()
                {
                    Size              = (ulong)Unsafe.SizeOf<ApplicationAlbumEntry>(),
                    TitleId           = titleId,
                    AlbumFileDateTime = new AlbumFileDateTime()
                    {
                        Year     = (ushort)currentDateTime.Year,
                        Month    = (byte)currentDateTime.Month,
                        Day      = (byte)currentDateTime.Day,
                        Hour     = (byte)currentDateTime.Hour,
                        Minute   = (byte)currentDateTime.Minute,
                        Second   = (byte)currentDateTime.Second,
                        UniqueId = 0
                    },
                    AlbumStorage      = AlbumStorage.Sd,
                    ContentType       = ContentType.Screenshot,
                    Padding           = new Array5<byte>(),
                    Unknown0x1f       = 1
                };

                using (SHA256 sha256Hash = SHA256.Create())
                {
                    // NOTE: The hex hash is a HMAC-SHA256 (first 32 bytes) using a hardcoded secret key over the titleId, we can simulate it by hashing the titleId instead.
                    string hash       = BitConverter.ToString(sha256Hash.ComputeHash(BitConverter.GetBytes(titleId))).Replace("-", "").Remove(0x20);
                    string folderPath = Path.Combine(_sdCardPath, "Nintendo", "Album", currentDateTime.Year.ToString("00"), currentDateTime.Month.ToString("00"), currentDateTime.Day.ToString("00"));
                    string filePath   = GenerateFilePath(folderPath, applicationAlbumEntry, currentDateTime, hash);

                    // TODO: Handle that using the FS service implementation and return the right error code instead of throwing exceptions.
                    Directory.CreateDirectory(folderPath);

                    while (File.Exists(filePath))
                    {
                        applicationAlbumEntry.AlbumFileDateTime.UniqueId++;

                        filePath = GenerateFilePath(folderPath, applicationAlbumEntry, currentDateTime, hash);
                    }
                
                    // NOTE: The saved JPEG file doesn't have the limitation in the extra EXIF data.
                    Image.LoadPixelData<Rgba32>(screenshotData, 1280, 720).SaveAsJpegAsync(filePath);
                }

                return ResultCode.Success;
            }

            return ResultCode.NullInputBuffer;
        }

        private string GenerateFilePath(string folderPath, ApplicationAlbumEntry applicationAlbumEntry, DateTime currentDateTime, string hash)
        {
            string fileName = $"{currentDateTime:yyyyMMddHHmmss}{applicationAlbumEntry.AlbumFileDateTime.UniqueId:00}-{hash}.jpg";

            return Path.Combine(folderPath, fileName);
        }
    }
}