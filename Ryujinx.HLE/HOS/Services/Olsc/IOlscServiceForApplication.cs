using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Account.Acc;

namespace Ryujinx.HLE.HOS.Services.Olsc
{
    [Service("olsc:u")] // 10.0.0+
    class IOlscServiceForApplication : IpcService
    {
        private bool _initialized;

        public IOlscServiceForApplication(ServiceCtx context) { }

        [Command(0)]
        // Initialize(pid)
        public ResultCode Initialize(ServiceCtx context)
        {
            // NOTE: Service call arp:r GetApplicationInstanceUnregistrationNotifier with the pid and initialize some internal struct.
            //       Since we will not support online savedata backup. It's fine to stub it for now.

            _initialized = true;

            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);

            return ResultCode.Success;
        }

        [Command(14)]
        // SetSaveDataBackupSettingEnabled(nn::account::Uid, bool)
        public ResultCode SetSaveDataBackupSettingEnabled(ServiceCtx context)
        {
            UserId userId                       = context.RequestData.ReadStruct<UserId>();
            ulong  saveDataBackupSettingEnabled = context.RequestData.ReadUInt64();

            if (!_initialized)
            {
                return ResultCode.NotInitialized;
            }

            if (userId.IsNull)
            {
                return ResultCode.NullArgument;
            }

            // NOTE: Service store the UserId and the boolean in an internal SaveDataBackupSettingDatabase object.
            //       Since we will not support online savedata backup. It's fine to stub it for now.

            Logger.Stub?.PrintStub(LogClass.ServiceOlsc, new { userId, saveDataBackupSettingEnabled });

            return ResultCode.Success;
        }
    }
}