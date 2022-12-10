using Ryujinx.Tests.Unicorn.Native;
using Ryujinx.Tests.Unicorn.Native.Const;
using System;

namespace Ryujinx.Tests.Unicorn
{
    public class UnicornAArch32 : IDisposable
    {
        internal readonly IntPtr uc;
        private bool _isDisposed = false;

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
            get => GetRegister(Arm.REG_LR);
            set => SetRegister(Arm.REG_LR, value);
        }

        public uint SP
        {
            get => GetRegister(Arm.REG_SP);
            set => SetRegister(Arm.REG_SP, value);
        }

        public uint PC
        {
            get => GetRegister(Arm.REG_PC) & 0xfffffffeu;
            set => SetRegister(Arm.REG_PC, (value & 0xfffffffeu) | (ThumbFlag ? 1u : 0u));
        }

        public uint CPSR
        {
            get => GetRegister(Arm.REG_CPSR);
            set => SetRegister(Arm.REG_CPSR, value);
        }

        public int Fpscr
        {
            get => (int)GetRegister(Arm.REG_FPSCR) | ((int)GetRegister(Arm.REG_FPSCR_NZCV));
            set => SetRegister(Arm.REG_FPSCR, (uint)value);
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

        public bool ThumbFlag
        {
            get => (CPSR & 0x00000020u) != 0;
            set
            {
                CPSR = (CPSR & ~0x00000020u) | (value ? 0x00000020u : 0u);
                SetRegister(Arm.REG_PC, (GetRegister(Arm.REG_PC) & 0xfffffffeu) | (value ? 1u : 0u));
            }
        }

        public UnicornAArch32()
        {
            Interface.Checked(Interface.uc_open(Arch.ARM, Mode.LITTLE_ENDIAN, out uc));

            SetRegister(Arm.REG_C1_C0_2, GetRegister(Arm.REG_C1_C0_2) | 0xf00000);
            SetRegister(Arm.REG_FPEXC, 0x40000000);
        }

        ~UnicornAArch32()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                Interface.Checked(Interface.uc_close(uc));
                _isDisposed = true;
            }
        }

        public void RunForCount(ulong count)
        {
            Interface.Checked(Interface.uc_emu_start(uc, this.PC, 0xFFFFFFFFFFFFFFFFu, 0, count));
        }

        public void Step()
        {
            RunForCount(1);
        }

        private static Arm[] XRegisters = new Arm[16]
        {
            Arm.REG_R0,
            Arm.REG_R1,
            Arm.REG_R2,
            Arm.REG_R3,
            Arm.REG_R4,
            Arm.REG_R5,
            Arm.REG_R6,
            Arm.REG_R7,
            Arm.REG_R8,
            Arm.REG_R9,
            Arm.REG_R10,
            Arm.REG_R11,
            Arm.REG_R12,
            Arm.REG_R13,
            Arm.REG_R14,
            Arm.REG_R15,
        };

        private static Arm[] QRegisters = new Arm[16]
        {
            Arm.REG_Q0,
            Arm.REG_Q1,
            Arm.REG_Q2,
            Arm.REG_Q3,
            Arm.REG_Q4,
            Arm.REG_Q5,
            Arm.REG_Q6,
            Arm.REG_Q7,
            Arm.REG_Q8,
            Arm.REG_Q9,
            Arm.REG_Q10,
            Arm.REG_Q11,
            Arm.REG_Q12,
            Arm.REG_Q13,
            Arm.REG_Q14,
            Arm.REG_Q15
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
            return GetVector((Arm)((int)Arm.REG_D0 + index * 2));
        }

        public void SetQ(int index, SimdValue value)
        {
            if ((uint)index > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            SetVector((Arm)((int)Arm.REG_D0 + index * 2), value);
        }

        public uint GetRegister(Arm register)
        {
            byte[] data = new byte[4];

            Interface.Checked(Interface.uc_reg_read(uc, (int)register, data));

            return (uint)BitConverter.ToInt32(data, 0);
        }

        public void SetRegister(Arm register, uint value)
        {
            byte[] data = BitConverter.GetBytes(value);

            Interface.Checked(Interface.uc_reg_write(uc, (int)register, data));
        }

        public SimdValue GetVector(Arm register)
        {
            byte[] data = new byte[8];

            Interface.Checked(Interface.uc_reg_read(uc, (int)register, data));
            ulong lo = BitConverter.ToUInt64(data, 0);
            Interface.Checked(Interface.uc_reg_read(uc, (int)register + 1, data));
            ulong hi = BitConverter.ToUInt64(data, 0);

            return new SimdValue(lo, hi);
        }

        private void SetVector(Arm register, SimdValue value)
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
            }
            catch (DllNotFoundException) {  }

            return Interface.IsUnicornAvailable;
        }
    }
}