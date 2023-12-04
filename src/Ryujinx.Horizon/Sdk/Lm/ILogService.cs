using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.LogManager.Ipc;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Lm
{
    interface ILogService : IServiceObject
    {
        Result OpenLogger(out LmLogger logger, ulong pid);
    }
}
