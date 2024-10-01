using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Horizon.Common;

namespace Ryujinx.HLE.HOS.Services.Pm
{
    [Service("pm:dmnt")]
    class IDebugMonitorInterface : IpcService
    {
        public IDebugMonitorInterface(ServiceCtx context) { }

        [CommandCmif(4)]
        // GetProgramId() -> sf::Out<ncm::ProgramId> out_process_id
        public ResultCode GetApplicationProcessId(ServiceCtx context)
        {
            // TODO: Not correct as it shouldn't be directly using kernel objects here
            foreach (KProcess process in context.Device.System.KernelContext.Processes.Values)
            {
                if (process.IsApplication)
                {
                    context.ResponseData.Write(process.Pid);

                    return ResultCode.Success;
                }
            }

            return ResultCode.ProcessNotFound;
        }

        [CommandCmif(65000)]
        // AtmosphereGetProcessInfo(os::ProcessId process_id) -> sf::OutCopyHandle out_process_handle, sf::Out<ncm::ProgramLocation> out_loc, sf::Out<cfg::OverrideStatus> out_status
        public ResultCode GetProcessInfo(ServiceCtx context)
        {
            ulong pid = context.RequestData.ReadUInt64();

            KProcess process = KernelStatic.GetProcessByPid(pid);

            if (context.Process.HandleTable.GenerateHandle(process, out int processHandle) != Result.Success)
            {
                throw new System.Exception("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(processHandle);

            return ResultCode.Success;
        }
    }
}
