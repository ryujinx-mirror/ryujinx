using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IAudioController : IpcService
    {
        public IAudioController() { }

        [CommandCmif(0)]
        // SetExpectedMasterVolume(f32, f32)
        public ResultCode SetExpectedMasterVolume(ServiceCtx context)
        {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            float appletVolume = context.RequestData.ReadSingle();
            float libraryAppletVolume = context.RequestData.ReadSingle();
#pragma warning restore IDE0059

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // GetMainAppletExpectedMasterVolume() -> f32
        public ResultCode GetMainAppletExpectedMasterVolume(ServiceCtx context)
        {
            context.ResponseData.Write(1f);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetLibraryAppletExpectedMasterVolume() -> f32
        public ResultCode GetLibraryAppletExpectedMasterVolume(ServiceCtx context)
        {
            context.ResponseData.Write(1f);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // ChangeMainAppletMasterVolume(f32, u64)
        public ResultCode ChangeMainAppletMasterVolume(ServiceCtx context)
        {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            float unknown0 = context.RequestData.ReadSingle();
            long unknown1 = context.RequestData.ReadInt64();
#pragma warning restore IDE0059

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // SetTransparentVolumeRate(f32)
        public ResultCode SetTransparentVolumeRate(ServiceCtx context)
        {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            float unknown0 = context.RequestData.ReadSingle();
#pragma warning restore IDE0059

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
    }
}
