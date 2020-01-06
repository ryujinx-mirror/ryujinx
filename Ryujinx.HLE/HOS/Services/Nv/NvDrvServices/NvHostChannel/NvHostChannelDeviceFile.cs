using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel.Types;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel
{
    class NvHostChannelDeviceFile : NvDeviceFile
    {
        private uint _timeout;
        private uint _submitTimeout;
        private uint _timeslice;

        private GpuContext _gpu;

        private ARMeilleure.Memory.MemoryManager _memory;

        public NvHostChannelDeviceFile(ServiceCtx context) : base(context)
        {
            _gpu           = context.Device.Gpu;
            _memory        = context.Memory;
            _timeout       = 3000;
            _submitTimeout = 0;
            _timeslice     = 0;
        }

        public override NvInternalResult Ioctl(NvIoctl command, Span<byte> arguments)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvHostCustomMagic)
            {
                switch (command.Number)
                {
                    case 0x01:
                        result = Submit(arguments);
                        break;
                    case 0x02:
                        result = CallIoctlMethod<GetParameterArguments>(GetSyncpoint, arguments);
                        break;
                    case 0x03:
                        result = CallIoctlMethod<GetParameterArguments>(GetWaitBase, arguments);
                        break;
                    case 0x07:
                        result = CallIoctlMethod<uint>(SetSubmitTimeout, arguments);
                        break;
                    case 0x09:
                        result = MapCommandBuffer(arguments);
                        break;
                    case 0x0a:
                        result = UnmapCommandBuffer(arguments);
                        break;
                }
            }
            else if (command.Type == NvIoctl.NvHostMagic)
            {
                switch (command.Number)
                {
                    case 0x01:
                        result = CallIoctlMethod<int>(SetNvMapFd, arguments);
                        break;
                    case 0x03:
                        result = CallIoctlMethod<uint>(SetTimeout, arguments);
                        break;
                    case 0x08:
                        result = SubmitGpfifo(arguments);
                        break;
                    case 0x09:
                        result = CallIoctlMethod<AllocObjCtxArguments>(AllocObjCtx, arguments);
                        break;
                    case 0x0b:
                        result = CallIoctlMethod<ZcullBindArguments>(ZcullBind, arguments);
                        break;
                    case 0x0c:
                        result = CallIoctlMethod<SetErrorNotifierArguments>(SetErrorNotifier, arguments);
                        break;
                    case 0x0d:
                        result = CallIoctlMethod<NvChannelPriority>(SetPriority, arguments);
                        break;
                    case 0x18:
                        result = CallIoctlMethod<AllocGpfifoExArguments>(AllocGpfifoEx, arguments);
                        break;
                    case 0x1a:
                        result = CallIoctlMethod<AllocGpfifoExArguments>(AllocGpfifoEx2, arguments);
                        break;
                    case 0x1d:
                        result = CallIoctlMethod<uint>(SetTimeslice, arguments);
                        break;
                }
            }
            else if (command.Type == NvIoctl.NvGpuMagic)
            {
                switch (command.Number)
                {
                    case 0x14:
                        result = CallIoctlMethod<ulong>(SetUserData, arguments);
                        break;
                }
            }

            return result;
        }

        private NvInternalResult Submit(Span<byte> arguments)
        {
            int                 headerSize           = Unsafe.SizeOf<SubmitArguments>();
            SubmitArguments     submitHeader         = MemoryMarshal.Cast<byte, SubmitArguments>(arguments)[0];
            Span<CommandBuffer> commandBufferEntries = MemoryMarshal.Cast<byte, CommandBuffer>(arguments.Slice(headerSize)).Slice(0, submitHeader.CmdBufsCount);
            MemoryManager       gmm                  = NvHostAsGpuDeviceFile.GetAddressSpaceContext(Context).Gmm;

            foreach (CommandBuffer commandBufferEntry in commandBufferEntries)
            {
                NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(Owner, commandBufferEntry.MemoryId);

                int[] commandBufferData = new int[commandBufferEntry.WordsCount];

                for (int offset = 0; offset < commandBufferData.Length; offset++)
                {
                    commandBufferData[offset] = _memory.ReadInt32(map.Address + commandBufferEntry.Offset + offset * 4);
                }

                // TODO: Submit command to engines.
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult GetSyncpoint(ref GetParameterArguments arguments)
        {
            arguments.Value = 0;

            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult GetWaitBase(ref GetParameterArguments arguments)
        {
            arguments.Value = 0;

            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SetSubmitTimeout(ref uint submitTimeout)
        {
            _submitTimeout = submitTimeout;

            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult MapCommandBuffer(Span<byte> arguments)
        {
            int                       headerSize           = Unsafe.SizeOf<MapCommandBufferArguments>();
            MapCommandBufferArguments commandBufferHeader  = MemoryMarshal.Cast<byte, MapCommandBufferArguments>(arguments)[0];
            Span<CommandBufferHandle> commandBufferEntries = MemoryMarshal.Cast<byte, CommandBufferHandle>(arguments.Slice(headerSize)).Slice(0, commandBufferHeader.NumEntries);
            MemoryManager             gmm                  = NvHostAsGpuDeviceFile.GetAddressSpaceContext(Context).Gmm;

            foreach (ref CommandBufferHandle commandBufferEntry in commandBufferEntries)
            {
                NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(Owner, commandBufferEntry.MapHandle);

                if (map == null)
                {
                    Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{commandBufferEntry.MapHandle:x8}!");

                    return NvInternalResult.InvalidInput;
                }

                lock (map)
                {
                    if (map.DmaMapAddress == 0)
                    {
                        map.DmaMapAddress = (long)gmm.MapLow((ulong)map.Address, (uint)map.Size);
                    }

                    commandBufferEntry.MapAddress = (int)map.DmaMapAddress;
                }
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult UnmapCommandBuffer(Span<byte> arguments)
        {
            int                       headerSize           = Unsafe.SizeOf<MapCommandBufferArguments>();
            MapCommandBufferArguments commandBufferHeader  = MemoryMarshal.Cast<byte, MapCommandBufferArguments>(arguments)[0];
            Span<CommandBufferHandle> commandBufferEntries = MemoryMarshal.Cast<byte, CommandBufferHandle>(arguments.Slice(headerSize)).Slice(0, commandBufferHeader.NumEntries);
            MemoryManager             gmm                  = NvHostAsGpuDeviceFile.GetAddressSpaceContext(Context).Gmm;

            foreach (ref CommandBufferHandle commandBufferEntry in commandBufferEntries)
            {
                NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(Owner, commandBufferEntry.MapHandle);

                if (map == null)
                {
                    Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{commandBufferEntry.MapHandle:x8}!");

                    return NvInternalResult.InvalidInput;
                }

                lock (map)
                {
                    if (map.DmaMapAddress != 0)
                    {
                        gmm.Free((ulong)map.DmaMapAddress, (uint)map.Size);

                        map.DmaMapAddress = 0;
                    }
                }
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult SetNvMapFd(ref int nvMapFd)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SetTimeout(ref uint timeout)
        {
            _timeout = timeout;

            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SubmitGpfifo(Span<byte> arguments)
        {
            int                   headerSize             = Unsafe.SizeOf<SubmitGpfifoArguments>();
            SubmitGpfifoArguments gpfifoSubmissionHeader = MemoryMarshal.Cast<byte, SubmitGpfifoArguments>(arguments)[0];
            Span<ulong>           gpfifoEntries          = MemoryMarshal.Cast<byte, ulong>(arguments.Slice(headerSize)).Slice(0, gpfifoSubmissionHeader.NumEntries);

            return SubmitGpfifo(ref gpfifoSubmissionHeader, gpfifoEntries);
        }

        private NvInternalResult AllocObjCtx(ref AllocObjCtxArguments arguments)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult ZcullBind(ref ZcullBindArguments arguments)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SetErrorNotifier(ref SetErrorNotifierArguments arguments)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SetPriority(ref NvChannelPriority priority)
        {
            switch (priority)
            {
                case NvChannelPriority.Low:
                    _timeslice = 1300; // Timeslice low priority in micro-seconds
                    break;
                case NvChannelPriority.Medium:
                    _timeslice = 2600; // Timeslice medium priority in micro-seconds
                    break;
                case NvChannelPriority.High:
                    _timeslice = 5200; // Timeslice high priority in micro-seconds
                    break;
                default:
                    return NvInternalResult.InvalidInput;
            }

            Logger.PrintStub(LogClass.ServiceNv);

            // TODO: disable and preempt channel when GPU scheduler will be implemented.

            return NvInternalResult.Success;
        }

        private NvInternalResult AllocGpfifoEx(ref AllocGpfifoExArguments arguments)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult AllocGpfifoEx2(ref AllocGpfifoExArguments arguments)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SetTimeslice(ref uint timeslice)
        {
            if (timeslice < 1000 || timeslice > 50000)
            {
                return NvInternalResult.InvalidInput;
            }

            _timeslice = timeslice; // in micro-seconds

            Logger.PrintStub(LogClass.ServiceNv);

            // TODO: disable and preempt channel when GPU scheduler will be implemented.

            return NvInternalResult.Success;
        }

        private NvInternalResult SetUserData(ref ulong userData)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        protected NvInternalResult SubmitGpfifo(ref SubmitGpfifoArguments header, Span<ulong> entries)
        {
            foreach (ulong entry in entries)
            {
                _gpu.DmaPusher.Push(entry);
            }

            header.Fence.Id    = 0;
            header.Fence.Value = 0;

            return NvInternalResult.Success;
        }

        public override void Close() { }
    }
}
