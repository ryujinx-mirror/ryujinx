using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IAudioController : IpcService
    {
        public IAudioController() { }

        [Command(0)]
        // SetExpectedMasterVolume(f32, f32)
        public long SetExpectedMasterVolume(ServiceCtx context)
        {
            float appletVolume        = context.RequestData.ReadSingle();
            float libraryAppletVolume = context.RequestData.ReadSingle();

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(1)]
        // GetMainAppletExpectedMasterVolume() -> f32
        public long GetMainAppletExpectedMasterVolume(ServiceCtx context)
        {
            context.ResponseData.Write(1f);

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(2)]
        // GetLibraryAppletExpectedMasterVolume() -> f32
        public long GetLibraryAppletExpectedMasterVolume(ServiceCtx context)
        {
            context.ResponseData.Write(1f);

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(3)]
        // ChangeMainAppletMasterVolume(f32, u64)
        public long ChangeMainAppletMasterVolume(ServiceCtx context)
        {
            float unknown0 = context.RequestData.ReadSingle();
            long  unknown1 = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(4)]
        // SetTransparentVolumeRate(f32)
        public long SetTransparentVolumeRate(ServiceCtx context)
        {
            float unknown0 = context.RequestData.ReadSingle();

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }
    }
}