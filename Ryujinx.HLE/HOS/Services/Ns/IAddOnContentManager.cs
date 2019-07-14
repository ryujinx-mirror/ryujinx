using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    [Service("aoc:u")]
    class IAddOnContentManager : IpcService
    {
        public IAddOnContentManager(ServiceCtx context) { }

        [Command(2)]
        // CountAddOnContent(u64, pid) -> u32
        public static ResultCode CountAddOnContent(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }

        [Command(3)]
        // ListAddOnContent(u32, u32, u64, pid) -> (u32, buffer<u32, 6>)
        public static ResultCode ListAddOnContent(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNs);

            // TODO: This is supposed to write a u32 array aswell.
            // It's unknown what it contains.
            context.ResponseData.Write(0);

            return ResultCode.Success;
        }
    }
}