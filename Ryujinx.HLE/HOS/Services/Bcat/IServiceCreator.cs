namespace Ryujinx.HLE.HOS.Services.Bcat
{
    [Service("bcat:a")]
    [Service("bcat:m")]
    [Service("bcat:u")]
    [Service("bcat:s")]
    class IServiceCreator : IpcService
    {
        public IServiceCreator(ServiceCtx context) { }

        [Command(0)]
        // CreateBcatService(u64, pid) -> object<nn::bcat::detail::ipc::IBcatService>
        public long CreateBcatService(ServiceCtx context)
        {
            long id = context.RequestData.ReadInt64();

            MakeObject(context, new IBcatService());

            return 0;
        }

        [Command(1)]
        // CreateDeliveryCacheStorageService(u64, pid) -> object<nn::bcat::detail::ipc::IDeliveryCacheStorageService>
        public long CreateDeliveryCacheStorageService(ServiceCtx context)
        {
            long id = context.RequestData.ReadInt64();

            MakeObject(context, new IDeliveryCacheStorageService());

            return 0;
        }
    }
}
