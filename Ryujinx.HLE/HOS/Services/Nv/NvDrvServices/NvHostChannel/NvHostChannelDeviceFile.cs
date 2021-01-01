using Ryujinx.Common.Collections;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel.Types;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;
using Ryujinx.HLE.HOS.Services.Nv.Types;
using Ryujinx.Memory;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel
{
    class NvHostChannelDeviceFile : NvDeviceFile
    {
        private const uint MaxModuleSyncpoint = 16;

        private uint _timeout;
        private uint _submitTimeout;
        private uint _timeslice;

        private readonly Switch _device;

        private readonly IVirtualMemoryManager _memory;
        private NvMemoryAllocator _memoryAllocator;

        public enum ResourcePolicy
        {
            Device,
            Channel
        }

        protected static uint[] DeviceSyncpoints = new uint[MaxModuleSyncpoint];

        protected uint[] ChannelSyncpoints;

        protected static ResourcePolicy ChannelResourcePolicy = ResourcePolicy.Device;

        private NvFence _channelSyncpoint;

        public NvHostChannelDeviceFile(ServiceCtx context, IVirtualMemoryManager memory, long owner) : base(context, owner)
        {
            _device        = context.Device;
            _memory        = memory;
            _timeout       = 3000;
            _submitTimeout = 0;
            _timeslice     = 0;
            _memoryAllocator = _device.MemoryAllocator;

            ChannelSyncpoints = new uint[MaxModuleSyncpoint];

            _channelSyncpoint.Id = _device.System.HostSyncpoint.AllocateSyncpoint(false);
            _channelSyncpoint.UpdateValue(_device.System.HostSyncpoint);
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
            SubmitArguments     submitHeader   = GetSpanAndSkip<SubmitArguments>(ref arguments, 1)[0];
            Span<CommandBuffer> commandBuffers = GetSpanAndSkip<CommandBuffer>(ref arguments, submitHeader.CmdBufsCount);
            Span<Reloc>         relocs         = GetSpanAndSkip<Reloc>(ref arguments, submitHeader.RelocsCount);
            Span<uint>          relocShifts    = GetSpanAndSkip<uint>(ref arguments, submitHeader.RelocsCount);
            Span<SyncptIncr>    syncptIncrs    = GetSpanAndSkip<SyncptIncr>(ref arguments, submitHeader.SyncptIncrsCount);
            Span<SyncptIncr>    waitChecks     = GetSpanAndSkip<SyncptIncr>(ref arguments, submitHeader.SyncptIncrsCount); // ?
            Span<Fence>         fences         = GetSpanAndSkip<Fence>(ref arguments, submitHeader.FencesCount);

            lock (_device)
            {
                for (int i = 0; i < syncptIncrs.Length; i++)
                {
                    SyncptIncr syncptIncr = syncptIncrs[i];

                    uint id = syncptIncr.Id;

                    fences[i].Id = id;
                    fences[i].Thresh = Context.Device.System.HostSyncpoint.IncrementSyncpointMax(id, syncptIncr.Incrs);
                }

                foreach (CommandBuffer commandBuffer in commandBuffers)
                {
                    NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(Owner, commandBuffer.Mem);

                    var data = _memory.GetSpan(map.Address + commandBuffer.Offset, commandBuffer.WordsCount * 4);

                    _device.Host1x.Submit(MemoryMarshal.Cast<byte, int>(data));
                }
            }

            fences[0].Thresh = Context.Device.System.HostSyncpoint.IncrementSyncpointMax(fences[0].Id, 1);

            Span<int> tmpCmdBuff = stackalloc int[1];

            tmpCmdBuff[0] = (4 << 28) | (int)fences[0].Id;

            _device.Host1x.Submit(tmpCmdBuff);

            return NvInternalResult.Success;
        }

        private Span<T> GetSpanAndSkip<T>(ref Span<byte> arguments, int count) where T : unmanaged
        {
            Span<T> output = MemoryMarshal.Cast<byte, T>(arguments).Slice(0, count);

            arguments = arguments.Slice(Unsafe.SizeOf<T>() * count);

            return output;
        }

        private NvInternalResult GetSyncpoint(ref GetParameterArguments arguments)
        {
            if (arguments.Parameter >= MaxModuleSyncpoint)
            {
                return NvInternalResult.InvalidInput;
            }

            if (ChannelResourcePolicy == ResourcePolicy.Device)
            {
                arguments.Value = GetSyncpointDevice(_device.System.HostSyncpoint, arguments.Parameter, false);
            }
            else
            {
                arguments.Value = GetSyncpointChannel(arguments.Parameter, false);
            }

            if (arguments.Value == 0)
            {
                return NvInternalResult.TryAgain;
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult GetWaitBase(ref GetParameterArguments arguments)
        {
            arguments.Value = 0;

            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SetSubmitTimeout(ref uint submitTimeout)
        {
            _submitTimeout = submitTimeout;

            Logger.Stub?.PrintStub(LogClass.ServiceNv);

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
                    Logger.Warning?.Print(LogClass.ServiceNv, $"Invalid handle 0x{commandBufferEntry.MapHandle:x8}!");

                    return NvInternalResult.InvalidInput;
                }

                lock (map)
                {
                    if (map.DmaMapAddress == 0)
                    {
                        ulong va = _memoryAllocator.GetFreeAddress((ulong) map.Size, out ulong freeAddressStartPosition, 1, MemoryManager.PageSize);

                        if (va != NvMemoryAllocator.PteUnmapped && va <= uint.MaxValue && (va + (uint)map.Size) <= uint.MaxValue)
                        {
                            _memoryAllocator.AllocateRange(va, (uint)map.Size, freeAddressStartPosition);
                            gmm.Map(map.Address, va, (uint)map.Size);
                            map.DmaMapAddress = va;
                        }
                        else
                        {
                            map.DmaMapAddress = NvMemoryAllocator.PteUnmapped;
                        }
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
                    Logger.Warning?.Print(LogClass.ServiceNv, $"Invalid handle 0x{commandBufferEntry.MapHandle:x8}!");

                    return NvInternalResult.InvalidInput;
                }

                lock (map)
                {
                    if (map.DmaMapAddress != 0)
                    {
                        // FIXME:
                        // To make unmapping work, we need separate address space per channel.
                        // Right now NVDEC and VIC share the GPU address space which is not correct at all.

                        // gmm.Free((ulong)map.DmaMapAddress, (uint)map.Size);

                        // map.DmaMapAddress = 0;
                    }
                }
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult SetNvMapFd(ref int nvMapFd)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SetTimeout(ref uint timeout)
        {
            _timeout = timeout;

            Logger.Stub?.PrintStub(LogClass.ServiceNv);

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
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult ZcullBind(ref ZcullBindArguments arguments)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SetErrorNotifier(ref SetErrorNotifierArguments arguments)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

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

            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            // TODO: disable and preempt channel when GPU scheduler will be implemented.

            return NvInternalResult.Success;
        }

        private NvInternalResult AllocGpfifoEx(ref AllocGpfifoExArguments arguments)
        {
            _channelSyncpoint.UpdateValue(_device.System.HostSyncpoint);

            arguments.Fence = _channelSyncpoint;

            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult AllocGpfifoEx2(ref AllocGpfifoExArguments arguments)
        {
            _channelSyncpoint.UpdateValue(_device.System.HostSyncpoint);

            arguments.Fence = _channelSyncpoint;

            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SetTimeslice(ref uint timeslice)
        {
            if (timeslice < 1000 || timeslice > 50000)
            {
                return NvInternalResult.InvalidInput;
            }

            _timeslice = timeslice; // in micro-seconds

            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            // TODO: disable and preempt channel when GPU scheduler will be implemented.

            return NvInternalResult.Success;
        }

        private NvInternalResult SetUserData(ref ulong userData)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        protected NvInternalResult SubmitGpfifo(ref SubmitGpfifoArguments header, Span<ulong> entries)
        {
            if (header.Flags.HasFlag(SubmitGpfifoFlags.FenceWait) && header.Flags.HasFlag(SubmitGpfifoFlags.IncrementWithValue))
            {
                return NvInternalResult.InvalidInput;
            }

            if (header.Flags.HasFlag(SubmitGpfifoFlags.FenceWait) && !_device.System.HostSyncpoint.IsSyncpointExpired(header.Fence.Id, header.Fence.Value))
            {
                _device.Gpu.GPFifo.PushHostCommandBuffer(CreateWaitCommandBuffer(header.Fence));
            }

            _device.Gpu.GPFifo.PushEntries(entries);

            header.Fence.Id = _channelSyncpoint.Id;

            if (header.Flags.HasFlag(SubmitGpfifoFlags.FenceIncrement) || header.Flags.HasFlag(SubmitGpfifoFlags.IncrementWithValue))
            {
                uint incrementCount = header.Flags.HasFlag(SubmitGpfifoFlags.FenceIncrement) ? 2u : 0u;

                if (header.Flags.HasFlag(SubmitGpfifoFlags.IncrementWithValue))
                {
                    incrementCount += header.Fence.Value;
                }

                header.Fence.Value = _device.System.HostSyncpoint.IncrementSyncpointMaxExt(header.Fence.Id, (int)incrementCount);
            }
            else
            {
                header.Fence.Value = _device.System.HostSyncpoint.ReadSyncpointMaxValue(header.Fence.Id);
            }

            if (header.Flags.HasFlag(SubmitGpfifoFlags.FenceIncrement))
            {
                _device.Gpu.GPFifo.PushHostCommandBuffer(CreateIncrementCommandBuffer(ref header.Fence, header.Flags));
            }

            header.Flags = SubmitGpfifoFlags.None;

            _device.Gpu.GPFifo.SignalNewEntries();

            return NvInternalResult.Success;
        }

        public uint GetSyncpointChannel(uint index, bool isClientManaged)
        {
            if (ChannelSyncpoints[index] != 0)
            {
                return ChannelSyncpoints[index];
            }

            ChannelSyncpoints[index] = _device.System.HostSyncpoint.AllocateSyncpoint(isClientManaged);

            return ChannelSyncpoints[index];
        }

        public static uint GetSyncpointDevice(NvHostSyncpt syncpointManager, uint index, bool isClientManaged)
        {
            if (DeviceSyncpoints[index] != 0)
            {
                return DeviceSyncpoints[index];
            }

            DeviceSyncpoints[index] = syncpointManager.AllocateSyncpoint(isClientManaged);

            return DeviceSyncpoints[index];
        }

        private static int[] CreateWaitCommandBuffer(NvFence fence)
        {
            int[] commandBuffer = new int[4];

            // SyncpointValue = fence.Value;
            commandBuffer[0] = 0x2001001C;
            commandBuffer[1] = (int)fence.Value;

            // SyncpointAction(fence.id, increment: false, switch_en: true);
            commandBuffer[2] = 0x2001001D;
            commandBuffer[3] = (((int)fence.Id << 8) | (0 << 0) | (1 << 4));

            return commandBuffer;
        }

        private int[] CreateIncrementCommandBuffer(ref NvFence fence, SubmitGpfifoFlags flags)
        {
            bool hasWfi = !flags.HasFlag(SubmitGpfifoFlags.SuppressWfi);

            int[] commandBuffer;

            int offset = 0;

            if (hasWfi)
            {
                commandBuffer = new int[8];

                // WaitForInterrupt(handle)
                commandBuffer[offset++] = 0x2001001E;
                commandBuffer[offset++] = 0x0;
            }
            else
            {
                commandBuffer = new int[6];
            }

            // SyncpointValue = 0x0;
            commandBuffer[offset++] = 0x2001001C;
            commandBuffer[offset++] = 0x0;

            // Increment the syncpoint 2 times. (mitigate a hardware bug)

            // SyncpointAction(fence.id, increment: true, switch_en: false);
            commandBuffer[offset++] = 0x2001001D;
            commandBuffer[offset++] = (((int)fence.Id << 8) | (1 << 0) | (0 << 4));

            // SyncpointAction(fence.id, increment: true, switch_en: false);
            commandBuffer[offset++] = 0x2001001D;
            commandBuffer[offset++] = (((int)fence.Id << 8) | (1 << 0) | (0 << 4));

            return commandBuffer;
        }

        public override void Close() { }
    }
}
