using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Mm
{
    [Service("mm:u")]
    class IRequest : IpcService
    {
        public IRequest(ServiceCtx context) { }

        [Command(0)]
        // InitializeOld(u32, u32, u32)
        public ResultCode InitializeOld(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();
            int unknown1 = context.RequestData.ReadInt32();
            int unknown2 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, new { unknown0, unknown1, unknown2 });

            return ResultCode.Success;
        }

        [Command(1)]
        // FinalizeOld(u32)
        public ResultCode FinalizeOld(ServiceCtx context)
        {
            context.Device.Gpu.UninitializeVideoDecoder();

            Logger.PrintStub(LogClass.ServiceMm);

            return ResultCode.Success;
        }

        [Command(2)]
        // SetAndWaitOld(u32, u32, u32)
        public ResultCode SetAndWaitOld(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();
            int unknown1 = context.RequestData.ReadInt32();
            int unknown2 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, new { unknown0, unknown1, unknown2 });
            return ResultCode.Success;
        }

        [Command(3)]
        // GetOld(u32) -> u32
        public ResultCode GetOld(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, new { unknown0 });

            context.ResponseData.Write(0);

            return ResultCode.Success;
        }

        [Command(4)]
        // Initialize()
        public ResultCode Initialize(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceMm);

            return ResultCode.Success;
        }

        [Command(5)]
        // Finalize(u32)
        public ResultCode Finalize(ServiceCtx context)
        {
            context.Device.Gpu.UninitializeVideoDecoder();

            Logger.PrintStub(LogClass.ServiceMm);

            return ResultCode.Success;
        }

        [Command(6)]
        // SetAndWait(u32, u32, u32)
        public ResultCode SetAndWait(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();
            int unknown1 = context.RequestData.ReadInt32();
            int unknown2 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, new { unknown0, unknown1, unknown2 });

            return ResultCode.Success;
        }

        [Command(7)]
        // Get(u32) -> u32
        public ResultCode Get(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, new { unknown0 });

            context.ResponseData.Write(0);

            return ResultCode.Success;
        }
    }
}