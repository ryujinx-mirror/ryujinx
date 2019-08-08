using Ryujinx.Tests.Unicorn.Native;
using System;

namespace Ryujinx.Tests.Unicorn
{
    public class UnicornAArch64
    {
        internal readonly IntPtr uc;

        public IndexedProperty<int, ulong> X
        {
            get
            {
                return new IndexedProperty<int, ulong>(
                    (int i)              => GetX(i),
                    (int i, ulong value) => SetX(i, value));
            }
        }

        public IndexedProperty<int, SimdValue> Q
        {
            get
            {
                return new IndexedProperty<int, SimdValue>(
                    (int i)                  => GetQ(i),
                    (int i, SimdValue value) => SetQ(i, value));
            }
        }

        public ulong LR
        {
            get => GetRegister(ArmRegister.LR);
            set => SetRegister(ArmRegister.LR, value);
        }

        public ulong SP
        {
            get => GetRegister(ArmRegister.SP);
            set => SetRegister(ArmRegister.SP, value);
        }

        public ulong PC
        {
            get => GetRegister(ArmRegister.PC);
            set => SetRegister(ArmRegister.PC, value);
        }

        public uint Pstate
        {
            get => (uint)GetRegister(ArmRegister.PSTATE);
            set =>       SetRegister(ArmRegister.PSTATE, (uint)value);
        }

        public int Fpcr
        {
            get => (int)GetRegister(ArmRegister.FPCR);
            set =>      SetRegister(ArmRegister.FPCR, (uint)value);
        }

        public int Fpsr
        {
            get => (int)GetRegister(ArmRegister.FPSR);
            set =>      SetRegister(ArmRegister.FPSR, (uint)value);
        }

        public bool OverflowFlag
        {
            get =>          (Pstate &  0x10000000u) != 0;
            set => Pstate = (Pstate & ~0x10000000u) | (value ? 0x10000000u : 0u);
        }

        public bool CarryFlag
        {
            get =>          (Pstate &  0x20000000u) != 0;
            set => Pstate = (Pstate & ~0x20000000u) | (value ? 0x20000000u : 0u);
        }

        public bool ZeroFlag
        {
            get =>          (Pstate &  0x40000000u) != 0;
            set => Pstate = (Pstate & ~0x40000000u) | (value ? 0x40000000u : 0u);
        }

        public bool NegativeFlag
        {
            get =>          (Pstate &  0x80000000u) != 0;
            set => Pstate = (Pstate & ~0x80000000u) | (value ? 0x80000000u : 0u);
        }

        public UnicornAArch64()
        {
            Interface.Checked(Interface.uc_open(UnicornArch.UC_ARCH_ARM64, UnicornMode.UC_MODE_LITTLE_ENDIAN, out uc));

            SetRegister(ArmRegister.CPACR_EL1, 0x00300000);
        }

        ~UnicornAArch64()
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

        private static ArmRegister[] XRegisters = new ArmRegister[31]
        {
            ArmRegister.X0,
            ArmRegister.X1,
            ArmRegister.X2,
            ArmRegister.X3,
            ArmRegister.X4,
            ArmRegister.X5,
            ArmRegister.X6,
            ArmRegister.X7,
            ArmRegister.X8,
            ArmRegister.X9,
            ArmRegister.X10,
            ArmRegister.X11,
            ArmRegister.X12,
            ArmRegister.X13,
            ArmRegister.X14,
            ArmRegister.X15,
            ArmRegister.X16,
            ArmRegister.X17,
            ArmRegister.X18,
            ArmRegister.X19,
            ArmRegister.X20,
            ArmRegister.X21,
            ArmRegister.X22,
            ArmRegister.X23,
            ArmRegister.X24,
            ArmRegister.X25,
            ArmRegister.X26,
            ArmRegister.X27,
            ArmRegister.X28,
            ArmRegister.X29,
            ArmRegister.X30,
        };

