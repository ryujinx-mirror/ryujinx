using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;

namespace Ryujinx.HLE.HOS.Services.Pm
{
    [Service("pm:dmnt")]
    class IDebugMonitorInterface : IpcService
    {
        public IDebugMonitorInterface(ServiceCtx context) { }

        [CommandHipc(65000)]
        // AtmosphereGetProcessInfo(os::ProcessId process_id) -> sf::OutCopyHandle out_process_handle, sf::Out<ncm::ProgramLocation> out_loc, sf::Out<cfg::OverrideStatus> out_status
        public ResultCode GetProcessInfo(ServiceCtx context)
        {
            ulong pid = context.RequestData.ReadUInt64();

            KProcess process = KernelStatic.GetProcessByPid(pid);

            if (context.Process.HandleTable.GenerateHandle(process, out int processHandle) != KernelResult.Success)
            {
                throw new System.Exception("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(processHandle);

            return ResultCode.Success;
        }
    }
}