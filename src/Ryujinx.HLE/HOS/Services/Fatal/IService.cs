using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Fatal.Types;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Fatal
{
    [Service("fatal:u")]
    class IService : IpcService
    {
        public IService(ServiceCtx context) { }

        [CommandCmif(0)]
        // ThrowFatal(u64 result_code, u64 pid)
        public ResultCode ThrowFatal(ServiceCtx context)
        {
            ResultCode resultCode = (ResultCode)context.RequestData.ReadUInt64();
            ulong pid = context.Request.HandleDesc.PId;

            return ThrowFatalWithCpuContextImpl(context, resultCode, pid, FatalPolicy.ErrorReportAndErrorScreen, null);
        }

        [CommandCmif(1)]
        // ThrowFatalWithPolicy(u64 result_code, u32 fatal_policy, u64 pid)
        public ResultCode ThrowFatalWithPolicy(ServiceCtx context)
        {
            ResultCode resultCode = (ResultCode)context.RequestData.ReadUInt64();
            FatalPolicy fatalPolicy = (FatalPolicy)context.RequestData.ReadUInt32();
            ulong pid = context.Request.HandleDesc.PId;

            return ThrowFatalWithCpuContextImpl(context, resultCode, pid, fatalPolicy, null);
        }

        [CommandCmif(2)]
        // ThrowFatalWithCpuContext(u64 result_code, u32 fatal_policy, u64 pid, buffer<bytes, 0x15> cpu_context)
        public ResultCode ThrowFatalWithCpuContext(ServiceCtx context)
        {
            ResultCode resultCode = (ResultCode)context.RequestData.ReadUInt64();
            FatalPolicy fatalPolicy = (FatalPolicy)context.RequestData.ReadUInt32();
            ulong pid = context.Request.HandleDesc.PId;

            ulong cpuContextPosition = context.Request.SendBuff[0].Position;
            ulong cpuContextSize = context.Request.SendBuff[0].Size;

            ReadOnlySpan<byte> cpuContextData = context.Memory.GetSpan(cpuContextPosition, (int)cpuContextSize);

            return ThrowFatalWithCpuContextImpl(context, resultCode, pid, fatalPolicy, cpuContextData);
        }

        private ResultCode ThrowFatalWithCpuContextImpl(ServiceCtx context, ResultCode resultCode, ulong pid, FatalPolicy fatalPolicy, ReadOnlySpan<byte> cpuContext)
        {
            StringBuilder errorReport = new();

            errorReport.AppendLine();
            errorReport.AppendLine("ErrorReport log:");

            errorReport.AppendLine($"\tTitleId: {context.Device.Processes.ActiveApplication.ProgramIdText}");
            errorReport.AppendLine($"\tPid: {pid}");
            errorReport.AppendLine($"\tResultCode: {((int)resultCode & 0x1FF) + 2000}-{((int)resultCode >> 9) & 0x3FFF:d4}");
            errorReport.AppendLine($"\tFatalPolicy: {fatalPolicy}");

            if (!cpuContext.IsEmpty)
            {
                errorReport.AppendLine("CPU Context:");

                if (context.Device.Processes.ActiveApplication.Is64Bit)
                {
                    CpuContext64 cpuContext64 = MemoryMarshal.Cast<byte, CpuContext64>(cpuContext)[0];

                    errorReport.AppendLine($"\tStartAddress: 0x{cpuContext64.StartAddress:x16}");
                    errorReport.AppendLine($"\tRegisterSetFlags: {cpuContext64.RegisterSetFlags}");

                    if (cpuContext64.StackTraceSize > 0)
                    {
                        errorReport.AppendLine("\tStackTrace:");

                        for (int i = 0; i < cpuContext64.StackTraceSize; i++)
                        {
                            errorReport.AppendLine($"\t\t0x{cpuContext64.StackTrace[i]:x16}");
                        }
                    }

                    errorReport.AppendLine("\tRegisters:");

                    for (int i = 0; i < cpuContext64.X.Length; i++)
                    {
                        errorReport.AppendLine($"\t\tX[{i:d2}]:\t0x{cpuContext64.X[i]:x16}");
                    }

                    errorReport.AppendLine();
                    errorReport.AppendLine($"\t\tFP:\t0x{cpuContext64.FP:x16}");
                    errorReport.AppendLine($"\t\tLR:\t0x{cpuContext64.LR:x16}");
                    errorReport.AppendLine($"\t\tSP:\t0x{cpuContext64.SP:x16}");
                    errorReport.AppendLine($"\t\tPC:\t0x{cpuContext64.PC:x16}");
                    errorReport.AppendLine($"\t\tPState:\t0x{cpuContext64.PState:x16}");
                    errorReport.AppendLine($"\t\tAfsr0:\t0x{cpuContext64.Afsr0:x16}");
                    errorReport.AppendLine($"\t\tAfsr1:\t0x{cpuContext64.Afsr1:x16}");
                    errorReport.AppendLine($"\t\tEsr:\t0x{cpuContext64.Esr:x16}");
                    errorReport.AppendLine($"\t\tFar:\t0x{cpuContext64.Far:x16}");
                }
                else
                {
                    CpuContext32 cpuContext32 = MemoryMarshal.Cast<byte, CpuContext32>(cpuContext)[0];

                    errorReport.AppendLine($"\tStartAddress: 0x{cpuContext32.StartAddress:16}");
                    errorReport.AppendLine($"\tRegisterSetFlags: {cpuContext32.RegisterSetFlags}");

                    if (cpuContext32.StackTraceSize > 0)
                    {
                        errorReport.AppendLine("\tStackTrace:");

                        for (int i = 0; i < cpuContext32.StackTraceSize; i++)
                        {
                            errorReport.AppendLine($"\t\t0x{cpuContext32.StackTrace[i]:x16}");
                        }
                    }

                    errorReport.AppendLine("\tRegisters:");

                    for (int i = 0; i < cpuContext32.X.Length; i++)
                    {
                        errorReport.AppendLine($"\t\tX[{i:d2}]:\t0x{cpuContext32.X[i]:x16}");
                    }

                    errorReport.AppendLine();
                    errorReport.AppendLine($"\t\tFP:\t0x{cpuContext32.FP:x16}");
                    errorReport.AppendLine($"\t\tFP:\t0x{cpuContext32.IP:x16}");
                    errorReport.AppendLine($"\t\tSP:\t0x{cpuContext32.SP:x16}");
                    errorReport.AppendLine($"\t\tLR:\t0x{cpuContext32.LR:x16}");
                    errorReport.AppendLine($"\t\tPC:\t0x{cpuContext32.PC:x16}");
                    errorReport.AppendLine($"\t\tPState:\t0x{cpuContext32.PState:x16}");
                    errorReport.AppendLine($"\t\tAfsr0:\t0x{cpuContext32.Afsr0:x16}");
                    errorReport.AppendLine($"\t\tAfsr1:\t0x{cpuContext32.Afsr1:x16}");
                    errorReport.AppendLine($"\t\tEsr:\t0x{cpuContext32.Esr:x16}");
                    errorReport.AppendLine($"\t\tFar:\t0x{cpuContext32.Far:x16}");
                }
            }

            Logger.Info?.Print(LogClass.ServiceFatal, errorReport.ToString());

            context.Device.System.KernelContext.Syscall.Break((ulong)resultCode);

            return ResultCode.Success;
        }
    }
}
