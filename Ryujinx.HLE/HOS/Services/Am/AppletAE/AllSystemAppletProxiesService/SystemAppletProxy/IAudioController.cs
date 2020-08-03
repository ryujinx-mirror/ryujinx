using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IAudioController : IpcService
    {
        public IAudioController() { }

        [Command(0)]
        // SetExpectedMasterVolume(f32, f32)
        public ResultCode SetExpectedMasterVolume(ServiceCtx context)
        {
            float appletVolume        = context.RequestData.ReadSingle();
            float libraryAppletVolume = context.RequestData.ReadSingle();

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(1)]
        // GetMainAppletExpectedMasterVolume() -> f32
        public ResultCode GetMainAppletExpectedMasterVolume(ServiceCtx context)
        {
            context.ResponseData.Write(1f);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(2)]
        // GetLibraryAppletExpectedMasterVolume() -> f32
        public ResultCode GetLibraryAppletExpectedMasterVolume(ServiceCtx context)
        {
            context.ResponseData.Write(1f);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(3)]
        // ChangeMainAppletMasterVolume(f32, u64)
        public ResultCode ChangeMainAppletMasterVolume(ServiceCtx context)
        {
            float unknown0 = context.RequestData.ReadSingle();
            long  unknown1 = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(4)]
        // SetTransparentVolumeRate(f32)
        public ResultCode SetTransparentVolumeRate(ServiceCtx context)
        {
            float unknown0 = context.RequestData.ReadSingle();

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
    }
}