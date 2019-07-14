namespace Ryujinx.HLE.HOS.Services.Am
{
    class IStorage : IpcService
    {
        public byte[] Data { get; private set; }

        public IStorage(byte[] data)
        {
            Data = data;
        }

        [Command(0)]
        // Open() -> object<nn::am::service::IStorageAccessor>
        public ResultCode Open(ServiceCtx context)
        {
            MakeObject(context, new IStorageAccessor(this));

            return ResultCode.Success;
        }
    }
}