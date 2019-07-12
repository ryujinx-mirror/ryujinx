namespace Ryujinx.HLE.HOS.Services.Nifm
{
    [Service("nifm:u")]
    class IStaticService : IpcService
    {
        public IStaticService(ServiceCtx context) { }

        [Command(4)]
        // CreateGeneralServiceOld() -> object<nn::nifm::detail::IGeneralService>
        public long CreateGeneralServiceOld(ServiceCtx context)
        {
            MakeObject(context, new IGeneralService());

            return 0;
        }

        [Command(5)] // 3.0.0+
        // CreateGeneralService(u64, pid) -> object<nn::nifm::detail::IGeneralService>
        public long CreateGeneralService(ServiceCtx context)
        {
            MakeObject(context, new IGeneralService());

            return 0;
        }
    }
}