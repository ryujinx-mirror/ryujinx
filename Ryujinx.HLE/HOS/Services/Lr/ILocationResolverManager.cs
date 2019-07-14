using Ryujinx.HLE.FileSystem;

namespace Ryujinx.HLE.HOS.Services.Lr
{
    [Service("lr")]
    class ILocationResolverManager : IpcService
    {
        public ILocationResolverManager(ServiceCtx context) { }

        [Command(0)]
        // OpenLocationResolver()
        public ResultCode OpenLocationResolver(ServiceCtx context)
        {
            StorageId storageId = (StorageId)context.RequestData.ReadByte();

            MakeObject(context, new ILocationResolver(storageId));

            return ResultCode.Success;
        }
    }
}