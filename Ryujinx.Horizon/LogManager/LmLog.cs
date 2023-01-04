using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Lm;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.LogManager
{
    partial class LmLog : IServiceObject
    {
        public LogDestination LogDestination { get; set; } = LogDestination.TargetManager;

        [CmifCommand(0)]
        public Result OpenLogger(out LmLogger logger, [ClientProcessId] ulong clientProcessId)
        {
            logger = new LmLogger(this, clientProcessId);

            return Result.Success;
        }
    }
}
