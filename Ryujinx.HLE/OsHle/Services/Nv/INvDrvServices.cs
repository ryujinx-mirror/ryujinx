using ChocolArm64.Memory;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.OsHle.Ipc;
using Ryujinx.HLE.OsHle.Services.Nv.NvGpuAS;
using Ryujinx.HLE.OsHle.Services.Nv.NvGpuGpu;
using Ryujinx.HLE.OsHle.Services.Nv.NvHostChannel;
using Ryujinx.HLE.OsHle.Services.Nv.NvHostCtrl;
using Ryujinx.HLE.OsHle.Services.Nv.NvMap;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Nv
{
    class INvDrvServices : IpcService, IDisposable
    {
        private delegate int IoctlProcessor(ServiceCtx Context, int Cmd);

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private static Dictionary<string, IoctlProcessor> IoctlProcessors =
                   new Dictionary<string, IoctlProcessor>()
        {
            { "/dev/nvhost-as-gpu",   ProcessIoctlNvGpuAS    },
            { "/dev/nvhost-ctrl",     ProcessIoctlNvHostCtrl },
            { "/dev/nvhost-ctrl-gpu", ProcessIoctlNvGpuGpu   },
            { "/dev/nvhost-gpu",      ProcessIoctlNvHostGpu  },
            { "/dev/nvmap",           ProcessIoctlNvMap      }
        };

        public static GlobalStateTable Fds { get; private set; }

        private KEvent Event;

        public INvDrvServices()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,  Open             },
                { 1,  Ioctl            },
                { 2,  Close            },
                { 3,  Initialize       },
                { 4,  QueryEvent       },
                { 8,  SetClientPid     },
                { 11, Ioctl            },
                { 13, FinishInitialize }
            };

            Event = new KEvent();
        }

        static INvDrvServices()
        {
            Fds = new GlobalStateTable();
        }

        public long Open(ServiceCtx Context)
        {
            long NamePtr = Context.Request.SendBuff[0].Position;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, NamePtr);

            int Fd = Fds.Add(Context.Process, new NvFd(Name));

            Context.ResponseData.Write(Fd);
            Context.ResponseData.Write(0);

            return 0;
        }

        public long Ioctl(ServiceCtx Context)
        {
            int Fd  = Context.RequestData.ReadInt32();
            int Cmd = Context.RequestData.ReadInt32();

            NvFd FdData = Fds.GetData<NvFd>(Context.Process, Fd);

            int Result;

            if (IoctlProcessors.TryGetValue(FdData.Name, out IoctlProcessor Process))
            {
                Result = Process(Context, Cmd);
            }
            else
            {
                throw new NotImplementedException($"{FdData.Name} {Cmd:x4}");
            }

            //TODO: Verify if the error codes needs to be translated.
            Context.ResponseData.Write(Result);

            return 0;
        }

        public long Close(ServiceCtx Context)
        {
            int Fd = Context.RequestData.ReadInt32();

            Fds.Delete(Context.Process, Fd);

            Context.ResponseData.Write(0);

            return 0;
        }

        public long Initialize(ServiceCtx Context)
        {
            long TransferMemSize   = Context.RequestData.ReadInt64();
            int  TransferMemHandle = Context.Request.HandleDesc.ToCopy[0];

            NvMapIoctl.InitializeNvMap(Context);

            Context.ResponseData.Write(0);

            return 0;
        }

        public long QueryEvent(ServiceCtx Context)
        {
            int Fd      = Context.RequestData.ReadInt32();
            int EventId = Context.RequestData.ReadInt32();

            //TODO: Use Fd/EventId, different channels have different events.
            int Handle = Context.Process.HandleTable.OpenHandle(Event);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.ResponseData.Write(0);

            return 0;
        }

        public long SetClientPid(ServiceCtx Context)
        {
            long Pid = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(0);

            return 0;
        }

        public long FinishInitialize(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return 0;
        }

        private static int ProcessIoctlNvGpuAS(ServiceCtx Context, int Cmd)
        {
            return ProcessIoctl(Context, Cmd, NvGpuASIoctl.ProcessIoctl);
        }

        private static int ProcessIoctlNvHostCtrl(ServiceCtx Context, int Cmd)
        {
            return ProcessIoctl(Context, Cmd, NvHostCtrlIoctl.ProcessIoctl);
        }

        private static int ProcessIoctlNvGpuGpu(ServiceCtx Context, int Cmd)
        {
            return ProcessIoctl(Context, Cmd, NvGpuGpuIoctl.ProcessIoctl);
        }

        private static int ProcessIoctlNvHostGpu(ServiceCtx Context, int Cmd)
        {
            return ProcessIoctl(Context, Cmd, NvHostChannelIoctl.ProcessIoctlGpu);
        }

        private static int ProcessIoctlNvMap(ServiceCtx Context, int Cmd)
        {
            return ProcessIoctl(Context, Cmd, NvMapIoctl.ProcessIoctl);
        }

        private static int ProcessIoctl(ServiceCtx Context, int Cmd, IoctlProcessor Processor)
        {
            if (CmdIn(Cmd) && Context.Request.GetBufferType0x21().Position == 0)
            {
                Context.Ns.Log.PrintError(LogClass.ServiceNv, "Input buffer is null!");

                return NvResult.InvalidInput;
            }

            if (CmdOut(Cmd) && Context.Request.GetBufferType0x22().Position == 0)
            {
                Context.Ns.Log.PrintError(LogClass.ServiceNv, "Output buffer is null!");

                return NvResult.InvalidInput;
            }

            return Processor(Context, Cmd);
        }

        private static bool CmdIn(int Cmd)
        {
            return ((Cmd >> 30) & 1) != 0;
        }

        private static bool CmdOut(int Cmd)
        {
            return ((Cmd >> 31) & 1) != 0;
        }

        public static void UnloadProcess(Process Process)
        {
            Fds.DeleteProcess(Process);

            NvGpuASIoctl.UnloadProcess(Process);

            NvHostChannelIoctl.UnloadProcess(Process);

            NvHostCtrlIoctl.UnloadProcess(Process);

            NvMapIoctl.UnloadProcess(Process);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                Event.Dispose();
            }
        }
    }
}
