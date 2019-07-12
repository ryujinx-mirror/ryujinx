using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Mm
{
    [Service("mm:u")]
    class IRequest : IpcService
    {
        public IRequest(ServiceCtx context) { }

        [Command(0)]
        // InitializeOld(u32, u32, u32)
        public long InitializeOld(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();
            int unknown1 = context.RequestData.ReadInt32();
            int unknown2 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, new { unknown0, unknown1, unknown2 });

            return 0;
        }

        [Command(1)]
        // FinalizeOld(u32)
        public long FinalizeOld(ServiceCtx context)
        {
            context.Device.Gpu.UninitializeVideoDecoder();

            Logger.PrintStub(LogClass.ServiceMm);

            return 0;
        }

        [Command(2)]
        // SetAndWaitOld(u32, u32, u32)
        public long SetAndWaitOld(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();
            int unknown1 = context.RequestData.ReadInt32();
            int unknown2 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, new { unknown0, unknown1, unknown2 });
            return 0;
        }

        [Command(3)]
        // GetOld(u32) -> u32
        public long GetOld(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, new { unknown0 });

            context.ResponseData.Write(0);

            return 0;
        }

        [Command(4)]
        // Initialize()
        public long Initialize(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceMm);

            return 0;
        }

        [Command(5)]
        // Finalize(u32)
        public long Finalize(ServiceCtx context)
        {
            context.Device.Gpu.UninitializeVideoDecoder();

            Logger.PrintStub(LogClass.ServiceMm);

            return 0;
        }

        [Command(6)]
        // SetAndWait(u32, u32, u32)
        public long SetAndWait(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();
            int unknown1 = context.RequestData.ReadInt32();
            int unknown2 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, new { unknown0, unknown1, unknown2 });

            return 0;
        }

        [Command(7)]
        // Get(u32) -> u32
        public long Get(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, new { unknown0 });

            context.ResponseData.Write(0);

            return 0;
        }
    }
}