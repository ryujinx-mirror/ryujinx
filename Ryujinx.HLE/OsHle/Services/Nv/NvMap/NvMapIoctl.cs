using ChocolArm64.Memory;
using Ryujinx.HLE.Gpu.Memory;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Utilities;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.OsHle.Services.Nv.NvMap
{
    class NvMapIoctl
    {
        private const int FlagNotFreedYet = 1;

        private static ConcurrentDictionary<Process, IdDictionary> Maps;

        static NvMapIoctl()
        {
            Maps = new ConcurrentDictionary<Process, IdDictionary>();
        }

        public static int ProcessIoctl(ServiceCtx Context, int Cmd)
        {
            switch (Cmd & 0xffff)
            {
                case 0x0101: return Create(Context);
                case 0x0103: return FromId(Context);
                case 0x0104: return Alloc (Context);
                case 0x0105: return Free  (Context);
                case 0x0109: return Param (Context);
                case 0x010e: return GetId (Context);
            }

            Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Unsupported Ioctl command 0x{Cmd:x8}!");

            return NvResult.NotSupported;
        }

        private static int Create(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvMapCreate Args = AMemoryHelper.Read<NvMapCreate>(Context.Memory, InputPosition);

            if (Args.Size == 0)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid size 0x{Args.Size:x8}!");

                return NvResult.InvalidInput;
            }

            int Size = IntUtils.AlignUp(Args.Size, NvGpuVmm.PageSize);

            Args.Handle = AddNvMap(Context, new NvMapHandle(Size));

            Context.Ns.Log.PrintInfo(LogClass.ServiceNv, $"Created map {Args.Handle} with size 0x{Size:x8}!");

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int FromId(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvMapFromId Args = AMemoryHelper.Read<NvMapFromId>(Context.Memory, InputPosition);

            NvMapHandle Map = GetNvMap(Context, Args.Id);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{Args.Handle:x8}!");

                return NvResult.InvalidInput;
            }

            Map.IncrementRefCount();

            Args.Handle = Args.Id;

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int Alloc(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvMapAlloc Args = AMemoryHelper.Read<NvMapAlloc>(Context.Memory, InputPosition);

            NvMapHandle Map = GetNvMap(Context, Args.Handle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{Args.Handle:x8}!");

                return NvResult.InvalidInput;
            }

            if ((Args.Align & (Args.Align - 1)) != 0)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid alignment 0x{Args.Align:x8}!");

                return NvResult.InvalidInput;
            }

            if ((uint)Args.Align < NvGpuVmm.PageSize)
            {
                Args.Align = NvGpuVmm.PageSize;
            }

            int Result = NvResult.Success;

            if (!Map.Allocated)
            {
                Map.Allocated = true;

                Map.Align =       Args.Align;
                Map.Kind  = (byte)Args.Kind;

                int Size = IntUtils.AlignUp(Map.Size, NvGpuVmm.PageSize);

                long Address = Args.Address;

                if (Address == 0)
                {
                    //When the address is zero, we need to allocate
                    //our own backing memory for the NvMap.
                    if (!Context.Ns.Os.Allocator.TryAllocate((uint)Size, out Address))
                    {
                        Result = NvResult.OutOfMemory;
                    }
                }

                if (Result == NvResult.Success)
                {
                    Map.Size    = Size;
                    Map.Address = Address;
                }
            }

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return Result;
        }

        private static int Free(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvMapFree Args = AMemoryHelper.Read<NvMapFree>(Context.Memory, InputPosition);

            NvMapHandle Map = GetNvMap(Context, Args.Handle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{Args.Handle:x8}!");

                return NvResult.InvalidInput;
            }

            long OldRefCount = Map.DecrementRefCount();

            if (OldRefCount <= 1)
            {
                DeleteNvMap(Context, Args.Handle);

                Context.Ns.Log.PrintInfo(LogClass.ServiceNv, $"Deleted map {Args.Handle}!");

                Args.Flags = 0;
            }
            else
            {
                Args.Flags = FlagNotFreedYet;
            }

            Args.RefCount = OldRefCount;
            Args.Size     = Map.Size;

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int Param(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvMapParam Args = AMemoryHelper.Read<NvMapParam>(Context.Memory, InputPosition);

            NvMapHandle Map = GetNvMap(Context, Args.Handle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{Args.Handle:x8}!");

                return NvResult.InvalidInput;
            }

            switch ((NvMapHandleParam)Args.Param)
            {
                case NvMapHandleParam.Size:  Args.Result = Map.Size;   break;
                case NvMapHandleParam.Align: Args.Result = Map.Align;  break;
                case NvMapHandleParam.Heap:  Args.Result = 0x40000000; break;
                case NvMapHandleParam.Kind:  Args.Result = Map.Kind;   break;
                case NvMapHandleParam.Compr: Args.Result = 0;          break;

                //Note: Base is not supported and returns an error.
                //Any other value also returns an error.
                default: return NvResult.InvalidInput;
            }

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int GetId(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvMapGetId Args = AMemoryHelper.Read<NvMapGetId>(Context.Memory, InputPosition);

            NvMapHandle Map = GetNvMap(Context, Args.Handle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{Args.Handle:x8}!");

                return NvResult.InvalidInput;
            }

            Args.Id = Args.Handle;

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int AddNvMap(ServiceCtx Context, NvMapHandle Map)
        {
            IdDictionary Dict = Maps.GetOrAdd(Context.Process, (Key) =>
            {
                IdDictionary NewDict = new IdDictionary();

                NewDict.Add(0, new NvMapHandle());

                return NewDict;
            });

            return Dict.Add(Map);
        }

        private static bool DeleteNvMap(ServiceCtx Context, int Handle)
        {
            if (Maps.TryGetValue(Context.Process, out IdDictionary Dict))
            {
                return Dict.Delete(Handle) != null;
            }

            return false;
        }

        public static void InitializeNvMap(ServiceCtx Context)
        {
            IdDictionary Dict = Maps.GetOrAdd(Context.Process, (Key) =>new IdDictionary());

            Dict.Add(0, new NvMapHandle());
        }

        public static NvMapHandle GetNvMapWithFb(ServiceCtx Context, int Handle)
        {
            if (Maps.TryGetValue(Context.Process, out IdDictionary Dict))
            {
                return Dict.GetData<NvMapHandle>(Handle);
            }

            return null;
        }

        public static NvMapHandle GetNvMap(ServiceCtx Context, int Handle)
        {
            if (Handle != 0 && Maps.TryGetValue(Context.Process, out IdDictionary Dict))
            {
                return Dict.GetData<NvMapHandle>(Handle);
            }

            return null;
        }

        public static void UnloadProcess(Process Process)
        {
            Maps.TryRemove(Process, out _);
        }
    }
}