using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu.Types;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu
{
    class NvHostAsGpuDeviceFile : NvDeviceFile
    {
        private static ConcurrentDictionary<KProcess, AddressSpaceContext> _addressSpaceContextRegistry = new ConcurrentDictionary<KProcess, AddressSpaceContext>();

        public NvHostAsGpuDeviceFile(ServiceCtx context) : base(context) { }

        public override NvInternalResult Ioctl(NvIoctl command, Span<byte> arguments)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvGpuAsMagic)
            {
                switch (command.Number)
                {
                    case 0x01:
                        result = CallIoctlMethod<BindChannelArguments>(BindChannel, arguments);
                        break;
                    case 0x02:
                        result = CallIoctlMethod<AllocSpaceArguments>(AllocSpace, arguments);
                        break;
                    case 0x03:
                        result = CallIoctlMethod<FreeSpaceArguments>(FreeSpace, arguments);
                        break;
                    case 0x05:
                        result = CallIoctlMethod<UnmapBufferArguments>(UnmapBuffer, arguments);
                        break;
                    case 0x06:
                        result = CallIoctlMethod<MapBufferExArguments>(MapBufferEx, arguments);
                        break;
                    case 0x08:
                        result = CallIoctlMethod<GetVaRegionsArguments>(GetVaRegions, arguments);
                        break;
                    case 0x09:
                        result = CallIoctlMethod<InitializeExArguments>(InitializeEx, arguments);
                        break;
                    case 0x14:
                        result = CallIoctlMethod<RemapArguments>(Remap, arguments);
                        break;
                }
            }

            return result;
        }

        public override NvInternalResult Ioctl3(NvIoctl command, Span<byte> arguments, Span<byte> inlineOutBuffer)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvGpuAsMagic)
            {
                switch (command.Number)
                {
                    case 0x08:
                        // This is the same as the one in ioctl as inlineOutBuffer is empty.
                        result = CallIoctlMethod<GetVaRegionsArguments>(GetVaRegions, arguments);
                        break;
                }
            }

            return result;
        }

        private NvInternalResult BindChannel(ref BindChannelArguments arguments)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult AllocSpace(ref AllocSpaceArguments arguments)
        {
            AddressSpaceContext addressSpaceContext = GetAddressSpaceContext(Context);

            ulong size = (ulong)arguments.Pages * (ulong)arguments.PageSize;

            NvInternalResult result = NvInternalResult.Success;

            lock (addressSpaceContext)
            {
                // Note: When the fixed offset flag is not set,
                // the Offset field holds the alignment size instead.
                if ((arguments.Flags & AddressSpaceFlags.FixedOffset) != 0)
                {
                    arguments.Offset = (long)addressSpaceContext.Gmm.ReserveFixed((ulong)arguments.Offset, size);
                }
                else
                {
                    arguments.Offset = (long)addressSpaceContext.Gmm.Reserve((ulong)size, (ulong)arguments.Offset);
                }

                if (arguments.Offset < 0)
                {
                    arguments.Offset = 0;

                    Logger.PrintWarning(LogClass.ServiceNv, $"Failed to allocate size {size:x16}!");

                    result = NvInternalResult.OutOfMemory;
                }
                else
                {
                    addressSpaceContext.AddReservation(arguments.Offset, (long)size);
                }
            }

            return result;
        }

        private NvInternalResult FreeSpace(ref FreeSpaceArguments arguments)
        {
            AddressSpaceContext addressSpaceContext = GetAddressSpaceContext(Context);

            NvInternalResult result = NvInternalResult.Success;

            lock (addressSpaceContext)
            {
                ulong size = (ulong)arguments.Pages * (ulong)arguments.PageSize;

                if (addressSpaceContext.RemoveReservation(arguments.Offset))
                {
                    addressSpaceContext.Gmm.Free((ulong)arguments.Offset, size);
                }
                else
                {
                    Logger.PrintWarning(LogClass.ServiceNv,
                        $"Failed to free offset 0x{arguments.Offset:x16} size 0x{size:x16}!");

                    result = NvInternalResult.InvalidInput;
                }
            }

            return result;
        }

        private NvInternalResult UnmapBuffer(ref UnmapBufferArguments arguments)
        {
            AddressSpaceContext addressSpaceContext = GetAddressSpaceContext(Context);

            lock (addressSpaceContext)
            {
                if (addressSpaceContext.RemoveMap(arguments.Offset, out long size))
                {
                    if (size != 0)
                    {
                        addressSpaceContext.Gmm.Free((ulong)arguments.Offset, (ulong)size);
                    }
                }
                else
                {
                    Logger.PrintWarning(LogClass.ServiceNv, $"Invalid buffer offset {arguments.Offset:x16}!");
                }
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult MapBufferEx(ref MapBufferExArguments arguments)
        {
            const string mapErrorMsg = "Failed to map fixed buffer with offset 0x{0:x16} and size 0x{1:x16}!";

            AddressSpaceContext addressSpaceContext = GetAddressSpaceContext(Context);

            NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(Owner, arguments.NvMapHandle, true);

            if (map == null)
            {
                Logger.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap handle 0x{arguments.NvMapHandle:x8}!");

                return NvInternalResult.InvalidInput;
            }

            long physicalAddress;

            if ((arguments.Flags & AddressSpaceFlags.RemapSubRange) != 0)
            {
                lock (addressSpaceContext)
                {
                    if (addressSpaceContext.TryGetMapPhysicalAddress(arguments.Offset, out physicalAddress))
                    {
                        long virtualAddress = arguments.Offset + arguments.BufferOffset;

                        physicalAddress += arguments.BufferOffset;

                        if ((long)addressSpaceContext.Gmm.Map((ulong)physicalAddress, (ulong)virtualAddress, (ulong)arguments.MappingSize) < 0)
                        {
                            string message = string.Format(mapErrorMsg, virtualAddress, arguments.MappingSize);

                            Logger.PrintWarning(LogClass.ServiceNv, message);

                            return NvInternalResult.InvalidInput;
                        }

                        return NvInternalResult.Success;
                    }
                    else
                    {
                        Logger.PrintWarning(LogClass.ServiceNv, $"Address 0x{arguments.Offset:x16} not mapped!");

                        return NvInternalResult.InvalidInput;
                    }
                }
            }

            physicalAddress = map.Address + arguments.BufferOffset;

            long size = arguments.MappingSize;

            if (size == 0)
            {
                size = (uint)map.Size;
            }

            NvInternalResult result = NvInternalResult.Success;

            lock (addressSpaceContext)
            {
                // Note: When the fixed offset flag is not set,
                // the Offset field holds the alignment size instead.
                bool virtualAddressAllocated = (arguments.Flags & AddressSpaceFlags.FixedOffset) == 0;

                if (!virtualAddressAllocated)
                {
                    if (addressSpaceContext.ValidateFixedBuffer(arguments.Offset, size))
                    {
                        arguments.Offset = (long)addressSpaceContext.Gmm.Map((ulong)physicalAddress, (ulong)arguments.Offset, (ulong)size);
                    }
                    else
                    {
                        string message = string.Format(mapErrorMsg, arguments.Offset, size);

                        Logger.PrintWarning(LogClass.ServiceNv, message);

                        result = NvInternalResult.InvalidInput;
                    }
                }
                else
                {
                    arguments.Offset = (long)addressSpaceContext.Gmm.Map((ulong)physicalAddress, (ulong)size);
                }

                if (arguments.Offset < 0)
                {
                    arguments.Offset = 0;

                    Logger.PrintWarning(LogClass.ServiceNv, $"Failed to map size 0x{size:x16}!");

                    result = NvInternalResult.InvalidInput;
                }
                else
                {
                    addressSpaceContext.AddMap(arguments.Offset, size, physicalAddress, virtualAddressAllocated);
                }
            }

            return result;
        }

        private NvInternalResult GetVaRegions(ref GetVaRegionsArguments arguments)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult InitializeEx(ref InitializeExArguments arguments)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult Remap(Span<RemapArguments> arguments)
        {
            for (int index = 0; index < arguments.Length; index++)
            {
                MemoryManager gmm = GetAddressSpaceContext(Context).Gmm;

                NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(Owner, arguments[index].NvMapHandle, true);

                if (map == null)
                {
                    Logger.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap handle 0x{arguments[index].NvMapHandle:x8}!");

                    return NvInternalResult.InvalidInput;
                }

                long result = (long)gmm.Map(
                    ((ulong)arguments[index].MapOffset << 16) + (ulong)map.Address,
                    (ulong)arguments[index].GpuOffset << 16,
                    (ulong)arguments[index].Pages << 16);

                if (result < 0)
                {
                    Logger.PrintWarning(LogClass.ServiceNv,
                        $"Page 0x{arguments[index].GpuOffset:x16} size 0x{arguments[index].Pages:x16} not allocated!");

                    return NvInternalResult.InvalidInput;
                }
            }

            return NvInternalResult.Success;
        }

        public override void Close() { }

        public static AddressSpaceContext GetAddressSpaceContext(ServiceCtx context)
        {
            return _addressSpaceContextRegistry.GetOrAdd(context.Process, (key) => new AddressSpaceContext(context));
        }
    }
}
