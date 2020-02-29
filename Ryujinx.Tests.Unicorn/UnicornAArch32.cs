using Ryujinx.Tests.Unicorn.Native;
using System;

namespace Ryujinx.Tests.Unicorn
{
    public class UnicornAArch32
    {
        internal readonly IntPtr uc;

        public IndexedProperty<int, uint> R
        {
            get
            {
                return new IndexedProperty<int, uint>(
                    (int i) => GetX(i),
                    (int i, uint value) => SetX(i, value));
            }
        }

        public IndexedProperty<int, SimdValue> Q
        {
            get
            {
                return new IndexedProperty<int, SimdValue>(
                    (int i) => GetQ(i),
                    (int i, SimdValue value) => SetQ(i, value));
            }
        }

        public uint LR
        {
            get => GetRegister(Arm32Register.LR);
            set => SetRegister(Arm32Register.LR, value);
        }

        public uint SP
        {
            get => GetRegister(Arm32Register.SP);
            set => SetRegister(Arm32Register.SP, value);
        }

        public uint PC
        {
            get => GetRegister(Arm32Register.PC);
            set => SetRegister(Arm32Register.PC, value);
        }

        public uint CPSR
        {
            get => (uint)GetRegister(Arm32Register.CPSR);
            set => SetRegister(Arm32Register.CPSR, (uint)value);
        }

        public int Fpscr
        {
            get => (int)GetRegister(Arm32Register.FPSCR) | ((int)GetRegister(Arm32Register.FPSCR_NZCV));
            set => SetRegister(Arm32Register.FPSCR, (uint)value);
        }

        public bool QFlag
        {
            get => (CPSR & 0x8000000u) != 0;
            set => CPSR = (CPSR & ~0x8000000u) | (value ? 0x8000000u : 0u);
        }

        public bool OverflowFlag
        {
            get => (CPSR & 0x10000000u) != 0;
            set => CPSR = (CPSR & ~0x10000000u) | (value ? 0x10000000u : 0u);
        }

        public bool CarryFlag
        {
            get => (CPSR & 0x20000000u) != 0;
            set => CPSR = (CPSR & ~0x20000000u) | (value ? 0x20000000u : 0u);
        }

        public bool ZeroFlag
        {
            get => (CPSR & 0x40000000u) != 0;
            set => CPSR = (CPSR & ~0x40000000u) | (value ? 0x40000000u : 0u);
        }

        public bool NegativeFlag
        {
            get => (CPSR & 0x80000000u) != 0;
            set => CPSR = (CPSR & ~0x80000000u) | (value ? 0x80000000u : 0u);
        }

        public UnicornAArch32()
        {
            Interface.Checked(Interface.uc_open(UnicornArch.UC_ARCH_ARM, UnicornMode.UC_MODE_LITTLE_ENDIAN, out uc));

            SetRegister(Arm32Register.C1_C0_2, GetRegister(Arm32Register.C1_C0_2) | 0xf00000);
            SetRegister(Arm32Register.FPEXC, 0x40000000);
        }

        ~UnicornAArch32()
        {
            Interface.Checked(Native.Interface.uc_close(uc));
        }

        public void RunForCount(ulong count)
        {
            Interface.Checked(Native.Interface.uc_emu_start(uc, this.PC, 0xFFFFFFFFFFFFFFFFu, 0, count));
        }

        public void Step()
        {
            RunForCount(1);
        }

        private static Arm32Register[] XRegisters = new Arm32Register[16]
        {
            Arm32Register.R0,
            Arm32Register.R1,
            Arm32Register.R2,
            Arm32Register.R3,
            Arm32Register.R4,
            Arm32Register.R5,
            Arm32Register.R6,
            Arm32Register.R7,
            Arm32Register.R8,
            Arm32Register.R9,
            Arm32Register.R10,
            Arm32Register.R11,
            Arm32Register.R12,
            Arm32Register.R13,
            Arm32Register.R14,
            Arm32Register.R15,
        };

        private static Arm32Register[] QRegisters = new Arm32Register[16]
        {
            Arm32Register.Q0,
            Arm32Register.Q1,
            Arm32Register.Q2,
            Arm32Register.Q3,
            Arm32Register.Q4,
            Arm32Register.Q5,
            Arm32Register.Q6,
            Arm32Register.Q7,
            Arm32Register.Q8,
            Arm32Register.Q9,
            Arm32Register.Q10,
            Arm32Register.Q11,
            Arm32Register.Q12,
            Arm32Register.Q13,
            Arm32Register.Q14,
            Arm32Register.Q15
        };

