using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Services.Caps.Types;

namespace Ryujinx.HLE.HOS.Services.Caps
{
    [Service("caps:u")]
    class IAlbumApplicationService : IpcService
    {
        public IAlbumApplicationService(ServiceCtx context) { }

        [CommandCmif(32)] // 7.0.0+
        // SetShimLibraryVersion(pid, u64, nn::applet::AppletResourceUserId)
        public ResultCode SetShimLibraryVersion(ServiceCtx context)
        {
            return context.Device.System.CaptureManager.SetShimLibraryVersion(context);
        }

        [CommandCmif(102)]
        // GetAlbumFileList0AafeAruidDeprecated(pid, u16 content_type, u64 start_time, u64 end_time, nn::applet::AppletResourceUserId) -> (u64 count, buffer<ApplicationAlbumFileEntry, 0x6>)
        public ResultCode GetAlbumFileList0AafeAruidDeprecated(ServiceCtx context)
        {
            // NOTE: ApplicationAlbumFileEntry size is 0x30.
            return GetAlbumFileList(context);
        }

        [CommandCmif(142)]
        // GetAlbumFileList3AaeAruid(pid, u16 content_type, u64 start_time, u64 end_time, nn::applet::AppletResourceUserId) -> (u64 count, buffer<ApplicationAlbumFileEntry, 0x6>)
        public ResultCode GetAlbumFileList3AaeAruid(ServiceCtx context)
        {
            // NOTE: ApplicationAlbumFileEntry size is 0x20.
            return GetAlbumFileList(context);
        }

        private ResultCode GetAlbumFileList(ServiceCtx context)
        {
            ResultCode resultCode = ResultCode.Success;
            ulong count = 0;

            ContentType contentType = (ContentType)context.RequestData.ReadUInt16();
            ulong startTime = context.RequestData.ReadUInt64();
            ulong endTime = context.RequestData.ReadUInt64();

            context.RequestData.ReadUInt16(); // Alignment.

            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            ulong applicationAlbumFileEntryPosition = context.Request.ReceiveBuff[0].Position;
            ulong applicationAlbumFileEntrySize = context.Request.ReceiveBuff[0].Size;

            MemoryHelper.FillWithZeros(context.Memory, applicationAlbumFileEntryPosition, (int)applicationAlbumFileEntrySize);

            if (contentType > ContentType.Unknown || contentType == ContentType.ExtraMovie)
            {
                resultCode = ResultCode.InvalidContentType;
            }

            // TODO: Service checks if the pid is present in an internal list and returns ResultCode.BlacklistedPid if it is.
            //       The list contents needs to be determined.
            //       Service populate the buffer with a ApplicationAlbumFileEntry related to the pid.

            Logger.Stub?.PrintStub(LogClass.ServiceCaps, new { contentType, startTime, endTime, appletResourceUserId });

            context.ResponseData.Write(count);

            return resultCode;
        }
    }
}