        private static ArmRegister[] QRegisters = new ArmRegister[32]
        {
            ArmRegister.Q0,
            ArmRegister.Q1,
            ArmRegister.Q2,
            ArmRegister.Q3,
            ArmRegister.Q4,
            ArmRegister.Q5,
            ArmRegister.Q6,
            ArmRegister.Q7,
            ArmRegister.Q8,
            ArmRegister.Q9,
            ArmRegister.Q10,
            ArmRegister.Q11,
            ArmRegister.Q12,
            ArmRegister.Q13,
            ArmRegister.Q14,
            ArmRegister.Q15,
            ArmRegister.Q16,
            ArmRegister.Q17,
            ArmRegister.Q18,
            ArmRegister.Q19,
            ArmRegister.Q20,
            ArmRegister.Q21,
            ArmRegister.Q22,
            ArmRegister.Q23,
            ArmRegister.Q24,
            ArmRegister.Q25,
            ArmRegister.Q26,
            ArmRegister.Q27,
            ArmRegister.Q28,
            ArmRegister.Q29,
            ArmRegister.Q30,
            ArmRegister.Q31,
        };

        public ulong GetX(int index)
        {
            if ((uint)index > 30)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return GetRegister(XRegisters[index]);
        }

        public void SetX(int index, ulong value)
        {
            if ((uint)index > 30)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            SetRegister(XRegisters[index], value);
        }

        public SimdValue GetQ(int index)
        {
            if ((uint)index > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return GetVector(QRegisters[index]);
        }

        public void SetQ(int index, SimdValue value)
        {
            if ((uint)index > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            SetVector(QRegisters[index], value);
        }

        private ulong GetRegister(ArmRegister register)
        {
            byte[] data = new byte[8];

            Interface.Checked(Native.Interface.uc_reg_read(uc, (int)register, data));

            return (ulong)BitConverter.ToInt64(data, 0);
        }

        private void SetRegister(ArmRegister register, ulong value)
        {
            byte[] data = BitConverter.GetBytes(value);

            Interface.Checked(Interface.uc_reg_write(uc, (int)register, data));
        }

        private SimdValue GetVector(ArmRegister register)
        {
            byte[] data = new byte[16];

            Interface.Checked(Interface.uc_reg_read(uc, (int)register, data));

            return new SimdValue(data);
        }

        private void SetVector(ArmRegister register, SimdValue value)
        {
            byte[] data = value.ToArray();

            Interface.Checked(Interface.uc_reg_write(uc, (int)register, data));
        }

        public byte[] MemoryRead(ulong address, ulong size)
        {
            byte[] value = new byte[size];

            Interface.Checked(Interface.uc_mem_read(uc, address, value, size));

            return value;
        }

        public byte   MemoryRead8 (ulong address) => MemoryRead(address, 1)[0];
        public UInt16 MemoryRead16(ulong address) => (UInt16)BitConverter.ToInt16(MemoryRead(address, 2), 0);
        public UInt32 MemoryRead32(ulong address) => (UInt32)BitConverter.ToInt32(MemoryRead(address, 4), 0);
        public UInt64 MemoryRead64(ulong address) => (UInt64)BitConverter.ToInt64(MemoryRead(address, 8), 0);

        public void MemoryWrite(ulong address, byte[] value)
        {
            Interface.Checked(Interface.uc_mem_write(uc, address, value, (ulong)value.Length));
        }

        public void MemoryWrite8 (ulong address, byte value)   => MemoryWrite(address, new byte[]{value});
        public void MemoryWrite16(ulong address, Int16 value)  => MemoryWrite(address, BitConverter.GetBytes(value));
        public void MemoryWrite16(ulong address, UInt16 value) => MemoryWrite(address, BitConverter.GetBytes(value));
        public void MemoryWrite32(ulong address, Int32 value)  => MemoryWrite(address, BitConverter.GetBytes(value));
        public void MemoryWrite32(ulong address, UInt32 value) => MemoryWrite(address, BitConverter.GetBytes(value));
        public void MemoryWrite64(ulong address, Int64 value)  => MemoryWrite(address, BitConverter.GetBytes(value));
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