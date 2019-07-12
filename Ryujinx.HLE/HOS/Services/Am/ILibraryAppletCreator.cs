namespace Ryujinx.HLE.HOS.Services.Am
{
    class ILibraryAppletCreator : IpcService
    {
        public ILibraryAppletCreator() { }

        [Command(0)]
        // CreateLibraryApplet(u32, u32) -> object<nn::am::service::ILibraryAppletAccessor>
        public long CreateLibraryApplet(ServiceCtx context)
        {
            MakeObject(context, new ILibraryAppletAccessor(context.Device.System));

            return 0;
        }

        [Command(10)]
        // CreateStorage(u64) -> object<nn::am::service::IStorage>
        public long CreateStorage(ServiceCtx context)
        {
            long size = context.RequestData.ReadInt64();

            MakeObject(context, new IStorage(new byte[size]));

            return 0;
        }
    }
}