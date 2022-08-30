namespace Ryujinx.HLE.HOS.Services.Account.Acc.AccountService
{
    class IManagerForApplication : IpcService
    {
        private ManagerServer _managerServer;

        public IManagerForApplication(UserId userId)
        {
            _managerServer = new ManagerServer(userId);
        }

        [CommandHipc(0)]
        // CheckAvailability()
        public ResultCode CheckAvailability(ServiceCtx context)
        {
            return _managerServer.CheckAvailability(context);
        }

        [CommandHipc(1)]
        // GetAccountId() -> nn::account::NetworkServiceAccountId
        public ResultCode GetAccountId(ServiceCtx context)
        {
            return _managerServer.GetAccountId(context);
        }

        [CommandHipc(2)]
        // EnsureIdTokenCacheAsync() -> object<nn::account::detail::IAsyncContext>
        public ResultCode EnsureIdTokenCacheAsync(ServiceCtx context)
        {
            ResultCode resultCode = _managerServer.EnsureIdTokenCacheAsync(context, out IAsyncContext asyncContext);

            if (resultCode == ResultCode.Success)
            {
                MakeObject(context, asyncContext);
            }

            return resultCode;
        }

        [CommandHipc(3)]
        // LoadIdTokenCache() -> (u32 id_token_cache_size, buffer<bytes, 6>)
        public ResultCode LoadIdTokenCache(ServiceCtx context)
        {
            return _managerServer.LoadIdTokenCache(context);
        }

        [CommandHipc(130)]
        // GetNintendoAccountUserResourceCacheForApplication() -> (nn::account::NintendoAccountId, nn::account::nas::NasUserBaseForApplication, buffer<bytes, 6>)
        public ResultCode GetNintendoAccountUserResourceCacheForApplication(ServiceCtx context)
        {
            return _managerServer.GetNintendoAccountUserResourceCacheForApplication(context);
        }

        [CommandHipc(160)] // 5.0.0+
        // StoreOpenContext()
        public ResultCode StoreOpenContext(ServiceCtx context)
        {
            return _managerServer.StoreOpenContext(context);
        }

        [CommandHipc(170)] // 6.0.0+
        // LoadNetworkServiceLicenseKindAsync() -> object<nn::account::detail::IAsyncNetworkServiceLicenseKindContext>
        public ResultCode LoadNetworkServiceLicenseKindAsync(ServiceCtx context)
        {
            ResultCode resultCode = _managerServer.LoadNetworkServiceLicenseKindAsync(context, out IAsyncNetworkServiceLicenseKindContext asyncContext);

            if (resultCode == ResultCode.Success)
            {
                MakeObject(context, asyncContext);
            }

            return resultCode;
        }
    }
}