        public uint GetX(int index)
        {
            if ((uint)index > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return GetRegister(XRegisters[index]);
        }

        public void SetX(int index, uint value)
        {
            if ((uint)index > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            SetRegister(XRegisters[index], value);
        }

        public SimdValue GetQ(int index)
        {
            if ((uint)index > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            // Getting quadword registers from Unicorn A32 seems to be broken, so we combine its 2 doubleword registers instead.
            return GetVector((Arm32Register)((int)Arm32Register.D0 + index * 2));
        }

        public void SetQ(int index, SimdValue value)
        {
            if ((uint)index > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            SetVector((Arm32Register)((int)Arm32Register.D0 + index * 2), value);
        }

        public uint GetRegister(Arm32Register register)
        {
            byte[] data = new byte[4];

            Interface.Checked(Native.Interface.uc_reg_read(uc, (int)register, data));

            return (uint)BitConverter.ToInt32(data, 0);
        }

        public void SetRegister(Arm32Register register, uint value)
        {
            byte[] data = BitConverter.GetBytes(value);

            Interface.Checked(Interface.uc_reg_write(uc, (int)register, data));
        }

        public SimdValue GetVector(Arm32Register register)
        {
            byte[] data = new byte[8];

            Interface.Checked(Interface.uc_reg_read(uc, (int)register, data));
            ulong lo = BitConverter.ToUInt64(data, 0);
            Interface.Checked(Interface.uc_reg_read(uc, (int)register + 1, data));
            ulong hi = BitConverter.ToUInt64(data, 0);

            return new SimdValue(lo, hi);
        }

        private void SetVector(Arm32Register register, SimdValue value)
        {
            byte[] data = BitConverter.GetBytes(value.GetUInt64(0));
            Interface.Checked(Interface.uc_reg_write(uc, (int)register, data));
            data = BitConverter.GetBytes(value.GetUInt64(1));
            Interface.Checked(Interface.uc_reg_write(uc, (int)register + 1, data));
        }

        public byte[] MemoryRead(ulong address, ulong size)
        {
            byte[] value = new byte[size];

            Interface.Checked(Interface.uc_mem_read(uc, address, value, size));

            return value;
        }

        public byte MemoryRead8(ulong address) => MemoryRead(address, 1)[0];
        public UInt16 MemoryRead16(ulong address) => (UInt16)BitConverter.ToInt16(MemoryRead(address, 2), 0);
        public UInt32 MemoryRead32(ulong address) => (UInt32)BitConverter.ToInt32(MemoryRead(address, 4), 0);
        public UInt64 MemoryRead64(ulong address) => (UInt64)BitConverter.ToInt64(MemoryRead(address, 8), 0);

        public void MemoryWrite(ulong address, byte[] value)
        {
            Interface.Checked(Interface.uc_mem_write(uc, address, value, (ulong)value.Length));
        }

        public void MemoryWrite8(ulong address, byte value) => MemoryWrite(address, new byte[] { value });
        public void MemoryWrite16(ulong address, Int16 value) => MemoryWrite(address, BitConverter.GetBytes(value));
        public void MemoryWrite16(ulong address, UInt16 value) => MemoryWrite(address, BitConverter.GetBytes(value));
        public void MemoryWrite32(ulong address, Int32 value) => MemoryWrite(address, BitConverter.GetBytes(value));
        public void MemoryWrite32(ulong address, UInt32 value) => MemoryWrite(address, BitConverter.GetBytes(value));
        public void MemoryWrite64(ulong address, Int64 value) => MemoryWrite(address, BitConverter.GetBytes(value));
        public void MemoryWrite64(ulong address, UInt64 value) => MemoryWrite(address, BitConverter.GetBytes(value));

        public void MemoryMap(ulong address, ulong size, MemoryPermission permissions)
        {
            Interface.Checked(Interface.uc_mem_map(uc, address, size, (uint)permissions));
        }

        public void MemoryUnmap(ulong address, ulong size)
        {
            Interface.Checked(Interface.uc_mem_unmap(uc, address, size));
        }

        public void MemoryProtect(ulong address, ulong size, MemoryPermission permissions)
        {
            Interface.Checked(Interface.uc_mem_protect(uc, address, size, (uint)permissions));
        }

        public static bool IsAvailable()
        {
            try
            {
                Interface.uc_version(out _, out _);

                return true;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
        }
    }
}
