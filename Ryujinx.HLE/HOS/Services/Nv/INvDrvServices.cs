using ChocolArm64.Memory;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nv.NvGpuAS;
using Ryujinx.HLE.HOS.Services.Nv.NvGpuGpu;
using Ryujinx.HLE.HOS.Services.Nv.NvHostChannel;
using Ryujinx.HLE.HOS.Services.Nv.NvHostCtrl;
using Ryujinx.HLE.HOS.Services.Nv.NvMap;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nv
{
    class INvDrvServices : IpcService
    {
        private delegate int IoctlProcessor(ServiceCtx context, int cmd);

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private static Dictionary<string, IoctlProcessor> _ioctlProcessors =
                   new Dictionary<string, IoctlProcessor>()
        {
            { "/dev/nvhost-as-gpu",   ProcessIoctlNvGpuAS       },
            { "/dev/nvhost-ctrl",     ProcessIoctlNvHostCtrl    },
            { "/dev/nvhost-ctrl-gpu", ProcessIoctlNvGpuGpu      },
            { "/dev/nvhost-gpu",      ProcessIoctlNvHostChannel },
            { "/dev/nvhost-nvdec",    ProcessIoctlNvHostChannel },
            { "/dev/nvhost-vic",      ProcessIoctlNvHostChannel },
            { "/dev/nvmap",           ProcessIoctlNvMap         }
        };

        public static GlobalStateTable Fds { get; private set; }

        private KEvent _event;

        public INvDrvServices(Horizon system)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,  Open                   },
                { 1,  Ioctl                  },
                { 2,  Close                  },
                { 3,  Initialize             },
                { 4,  QueryEvent             },
                { 8,  SetClientPid           },
                { 9,  DumpGraphicsMemoryInfo },
                { 11, Ioctl                  },
                { 13, FinishInitialize       }
            };

            _event = new KEvent(system);
        }

        static INvDrvServices()
        {
            Fds = new GlobalStateTable();
        }

        public long Open(ServiceCtx context)
        {
            long namePtr = context.Request.SendBuff[0].Position;

            string name = MemoryHelper.ReadAsciiString(context.Memory, namePtr);

            int fd = Fds.Add(context.Process, new NvFd(name));

            context.ResponseData.Write(fd);
            context.ResponseData.Write(0);

            return 0;
        }

        public long Ioctl(ServiceCtx context)
        {
            int fd  = context.RequestData.ReadInt32();
            int cmd = context.RequestData.ReadInt32();

            NvFd fdData = Fds.GetData<NvFd>(context.Process, fd);

            int result = 0;

            if (_ioctlProcessors.TryGetValue(fdData.Name, out IoctlProcessor process))
            {
                result = process(context, cmd);
            }
            else if (!ServiceConfiguration.IgnoreMissingServices)
            {
                throw new NotImplementedException($"{fdData.Name} {cmd:x4}");
            }

            //TODO: Verify if the error codes needs to be translated.
            context.ResponseData.Write(result);

            return 0;
        }

        public long Close(ServiceCtx context)
        {
            int fd = context.RequestData.ReadInt32();

            Fds.Delete(context.Process, fd);

            context.ResponseData.Write(0);

            return 0;
        }

        public long Initialize(ServiceCtx context)
        {
            long transferMemSize   = context.RequestData.ReadInt64();
            int  transferMemHandle = context.Request.HandleDesc.ToCopy[0];

            NvMapIoctl.InitializeNvMap(context);

            context.ResponseData.Write(0);

            return 0;
        }

        public long QueryEvent(ServiceCtx context)
        {
            int fd      = context.RequestData.ReadInt32();
            int eventId = context.RequestData.ReadInt32();

            //TODO: Use Fd/EventId, different channels have different events.
            if (context.Process.HandleTable.GenerateHandle(_event.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            context.ResponseData.Write(0);

            return 0;
        }

        public long SetClientPid(ServiceCtx context)
        {
            long pid = context.RequestData.ReadInt64();

            context.ResponseData.Write(0);

            return 0;
        }

        public long DumpGraphicsMemoryInfo(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return 0;
        }

        public long FinishInitialize(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return 0;
        }

        private static int ProcessIoctlNvGpuAS(ServiceCtx context, int cmd)
        {
            return ProcessIoctl(context, cmd, NvGpuASIoctl.ProcessIoctl);
        }

        private static int ProcessIoctlNvHostCtrl(ServiceCtx context, int cmd)
        {
            return ProcessIoctl(context, cmd, NvHostCtrlIoctl.ProcessIoctl);
        }

        private static int ProcessIoctlNvGpuGpu(ServiceCtx context, int cmd)
        {
            return ProcessIoctl(context, cmd, NvGpuGpuIoctl.ProcessIoctl);
        }

        private static int ProcessIoctlNvHostChannel(ServiceCtx context, int cmd)
        {
            return ProcessIoctl(context, cmd, NvHostChannelIoctl.ProcessIoctl);
        }

        private static int ProcessIoctlNvMap(ServiceCtx context, int cmd)
        {
            return ProcessIoctl(context, cmd, NvMapIoctl.ProcessIoctl);
        }

        private static int ProcessIoctl(ServiceCtx context, int cmd, IoctlProcessor processor)
        {
            if (CmdIn(cmd) && context.Request.GetBufferType0x21().Position == 0)
            {
                Logger.PrintError(LogClass.ServiceNv, "Input buffer is null!");

                return NvResult.InvalidInput;
            }

            if (CmdOut(cmd) && context.Request.GetBufferType0x22().Position == 0)
            {
                Logger.PrintError(LogClass.ServiceNv, "Output buffer is null!");

                return NvResult.InvalidInput;
            }

            return processor(context, cmd);
        }

        private static bool CmdIn(int cmd)
        {
            return ((cmd >> 30) & 1) != 0;
        }

        private static bool CmdOut(int cmd)
        {
            return ((cmd >> 31) & 1) != 0;
        }

        public static void UnloadProcess(KProcess process)
        {
            Fds.DeleteProcess(process);

            NvGpuASIoctl.UnloadProcess(process);

            NvHostChannelIoctl.UnloadProcess(process);

            NvHostCtrlIoctl.UnloadProcess(process);

            NvMapIoctl.UnloadProcess(process);
        }
    }
}
