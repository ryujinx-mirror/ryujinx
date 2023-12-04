using LibHac.Ncm;
using Ryujinx.HLE.HOS.Services.Ncm.Lr.LocationResolverManager;

namespace Ryujinx.HLE.HOS.Services.Ncm.Lr
{
    [Service("lr")]
    class ILocationResolverManager : IpcService
    {
        public ILocationResolverManager(ServiceCtx context) { }

        [CommandCmif(0)]
        // OpenLocationResolver()
        public ResultCode OpenLocationResolver(ServiceCtx context)
        {
            StorageId storageId = (StorageId)context.RequestData.ReadByte();

            MakeObject(context, new ILocationResolver(storageId));

            return ResultCode.Success;
        }
    }
}
