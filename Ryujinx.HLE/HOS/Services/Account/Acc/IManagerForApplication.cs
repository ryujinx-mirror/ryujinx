using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Account.Acc.AsyncContext;
using Ryujinx.HLE.HOS.Services.Arp;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    class IManagerForApplication : IpcService
    {
        // TODO: Determine where and how NetworkServiceAccountId is set.
        private const long NetworkServiceAccountId = 0xcafe;

        private UserId                    _userId;
        private ApplicationLaunchProperty _applicationLaunchProperty;

        public IManagerForApplication(UserId userId, ApplicationLaunchProperty applicationLaunchProperty)
        {
            _userId                    = userId;
            _applicationLaunchProperty = applicationLaunchProperty;
        }

        [Command(0)]
        // CheckAvailability()
        public ResultCode CheckAvailability(ServiceCtx context)
        {
            // NOTE: This opens the file at "su/baas/USERID_IN_UUID_STRING.dat" where USERID_IN_UUID_STRING is formatted as "%08x-%04x-%04x-%02x%02x-%08x%04x".
            //       Then it searches the Availability of Online Services related to the UserId in this file and returns it.

            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            // NOTE: Even if we try to return different error codes here, the guest still needs other calls.
            return ResultCode.Success;
        }

        [Command(1)]
        // GetAccountId() -> nn::account::NetworkServiceAccountId
        public ResultCode GetAccountId(ServiceCtx context)
        {
            // NOTE: This opens the file at "su/baas/USERID_IN_UUID_STRING.dat" (where USERID_IN_UUID_STRING is formatted 
            //       as "%08x-%04x-%04x-%02x%02x-%08x%04x") in the account:/ savedata.
            //       Then it searches the NetworkServiceAccountId related to the UserId in this file and returns it.

            Logger.Stub?.PrintStub(LogClass.ServiceAcc, new { NetworkServiceAccountId });

            context.ResponseData.Write(NetworkServiceAccountId);

            return ResultCode.Success;
        }

        [Command(2)]
        // EnsureIdTokenCacheAsync() -> object<nn::account::detail::IAsyncContext>
        public ResultCode EnsureIdTokenCacheAsync(ServiceCtx context)
        {
            KEvent         asyncEvent     = new KEvent(context.Device.System.KernelContext);
            AsyncExecution asyncExecution = new AsyncExecution(asyncEvent);

            asyncExecution.Initialize(1000, EnsureIdTokenCacheAsyncImpl);

            MakeObject(context, new IAsyncContext(asyncExecution));

            // return ResultCode.NullObject if the IAsyncContext pointer is null. Doesn't occur in our case.

            return ResultCode.Success;
        }

        private async Task EnsureIdTokenCacheAsyncImpl(CancellationToken token)
        {
            // NOTE: This open the file at "su/baas/USERID_IN_UUID_STRING.dat" (where USERID_IN_UUID_STRING is formatted as "%08x-%04x-%04x-%02x%02x-%08x%04x")
            //       in the "account:/" savedata.
            //       Then its read data, use dauth API with this data to get the Token Id and probably store the dauth response
            //       in "su/cache/USERID_IN_UUID_STRING.dat" (where USERID_IN_UUID_STRING is formatted as "%08x-%04x-%04x-%02x%02x-%08x%04x") in the "account:/" savedata.
            //       Since we don't support online services, we can stub it.

            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            // TODO: Use a real function instead, with the CancellationToken.
            await Task.CompletedTask;
        }

        [Command(3)]
        // LoadIdTokenCache() -> (u32 id_token_cache_size, buffer<bytes, 6>)
        public ResultCode LoadIdTokenCache(ServiceCtx context)
        {
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferSize     = context.Request.ReceiveBuff[0].Size;

            // NOTE: This opens the file at "su/cache/USERID_IN_UUID_STRING.dat" (where USERID_IN_UUID_STRING is formatted as "%08x-%04x-%04x-%02x%02x-%08x%04x")
            //       in the "account:/" savedata and writes some data in the buffer.
            //       Since we don't support online services, we can stub it.

            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            /*
            if (internal_object != null)
            {
                if (bufferSize > 0xC00)
                {
                    return ResultCode.InvalidIdTokenCacheBufferSize;
                }
            }
            */

            int idTokenCacheSize = 0;

            MemoryHelper.FillWithZeros(context.Memory, bufferPosition, (int)bufferSize);

            context.ResponseData.Write(idTokenCacheSize);

            return ResultCode.Success;
        }

        [Command(130)]
        // GetNintendoAccountUserResourceCacheForApplication() -> (nn::account::NintendoAccountId, buffer<nn::account::nas::NasUserBaseForApplication, 0x1a>, buffer<bytes, 6>)
        public ResultCode GetNintendoAccountUserResourceCacheForApplication(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAcc, new { NetworkServiceAccountId });

            context.ResponseData.Write(NetworkServiceAccountId);

            // TODO: determine and fill the two output IPC buffers.

            return ResultCode.Success;
        }

        [Command(160)] // 5.0.0+
        // StoreOpenContext()
        public ResultCode StoreOpenContext(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }
    }
}