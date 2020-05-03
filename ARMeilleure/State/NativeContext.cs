using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Memory;
using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.State
{
    class NativeContext : IDisposable
    {
        private const int IntSize   = 8;
        private const int VecSize   = 16;
        private const int FlagSize  = 4;
        private const int ExtraSize = 8;

        private const int TotalSize = RegisterConsts.IntRegsCount * IntSize  +
                                      RegisterConsts.VecRegsCount * VecSize  +
                                      RegisterConsts.FlagsCount   * FlagSize +
                                      RegisterConsts.FpFlagsCount * FlagSize + ExtraSize;

        private readonly IJitMemoryBlock _block;

        public IntPtr BasePtr => _block.Pointer;

        public NativeContext(IJitMemoryAllocator allocator)
        {
            _block = allocator.Allocate(TotalSize);
        }

        public ulong GetX(int index)
        {
            if ((uint)index >= RegisterConsts.IntRegsCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return (ulong)Marshal.ReadInt64(BasePtr, index * IntSize);
        }

        public void SetX(int index, ulong value)
        {
            if ((uint)index >= RegisterConsts.IntRegsCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Marshal.WriteInt64(BasePtr, index * IntSize, (long)value);
        }

        public V128 GetV(int index)
        {
            if ((uint)index >= RegisterConsts.IntRegsCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            int offset = RegisterConsts.IntRegsCount * IntSize + index * VecSize;

            return new V128(
                Marshal.ReadInt64(BasePtr, offset + 0),
                Marshal.ReadInt64(BasePtr, offset + 8));
        }

        public void SetV(int index, V128 value)
        {
            if ((uint)index >= RegisterConsts.IntRegsCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            int offset = RegisterConsts.IntRegsCount * IntSize + index * VecSize;

            Marshal.WriteInt64(BasePtr, offset + 0, value.Extract<long>(0));
            Marshal.WriteInt64(BasePtr, offset + 8, value.Extract<long>(1));
        }

        public bool GetPstateFlag(PState flag)
        {
            if ((uint)flag >= RegisterConsts.FlagsCount)
            {
                throw new ArgumentException($"Invalid flag \"{flag}\" specified.");
            }

            int offset =
                RegisterConsts.IntRegsCount * IntSize +
                RegisterConsts.VecRegsCount * VecSize + (int)flag * FlagSize;

            int value = Marshal.ReadInt32(BasePtr, offset);

            return value != 0;
        }

        public void SetPstateFlag(PState flag, bool value)
        {
            if ((uint)flag >= RegisterConsts.FlagsCount)
            {
                throw new ArgumentException($"Invalid flag \"{flag}\" specified.");
            }

            int offset =
                RegisterConsts.IntRegsCount * IntSize +
                RegisterConsts.VecRegsCount * VecSize + (int)flag * FlagSize;

            Marshal.WriteInt32(BasePtr, offset, value ? 1 : 0);
        }

        public bool GetFPStateFlag(FPState flag)
        {
            if ((uint)flag >= RegisterConsts.FlagsCount)
            {
                throw new ArgumentException($"Invalid flag \"{flag}\" specified.");
            }

            int offset =
                RegisterConsts.IntRegsCount * IntSize  +
                RegisterConsts.VecRegsCount * VecSize  + 
                RegisterConsts.FlagsCount   * FlagSize + (int)flag * FlagSize;

            int value = Marshal.ReadInt32(BasePtr, offset);

            return value != 0;
        }

        public void SetFPStateFlag(FPState flag, bool value)
        {
            if ((uint)flag >= RegisterConsts.FlagsCount)
            {
                throw new ArgumentException($"Invalid flag \"{flag}\" specified.");
            }

            int offset =
                RegisterConsts.IntRegsCount * IntSize  +
                RegisterConsts.VecRegsCount * VecSize  +
                RegisterConsts.FlagsCount   * FlagSize + (int)flag * FlagSize;

            Marshal.WriteInt32(BasePtr, offset, value ? 1 : 0);
        }

        public int GetCounter()
        {
            return Marshal.ReadInt32(BasePtr, GetCounterOffset());
        }

        public void SetCounter(int value)
        {
            Marshal.WriteInt32(BasePtr, GetCounterOffset(), value);
        }

        public static int GetRegisterOffset(Register reg)
        {
            int offset, size;

            if (reg.Type == RegisterType.Integer)
            {
                offset = reg.Index * IntSize;

                size = IntSize;
            }
            else if (reg.Type == RegisterType.Vector)
            {
                offset = RegisterConsts.IntRegsCount * IntSize + reg.Index * VecSize;

                size = VecSize;
            }
            else /* if (reg.Type == RegisterType.Flag) */
            {
                offset = RegisterConsts.IntRegsCount * IntSize +
                         RegisterConsts.VecRegsCount * VecSize + reg.Index * FlagSize;

                size = FlagSize;
            }

            if ((uint)(offset + size) > (uint)TotalSize)
            {
                throw new ArgumentException("Invalid register.");
            }

            return offset;
        }

        public static int GetCounterOffset()
        {
            return RegisterConsts.IntRegsCount * IntSize  +
                   RegisterConsts.VecRegsCount * VecSize  +
                   RegisterConsts.FlagsCount   * FlagSize +
                   RegisterConsts.FpFlagsCount * FlagSize;
        }

        public static int GetCallAddressOffset()
        {
            return RegisterConsts.IntRegsCount * IntSize  +
                   RegisterConsts.VecRegsCount * VecSize  +
                   RegisterConsts.FlagsCount   * FlagSize +
                   RegisterConsts.FpFlagsCount * FlagSize + 4;
        }

        public void Dispose()
        {
            _block.Dispose();
        }
    }
}