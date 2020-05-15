namespace Ryujinx.HLE.HOS.Services.Ns
{
    class IReadOnlyApplicationControlDataInterface : IpcService
    {
        public IReadOnlyApplicationControlDataInterface(ServiceCtx context) { }

        [Command(0)]
        // GetApplicationControlData(u8, u64) -> (unknown<4>, buffer<unknown, 6>)
        public ResultCode GetApplicationControlData(ServiceCtx context)
        {
            byte source = (byte)context.RequestData.ReadInt64();
            ulong titleId = context.RequestData.ReadUInt64();

            long position = context.Request.ReceiveBuff[0].Position;

            byte[] nacpData = context.Device.Application.ControlData.ByteSpan.ToArray();

            context.Memory.Write((ulong)position, nacpData);

            return ResultCode.Success;
        }
    }
}
