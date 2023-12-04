using Ryujinx.HLE.HOS.Kernel.Process;

namespace Ryujinx.HLE.HOS.Services.Pm
{
    [Service("pm:info")]
    class IInformationInterface : IpcService
    {
        public IInformationInterface(ServiceCtx context) { }

        [CommandCmif(0)]
        // GetProgramId(os::ProcessId process_id) -> sf::Out<ncm::ProgramId> out
        public ResultCode GetProgramId(ServiceCtx context)
        {
            ulong pid = context.RequestData.ReadUInt64();

            // TODO: Not correct as it shouldn't be directly using kernel objects here
            if (context.Device.System.KernelContext.Processes.TryGetValue(pid, out KProcess process))
            {
                context.ResponseData.Write(process.TitleId);

                return ResultCode.Success;
            }

            return ResultCode.ProcessNotFound;
        }
    }
}
