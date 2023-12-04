using LibHac.Common;
using LibHac.Ns;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    class IReadOnlyApplicationControlDataInterface : IpcService
    {
        public IReadOnlyApplicationControlDataInterface(ServiceCtx context) { }

        [CommandCmif(0)]
        // GetApplicationControlData(u8, u64) -> (unknown<4>, buffer<unknown, 6>)
        public ResultCode GetApplicationControlData(ServiceCtx context)
        {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            byte source = (byte)context.RequestData.ReadInt64();
            ulong titleId = context.RequestData.ReadUInt64();
#pragma warning restore IDE0059

            ulong position = context.Request.ReceiveBuff[0].Position;

            ApplicationControlProperty nacp = context.Device.Processes.ActiveApplication.ApplicationControlProperties;

            context.Memory.Write(position, SpanHelpers.AsByteSpan(ref nacp).ToArray());

            return ResultCode.Success;
        }
    }
}
