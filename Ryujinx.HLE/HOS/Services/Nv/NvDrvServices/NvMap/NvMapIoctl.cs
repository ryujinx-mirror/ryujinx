using ARMeilleure.Memory;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.Utilities;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap
{
    class NvMapIoctl
    {
        private const int FlagNotFreedYet = 1;

        private static ConcurrentDictionary<KProcess, IdDictionary> _maps;

        static NvMapIoctl()
        {
            _maps = new ConcurrentDictionary<KProcess, IdDictionary>();
        }

        public static int ProcessIoctl(ServiceCtx context, int cmd)
        {
            switch (cmd & 0xffff)
            {
                case 0x0101: return Create(context);
                case 0x0103: return FromId(context);
                case 0x0104: return Alloc (context);
                case 0x0105: return Free  (context);
                case 0x0109: return Param (context);
                case 0x010e: return GetId (context);
            }

            Logger.PrintWarning(LogClass.ServiceNv, $"Unsupported Ioctl command 0x{cmd:x8}!");

            return NvResult.NotSupported;
        }

        private static int Create(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvMapCreate args = MemoryHelper.Read<NvMapCreate>(context.Memory, inputPosition);

            if (args.Size == 0)
            {
                Logger.PrintWarning(LogClass.ServiceNv, $"Invalid size 0x{args.Size:x8}!");

                return NvResult.InvalidInput;
            }

            int size = IntUtils.AlignUp(args.Size, NvGpuVmm.PageSize);

            args.Handle = AddNvMap(context, new NvMapHandle(size));

            Logger.PrintInfo(LogClass.ServiceNv, $"Created map {args.Handle} with size 0x{size:x8}!");

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static int FromId(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvMapFromId args = MemoryHelper.Read<NvMapFromId>(context.Memory, inputPosition);

            NvMapHandle map = GetNvMap(context, args.Id);

            if (map == null)
            {
                Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{args.Handle:x8}!");

                return NvResult.InvalidInput;
            }

            map.IncrementRefCount();

            args.Handle = args.Id;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static int Alloc(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvMapAlloc args = MemoryHelper.Read<NvMapAlloc>(context.Memory, inputPosition);

            NvMapHandle map = GetNvMap(context, args.Handle);

            if (map == null)
            {
                Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{args.Handle:x8}!");

                return NvResult.InvalidInput;
            }

            if ((args.Align & (args.Align - 1)) != 0)
            {
                Logger.PrintWarning(LogClass.ServiceNv, $"Invalid alignment 0x{args.Align:x8}!");

                return NvResult.InvalidInput;
            }

            if ((uint)args.Align < NvGpuVmm.PageSize)
            {
                args.Align = NvGpuVmm.PageSize;
            }

            int result = NvResult.Success;

            if (!map.Allocated)
            {
                map.Allocated = true;

                map.Align =       args.Align;
                map.Kind  = (byte)args.Kind;

                int size = IntUtils.AlignUp(map.Size, NvGpuVmm.PageSize);

                long address = args.Address;

                if (address == 0)
                {
                    // When the address is zero, we need to allocate
                    // our own backing memory for the NvMap.
                    // TODO: Is this allocation inside the transfer memory?
                    result = NvResult.OutOfMemory;
                }

                if (result == NvResult.Success)
                {
                    map.Size    = size;
                    map.Address = address;
                }
            }

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return result;
        }

        private static int Free(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvMapFree args = MemoryHelper.Read<NvMapFree>(context.Memory, inputPosition);

            NvMapHandle map = GetNvMap(context, args.Handle);

            if (map == null)
            {
                Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{args.Handle:x8}!");

                return NvResult.InvalidInput;
            }

            if (map.DecrementRefCount() <= 0)
            {
                DeleteNvMap(context, args.Handle);

                Logger.PrintInfo(LogClass.ServiceNv, $"Deleted map {args.Handle}!");

                args.Address = map.Address;
                args.Flags   = 0;
            }
            else
            {
                args.Address = 0;
                args.Flags   = FlagNotFreedYet;
            }

            args.Size = map.Size;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static int Param(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvMapParam args = MemoryHelper.Read<NvMapParam>(context.Memory, inputPosition);

            NvMapHandle map = GetNvMap(context, args.Handle);

            if (map == null)
            {
                Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{args.Handle:x8}!");

                return NvResult.InvalidInput;
            }

            switch ((NvMapHandleParam)args.Param)
            {
                case NvMapHandleParam.Size:  args.Result = map.Size;   break;
                case NvMapHandleParam.Align: args.Result = map.Align;  break;
                case NvMapHandleParam.Heap:  args.Result = 0x40000000; break;
                case NvMapHandleParam.Kind:  args.Result = map.Kind;   break;
                case NvMapHandleParam.Compr: args.Result = 0;          break;

                // Note: Base is not supported and returns an error.
                // Any other value also returns an error.
                default: return NvResult.InvalidInput;
            }

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static int GetId(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvMapGetId args = MemoryHelper.Read<NvMapGetId>(context.Memory, inputPosition);

            NvMapHandle map = GetNvMap(context, args.Handle);

            if (map == null)
            {
                Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{args.Handle:x8}!");

                return NvResult.InvalidInput;
            }

            args.Id = args.Handle;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static int AddNvMap(ServiceCtx context, NvMapHandle map)
        {
            IdDictionary dict = _maps.GetOrAdd(context.Process, (key) =>
            {
                IdDictionary newDict = new IdDictionary();

                newDict.Add(0, new NvMapHandle());

                return newDict;
            });

            return dict.Add(map);
        }

        private static bool DeleteNvMap(ServiceCtx context, int handle)
        {
            if (_maps.TryGetValue(context.Process, out IdDictionary dict))
            {
                return dict.Delete(handle) != null;
            }

            return false;
        }

        public static void InitializeNvMap(ServiceCtx context)
        {
            IdDictionary dict = _maps.GetOrAdd(context.Process, (key) =>new IdDictionary());

            dict.Add(0, new NvMapHandle());
        }

        public static NvMapHandle GetNvMapWithFb(ServiceCtx context, int handle)
        {
            if (_maps.TryGetValue(context.Process, out IdDictionary dict))
            {
                return dict.GetData<NvMapHandle>(handle);
            }

            return null;
        }

        public static NvMapHandle GetNvMap(ServiceCtx context, int handle)
        {
            if (handle != 0 && _maps.TryGetValue(context.Process, out IdDictionary dict))
            {
                return dict.GetData<NvMapHandle>(handle);
            }

            return null;
        }

        public static void UnloadProcess(KProcess process)
        {
            _maps.TryRemove(process, out _);
        }
    }
}