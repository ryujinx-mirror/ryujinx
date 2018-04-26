using ChocolArm64.Memory;
using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.OsHle.Utilities;
using Ryujinx.Graphics.Gpu;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Core.OsHle.Services.Nv
{
    class INvDrvServices : IpcService, IDisposable
    {
        private delegate long ServiceProcessIoctl(ServiceCtx Context);

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private Dictionary<(string, int), ServiceProcessIoctl> IoctlCmds;

        public static GlobalStateTable Fds { get; private set; }

        public static GlobalStateTable NvMaps     { get; private set; }
        public static GlobalStateTable NvMapsById { get; private set; }
        public static GlobalStateTable NvMapsFb   { get; private set; }

        private KEvent Event;

        public INvDrvServices()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Open         },
                { 1, Ioctl        },
                { 2, Close        },
                { 3, Initialize   },
                { 4, QueryEvent   },
                { 8, SetClientPid },
            };

            IoctlCmds = new Dictionary<(string, int), ServiceProcessIoctl>()
            {
                { ("/dev/nvhost-as-gpu",   0x4101), NvGpuAsIoctlBindChannel           },
                { ("/dev/nvhost-as-gpu",   0x4102), NvGpuAsIoctlAllocSpace            },
                { ("/dev/nvhost-as-gpu",   0x4105), NvGpuAsIoctlUnmap                 },
                { ("/dev/nvhost-as-gpu",   0x4106), NvGpuAsIoctlMapBufferEx           },
                { ("/dev/nvhost-as-gpu",   0x4108), NvGpuAsIoctlGetVaRegions          },
                { ("/dev/nvhost-as-gpu",   0x4109), NvGpuAsIoctlInitializeEx          },
                { ("/dev/nvhost-as-gpu",   0x4114), NvGpuAsIoctlRemap                 },
                { ("/dev/nvhost-ctrl",     0x001b), NvHostIoctlCtrlGetConfig          },
                { ("/dev/nvhost-ctrl",     0x001d), NvHostIoctlCtrlEventWait          },
                { ("/dev/nvhost-ctrl-gpu", 0x4701), NvGpuIoctlZcullGetCtxSize         },
                { ("/dev/nvhost-ctrl-gpu", 0x4702), NvGpuIoctlZcullGetInfo            },
                { ("/dev/nvhost-ctrl-gpu", 0x4703), NvGpuIoctlZbcSetTable             },
                { ("/dev/nvhost-ctrl-gpu", 0x4705), NvGpuIoctlGetCharacteristics      },
                { ("/dev/nvhost-ctrl-gpu", 0x4706), NvGpuIoctlGetTpcMasks             },
                { ("/dev/nvhost-ctrl-gpu", 0x4714), NvGpuIoctlZbcGetActiveSlotMask    },
                { ("/dev/nvhost-gpu",      0x4714), NvMapIoctlChannelSetUserData      },
                { ("/dev/nvhost-gpu",      0x4801), NvMapIoctlChannelSetNvMap         },
                { ("/dev/nvhost-gpu",      0x4808), NvMapIoctlChannelSubmitGpFifo     },
                { ("/dev/nvhost-gpu",      0x4809), NvMapIoctlChannelAllocObjCtx      },
                { ("/dev/nvhost-gpu",      0x480b), NvMapIoctlChannelZcullBind        },
                { ("/dev/nvhost-gpu",      0x480c), NvMapIoctlChannelSetErrorNotifier },
                { ("/dev/nvhost-gpu",      0x480d), NvMapIoctlChannelSetPriority      },
                { ("/dev/nvhost-gpu",      0x481a), NvMapIoctlChannelAllocGpFifoEx2   },
                { ("/dev/nvmap",           0x0101), NvMapIocCreate                    },
                { ("/dev/nvmap",           0x0103), NvMapIocFromId                    },
                { ("/dev/nvmap",           0x0104), NvMapIocAlloc                     },
                { ("/dev/nvmap",           0x0105), NvMapIocFree                      },
                { ("/dev/nvmap",           0x0109), NvMapIocParam                     },
                { ("/dev/nvmap",           0x010e), NvMapIocGetId                     },
            };

            Event = new KEvent();
        }

        static INvDrvServices()
        {
            Fds = new GlobalStateTable();

            NvMaps     = new GlobalStateTable();
            NvMapsById = new GlobalStateTable();
            NvMapsFb   = new GlobalStateTable();
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
            int Cmd = Context.RequestData.ReadInt32() & 0xffff;

            NvFd FdData = Fds.GetData<NvFd>(Context.Process, Fd);

            long Position = Context.Request.GetSendBuffPtr();

            Context.ResponseData.Write(0);

            if (IoctlCmds.TryGetValue((FdData.Name, Cmd), out ServiceProcessIoctl ProcReq))
            {
                return ProcReq(Context);
            }
            else
            {
                throw new NotImplementedException($"{FdData.Name} {Cmd:x4}");
            }
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

            Context.ResponseData.Write(0);

            NvMapsFb.Add(Context.Process, 0, new NvMapFb());

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

        private long NvGpuAsIoctlBindChannel(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            int Fd = Context.Memory.ReadInt32(Position);

            return 0;
        }

        private long NvGpuAsIoctlAllocSpace(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            int  Pages    = Reader.ReadInt32();
            int  PageSize = Reader.ReadInt32();
            int  Flags    = Reader.ReadInt32();
            int  Padding  = Reader.ReadInt32();
            long Align    = Reader.ReadInt64();

            if ((Flags & 1) != 0)
            {
                Align = Context.Ns.Gpu.ReserveMemory(Align, (long)Pages * PageSize, 1);
            }
            else
            {
                Align = Context.Ns.Gpu.ReserveMemory((long)Pages * PageSize, Align);
            }

            Context.Memory.WriteInt64(Position + 0x10, Align);

            return 0;
        }

        private long NvGpuAsIoctlUnmap(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            long Offset = Reader.ReadInt64();

            Context.Ns.Gpu.MemoryMgr.Unmap(Offset, 0x10000);

            return 0;
        }

        private long NvGpuAsIoctlMapBufferEx(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            int  Flags    = Reader.ReadInt32();
            int  Kind     = Reader.ReadInt32();
            int  Handle   = Reader.ReadInt32();
            int  PageSize = Reader.ReadInt32();
            long BuffAddr = Reader.ReadInt64();
            long MapSize  = Reader.ReadInt64();
            long Offset   = Reader.ReadInt64();

            if (Handle == 0)
            {
                //This is used to store offsets for the Framebuffer(s);
                NvMapFb MapFb = (NvMapFb)NvMapsFb.GetData(Context.Process, 0);

                MapFb.AddBufferOffset(BuffAddr);

                return 0;
            }

            NvMap Map = NvMaps.GetData<NvMap>(Context.Process, Handle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"invalid NvMap Handle {Handle}!");

                return -1; //TODO: Corrent error code.
            }

            if ((Flags & 1) != 0)
            {
                Offset = Context.Ns.Gpu.MapMemory(Map.CpuAddress, Offset, Map.Size);
            }
            else
            {
                Offset = Context.Ns.Gpu.MapMemory(Map.CpuAddress, Map.Size);
            }

            Context.Memory.WriteInt64(Position + 0x20, Offset);

            Map.GpuAddress = Offset;

            return 0;
        }

        private long NvGpuAsIoctlGetVaRegions(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);
            MemWriter Writer = new MemWriter(Context.Memory, Position);

            long Unused   = Reader.ReadInt64();
            int  BuffSize = Reader.ReadInt32();
            int  Padding  = Reader.ReadInt32();

            BuffSize = 0x30;

            Writer.WriteInt64(Unused);
            Writer.WriteInt32(BuffSize);
            Writer.WriteInt32(Padding);

            Writer.WriteInt64(0);
            Writer.WriteInt32(0);
            Writer.WriteInt32(0);
            Writer.WriteInt64(0);

            Writer.WriteInt64(0);
            Writer.WriteInt32(0);
            Writer.WriteInt32(0);
            Writer.WriteInt64(0);

            return 0;
        }

        private long NvGpuAsIoctlInitializeEx(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            int  BigPageSize = Reader.ReadInt32();
            int  AsFd        = Reader.ReadInt32();
            int  Flags       = Reader.ReadInt32();
            int  Reserved    = Reader.ReadInt32();
            long Unknown10   = Reader.ReadInt64();
            long Unknown18   = Reader.ReadInt64();
            long Unknown20   = Reader.ReadInt64();

            return 0;
        }

        private long NvGpuAsIoctlRemap(ServiceCtx Context)
        {
            Context.RequestData.BaseStream.Seek(-4, SeekOrigin.Current);

            int Cmd = Context.RequestData.ReadInt32();

            int Size = (Cmd >> 16) & 0xff;

            int Count = Size / 0x18;

            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            for (int Index = 0; Index < Count; Index++)
            {
                int Flags   = Reader.ReadInt32();
                int Kind    = Reader.ReadInt32();
                int Handle  = Reader.ReadInt32();
                int Padding = Reader.ReadInt32();
                int Offset  = Reader.ReadInt32();
                int Pages   = Reader.ReadInt32();

                NvMap Map = NvMaps.GetData<NvMap>(Context.Process, Handle);

                if (Map == null)
                {
                    Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"invalid NvMap Handle {Handle}!");

                    return -1; //TODO: Corrent error code.
                }

                Context.Ns.Gpu.MapMemory(Map.CpuAddress,
                    (long)(uint)Offset << 16,
                    (long)(uint)Pages  << 16);
            }

            //TODO

            return 0;
        }

        private long NvHostIoctlCtrlGetConfig(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);
            MemWriter Writer = new MemWriter(Context.Memory, Position + 0x82);

            for (int Index = 0; Index < 0x101; Index++)
            {
                Writer.WriteByte(0);
            }

            return 0;
        }

        private long NvHostIoctlCtrlEventWait(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            int SyncPtId  = Reader.ReadInt32();
            int Threshold = Reader.ReadInt32();
            int Timeout   = Reader.ReadInt32();
            int Value     = Reader.ReadInt32();

            Context.Memory.WriteInt32(Position + 0xc, 0xcafe);

            return 0;
        }

        private long NvGpuIoctlZcullGetCtxSize(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            Context.Memory.WriteInt32(Position, 1);

            return 0;
        }

        private long NvGpuIoctlZcullGetInfo(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemWriter Writer = new MemWriter(Context.Memory, Position);

            Writer.WriteInt32(0);
            Writer.WriteInt32(0);
            Writer.WriteInt32(0);
            Writer.WriteInt32(0);
            Writer.WriteInt32(0);
            Writer.WriteInt32(0);
            Writer.WriteInt32(0);
            Writer.WriteInt32(0);
            Writer.WriteInt32(0);
            Writer.WriteInt32(0);

            return 0;
        }

        private long NvGpuIoctlZbcSetTable(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            int[] ColorDs = new int[4];
            int[] ColorL2 = new int[4];

            ColorDs[0] = Reader.ReadInt32();
            ColorDs[1] = Reader.ReadInt32();
            ColorDs[2] = Reader.ReadInt32();
            ColorDs[3] = Reader.ReadInt32();

            ColorL2[0] = Reader.ReadInt32();
            ColorL2[1] = Reader.ReadInt32();
            ColorL2[2] = Reader.ReadInt32();
            ColorL2[3] = Reader.ReadInt32();

            int Depth  = Reader.ReadInt32();
            int Format = Reader.ReadInt32();
            int Type   = Reader.ReadInt32();

            return 0;
        }

        private long NvGpuIoctlGetCharacteristics(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);
            MemWriter Writer = new MemWriter(Context.Memory, Position);

            //Note: We should just ignore the BuffAddr, because official code
            //does __memcpy_device from Position + 0x10 to BuffAddr.
            long BuffSize = Reader.ReadInt64();
            long BuffAddr = Reader.ReadInt64();

            BuffSize = 0xa0;

            Writer.WriteInt64(BuffSize);
            Writer.WriteInt64(BuffAddr);
            Writer.WriteInt32(0x120);  //NVGPU_GPU_ARCH_GM200
            Writer.WriteInt32(0xb);    //NVGPU_GPU_IMPL_GM20B
            Writer.WriteInt32(0xa1);
            Writer.WriteInt32(1);
            Writer.WriteInt64(0x40000);
            Writer.WriteInt64(0);
            Writer.WriteInt32(2);
            Writer.WriteInt32(0x20);   //NVGPU_GPU_BUS_TYPE_AXI
            Writer.WriteInt32(0x20000);
            Writer.WriteInt32(0x20000);
            Writer.WriteInt32(0x1b);
            Writer.WriteInt32(0x30000);
            Writer.WriteInt32(1);
            Writer.WriteInt32(0x503);
            Writer.WriteInt32(0x503);
            Writer.WriteInt32(0x80);
            Writer.WriteInt32(0x28);
            Writer.WriteInt32(0);
            Writer.WriteInt64(0x55);
            Writer.WriteInt32(0x902d); //FERMI_TWOD_A
            Writer.WriteInt32(0xb197); //MAXWELL_B
            Writer.WriteInt32(0xb1c0); //MAXWELL_COMPUTE_B
            Writer.WriteInt32(0xb06f); //MAXWELL_CHANNEL_GPFIFO_A
            Writer.WriteInt32(0xa140); //KEPLER_INLINE_TO_MEMORY_B
            Writer.WriteInt32(0xb0b5); //MAXWELL_DMA_COPY_A
            Writer.WriteInt32(1);
            Writer.WriteInt32(0);
            Writer.WriteInt32(2);
            Writer.WriteInt32(1);
            Writer.WriteInt32(0);
            Writer.WriteInt32(1);
            Writer.WriteInt32(0x21d70);
            Writer.WriteInt32(0);
            Writer.WriteByte((byte)'g');
            Writer.WriteByte((byte)'m');
            Writer.WriteByte((byte)'2');
            Writer.WriteByte((byte)'0');
            Writer.WriteByte((byte)'b');
            Writer.WriteByte((byte)'\0');
            Writer.WriteByte((byte)'\0');
            Writer.WriteByte((byte)'\0');
            Writer.WriteInt64(0);

            return 0;
        }

        private long NvGpuIoctlGetTpcMasks(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            int  MaskBuffSize = Reader.ReadInt32();
            int  Reserved     = Reader.ReadInt32();
            long MaskBuffAddr = Reader.ReadInt64();
            long Unknown      = Reader.ReadInt64();

            return 0;
        }

        private long NvGpuIoctlZbcGetActiveSlotMask(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            Context.Memory.WriteInt32(Position + 0, 7);
            Context.Memory.WriteInt32(Position + 4, 1);

            return 0;
        }

        private long NvMapIoctlChannelSetUserData(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            return 0;
        }

        private long NvMapIoctlChannelSetNvMap(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            int Fd = Context.Memory.ReadInt32(Position);

            return 0;
        }

        private long NvMapIoctlChannelSubmitGpFifo(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);
            MemWriter Writer = new MemWriter(Context.Memory, Position + 0x10);

            long GpFifo   = Reader.ReadInt64();
            int  Count    = Reader.ReadInt32();
            int  Flags    = Reader.ReadInt32();
            int  FenceId  = Reader.ReadInt32();
            int  FenceVal = Reader.ReadInt32();

            for (int Index = 0; Index < Count; Index++)
            {
                long GpFifoHdr = Reader.ReadInt64();

                long GpuAddr = GpFifoHdr & 0xffffffffff;

                int Size = (int)(GpFifoHdr >> 40) & 0x7ffffc;

                long CpuAddr = Context.Ns.Gpu.GetCpuAddr(GpuAddr);

                if (CpuAddr != -1)
                {
                    byte[] Data = AMemoryHelper.ReadBytes(Context.Memory, CpuAddr, Size);

                    NsGpuPBEntry[] PushBuffer = NvGpuPushBuffer.Decode(Data);

                    Context.Ns.Gpu.Fifo.PushBuffer(Context.Memory, PushBuffer);
                }
            }

            Writer.WriteInt32(0);
            Writer.WriteInt32(0);

            return 0;
        }

        private long NvMapIoctlChannelAllocObjCtx(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            int ClassNum = Context.Memory.ReadInt32(Position + 0);
            int Flags    = Context.Memory.ReadInt32(Position + 4);

            Context.Memory.WriteInt32(Position + 8, 0);

            return 0;
        }

        private long NvMapIoctlChannelZcullBind(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            long GpuVa   = Reader.ReadInt64();
            int  Mode    = Reader.ReadInt32();
            int  Padding = Reader.ReadInt32();

            return 0;
        }

        private long NvMapIoctlChannelSetErrorNotifier(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            long Offset  = Reader.ReadInt64();
            long Size    = Reader.ReadInt64();
            int  Mem     = Reader.ReadInt32();
            int  Padding = Reader.ReadInt32();

            return 0;
        }

        private long NvMapIoctlChannelSetPriority(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            int Priority = Context.Memory.ReadInt32(Position);

            return 0;
        }

        private long NvMapIoctlChannelAllocGpFifoEx2(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);
            MemWriter Writer = new MemWriter(Context.Memory, Position + 0xc);

            int  Count     = Reader.ReadInt32();
            int  Flags     = Reader.ReadInt32();
            int  Unknown8  = Reader.ReadInt32();
            long Fence     = Reader.ReadInt64();
            int  Unknown14 = Reader.ReadInt32();
            int  Unknown18 = Reader.ReadInt32();

            Writer.WriteInt32(0);
            Writer.WriteInt32(0);

            return 0;
        }

        private long NvMapIocCreate(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            int Size = Context.Memory.ReadInt32(Position);

            NvMap Map = new NvMap() { Size = Size };

            Map.Handle = NvMaps.Add(Context.Process, Map);

            Map.Id = NvMapsById.Add(Context.Process, Map);

            Context.Memory.WriteInt32(Position + 4, Map.Handle);

            Context.Ns.Log.PrintInfo(LogClass.ServiceNv, $"NvMap {Map.Id} created with size {Size:x8}!");

            return 0;
        }

        private long NvMapIocFromId(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            int Id = Context.Memory.ReadInt32(Position);

            NvMap Map = NvMapsById.GetData<NvMap>(Context.Process, Id);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap Id {Id}!");

                return -1; //TODO: Corrent error code.
            }

            Context.Memory.WriteInt32(Position + 4, Map.Handle);

            return 0;
        }

        private long NvMapIocAlloc(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            int  Handle   =       Reader.ReadInt32();
            int  HeapMask =       Reader.ReadInt32();
            int  Flags    =       Reader.ReadInt32();
            int  Align    =       Reader.ReadInt32();
            byte Kind     = (byte)Reader.ReadInt64();
            long Addr     =       Reader.ReadInt64();

            NvMap Map = NvMaps.GetData<NvMap>(Context.Process, Handle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap Handle {Handle}!");

                return -1; //TODO: Corrent error code.
            }

            Map.CpuAddress = Addr;
            Map.Align      = Align;
            Map.Kind       = Kind;

            return 0;
        }

        private long NvMapIocFree(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);
            MemWriter Writer = new MemWriter(Context.Memory, Position + 8);

            int Handle  = Reader.ReadInt32();
            int Padding = Reader.ReadInt32();

            NvMap Map = NvMaps.GetData<NvMap>(Context.Process, Handle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap Handle {Handle}!");

                return -1; //TODO: Corrent error code.
            }

            Writer.WriteInt64(0);
            Writer.WriteInt32(Map.Size);
            Writer.WriteInt32(0);

            return 0;
        }

        private long NvMapIocParam(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            MemReader Reader = new MemReader(Context.Memory, Position);

            int Handle = Reader.ReadInt32();
            int Param  = Reader.ReadInt32();

            NvMap Map = NvMaps.GetData<NvMap>(Context.Process, Handle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap Handle {Handle}!");

                return -1; //TODO: Corrent error code.
            }

            int Response = 0;

            switch (Param)
            {
                case 1: Response = Map.Size;   break;
                case 2: Response = Map.Align;  break;
                case 4: Response = 0x40000000; break;
                case 5: Response = Map.Kind;   break;
            }

            Context.Memory.WriteInt32(Position + 8, Response);

            return 0;
        }

        private long NvMapIocGetId(ServiceCtx Context)
        {
            long Position = Context.Request.GetSendBuffPtr();

            int Handle = Context.Memory.ReadInt32(Position + 4);

            NvMap Map = NvMaps.GetData<NvMap>(Context.Process, Handle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap Handle {Handle}!");

                return -1; //TODO: Corrent error code.
            }

            Context.Memory.WriteInt32(Position, Map.Id);

            return 0;
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