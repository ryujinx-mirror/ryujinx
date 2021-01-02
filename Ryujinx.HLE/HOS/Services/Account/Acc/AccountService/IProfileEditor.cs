namespace Ryujinx.HLE.HOS.Services.Account.Acc.AccountService
{
    class IProfileEditor : IpcService
    {
        private ProfileServer _profileServer;

        public IProfileEditor(UserProfile profile)
        {
            _profileServer = new ProfileServer(profile);
        }

        [Command(0)]
        // Get() -> (nn::account::profile::ProfileBase, buffer<nn::account::profile::UserData, 0x1a>)
        public ResultCode Get(ServiceCtx context)
        {
            return _profileServer.Get(context);
        }

        [Command(1)]
        // GetBase() -> nn::account::profile::ProfileBase
        public ResultCode GetBase(ServiceCtx context)
        {
            return _profileServer.GetBase(context);
        }

        [Command(10)]
        // GetImageSize() -> u32
        public ResultCode GetImageSize(ServiceCtx context)
        {
            return _profileServer.GetImageSize(context);
        }

        [Command(11)]
        // LoadImage() -> (u32, buffer<bytes, 6>)
        public ResultCode LoadImage(ServiceCtx context)
        {
            return _profileServer.LoadImage(context);
        }

        [Command(100)]
        // Store(nn::account::profile::ProfileBase, buffer<nn::account::profile::UserData, 0x19>)
        public ResultCode Store(ServiceCtx context)
        {
            return _profileServer.Store(context);
        }

        [Command(101)]
        // StoreWithImage(nn::account::profile::ProfileBase, buffer<nn::account::profile::UserData, 0x19>, buffer<bytes, 5>)
        public ResultCode StoreWithImage(ServiceCtx context)
        {
            return _profileServer.StoreWithImage(context);
        }
    }
}