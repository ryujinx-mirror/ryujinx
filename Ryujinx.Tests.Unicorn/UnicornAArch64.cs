using System;
using System.Diagnostics.Contracts;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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
                    (int i) => GetX(i),
                    (int i, ulong value) => SetX(i, value));
            }
        }

        public IndexedProperty<int, Vector128<float>> Q
        {
            get
            {
                return new IndexedProperty<int, Vector128<float>>(
                    (int i) => GetQ(i),
                    (int i, Vector128<float> value) => SetQ(i, value));
            }
        }

        public ulong LR
        {
            get { return GetRegister(Native.ArmRegister.LR); }
            set { SetRegister(Native.ArmRegister.LR, value); }
        }

        public ulong SP
        {
            get { return GetRegister(Native.ArmRegister.SP); }
            set { SetRegister(Native.ArmRegister.SP, value); }
        }

        public ulong PC
        {
            get { return GetRegister(Native.ArmRegister.PC); }
            set { SetRegister(Native.ArmRegister.PC, value); }
        }

        public uint Pstate
        {
            get { return (uint)GetRegister(Native.ArmRegister.PSTATE); }
            set { SetRegister(Native.ArmRegister.PSTATE, (uint)value); }
        }

        public int Fpcr
        {
            get { return (int)GetRegister(Native.ArmRegister.FPCR); }
            set { SetRegister(Native.ArmRegister.FPCR, (uint)value); }
        }

        public int Fpsr
        {
            get { return (int)GetRegister(Native.ArmRegister.FPSR); }
            set { SetRegister(Native.ArmRegister.FPSR, (uint)value); }
        }

        public bool OverflowFlag
        {
            get { return (Pstate & 0x10000000u) != 0; }
            set { Pstate = (Pstate & ~0x10000000u) | (value ? 0x10000000u : 0u); }
        }

        public bool CarryFlag
        {
            get { return (Pstate & 0x20000000u) != 0; }
            set { Pstate = (Pstate & ~0x20000000u) | (value ? 0x20000000u : 0u); }
        }

        public bool ZeroFlag
        {
            get { return (Pstate & 0x40000000u) != 0; }
            set { Pstate = (Pstate & ~0x40000000u) | (value ? 0x40000000u : 0u); }
        }

        public bool NegativeFlag
        {
            get { return (Pstate & 0x80000000u) != 0; }
            set { Pstate = (Pstate & ~0x80000000u) | (value ? 0x80000000u : 0u); }
        }

        public UnicornAArch64()
        {
            Native.Interface.Checked(Native.Interface.uc_open((uint)Native.UnicornArch.UC_ARCH_ARM64, (uint)Native.UnicornMode.UC_MODE_LITTLE_ENDIAN, out uc));
            SetRegister(Native.ArmRegister.CPACR_EL1, 0x00300000);
        }

        ~UnicornAArch64()
        {
            Native.Interface.Checked(Native.Interface.uc_close(uc));
        }

        public void RunForCount(ulong count)
        {
            Native.Interface.Checked(Native.Interface.uc_emu_start(uc, this.PC, 0xFFFFFFFFFFFFFFFFu, 0, count));
        }

        public void Step()
        {
            RunForCount(1);
        }

        internal static Native.ArmRegister[] X_registers = new Native.ArmRegister[31]
        {
            Native.ArmRegister.X0,
            Native.ArmRegister.X1,
            Native.ArmRegister.X2,
            Native.ArmRegister.X3,
            Native.ArmRegister.X4,
            Native.ArmRegister.X5,
            Native.ArmRegister.X6,
            Native.ArmRegister.X7,
            Native.ArmRegister.X8,
            Native.ArmRegister.X9,
            Native.ArmRegister.X10,
            Native.ArmRegister.X11,
            Native.ArmRegister.X12,
            Native.ArmRegister.X13,
            Native.ArmRegister.X14,
            Native.ArmRegister.X15,
            Native.ArmRegister.X16,
            Native.ArmRegister.X17,
            Native.ArmRegister.X18,
            Native.ArmRegister.X19,
            Native.ArmRegister.X20,
            Native.ArmRegister.X21,
            Native.ArmRegister.X22,
            Native.ArmRegister.X23,
            Native.ArmRegister.X24,
            Native.ArmRegister.X25,
            Native.ArmRegister.X26,
            Native.ArmRegister.X27,
            Native.ArmRegister.X28,
            Native.ArmRegister.X29,
            Native.ArmRegister.X30,
        };

        internal static Native.ArmRegister[] Q_registers = new Native.ArmRegister[32]
        {
            Native.ArmRegister.Q0,
            Native.ArmRegister.Q1,
            Native.ArmRegister.Q2,
            Native.ArmRegister.Q3,
            Native.ArmRegister.Q4,
            Native.ArmRegister.Q5,
            Native.ArmRegister.Q6,
            Native.ArmRegister.Q7,
            Native.ArmRegister.Q8,
            Native.ArmRegister.Q9,
            Native.ArmRegister.Q10,
            Native.ArmRegister.Q11,
            Native.ArmRegister.Q12,
            Native.ArmRegister.Q13,
            Native.ArmRegister.Q14,
            Native.ArmRegister.Q15,
            Native.ArmRegister.Q16,
            Native.ArmRegister.Q17,
            Native.ArmRegister.Q18,
            Native.ArmRegister.Q19,
            Native.ArmRegister.Q20,
            Native.ArmRegister.Q21,
            Native.ArmRegister.Q22,
            Native.ArmRegister.Q23,
            Native.ArmRegister.Q24,
            Native.ArmRegister.Q25,
            Native.ArmRegister.Q26,
            Native.ArmRegister.Q27,
            Native.ArmRegister.Q28,
            Native.ArmRegister.Q29,
            Native.ArmRegister.Q30,
            Native.ArmRegister.Q31,
        };

        internal ulong GetRegister(Native.ArmRegister register)
        {
            byte[] value_bytes = new byte[8];
            Native.Interface.Checked(Native.Interface.uc_reg_read(uc, (int)register, value_bytes));
            return (ulong)BitConverter.ToInt64(value_bytes, 0);
        }

        internal void SetRegister(Native.ArmRegister register, ulong value)
        {
            byte[] value_bytes = BitConverter.GetBytes(value);
            Native.Interface.Checked(Native.Interface.uc_reg_write(uc, (int)register, value_bytes));
        }

        internal Vector128<float> GetVector(Native.ArmRegister register)
        {
            byte[] value_bytes = new byte[16];
            Native.Interface.Checked(Native.Interface.uc_reg_read(uc, (int)register, value_bytes));
            unsafe
            {
                fixed (byte* p = &value_bytes[0])
                {
                    return Sse.LoadVector128((float*)p);
                }
            }
        }

        internal void SetVector(Native.ArmRegister register, Vector128<float> value)
        {
            byte[] value_bytes = new byte[16];
            unsafe
            {
                fixed (byte* p = &value_bytes[0])
                {
                    Sse.Store((float*)p, value);
                }
            }
            Native.Interface.Checked(Native.Interface.uc_reg_write(uc, (int)register, value_bytes));
        }

        public ulong GetX(int index)
        {
            Contract.Requires(index <= 30, "invalid register");

            return GetRegister(X_registers[index]);
        }

        public void SetX(int index, ulong value)
        {
            Contract.Requires(index <= 30, "invalid register");

            SetRegister(X_registers[index], value);
        }

        public Vector128<float> GetQ(int index)
        {
            Contract.Requires(index <= 31, "invalid vector");

            return GetVector(Q_registers[index]);
        }

        public void SetQ(int index, Vector128<float> value)
        {
            Contract.Requires(index <= 31, "invalid vector");

            SetVector(Q_registers[index], value);
        }

        public byte[] MemoryRead(ulong address, ulong size)
        {
            byte[] value = new byte[size];
            Native.Interface.Checked(Native.Interface.uc_mem_read(uc, address, value, size));
            return value;
        }

        public byte   MemoryRead8 (ulong address) { return MemoryRead(address, 1)[0]; }
        public UInt16 MemoryRead16(ulong address) { return (UInt16)BitConverter.ToInt16(MemoryRead(address, 2), 0); }
        public UInt32 MemoryRead32(ulong address) { return (UInt32)BitConverter.ToInt32(MemoryRead(address, 4), 0); }
        public UInt64 MemoryRead64(ulong address) { return (UInt64)BitConverter.ToInt64(MemoryRead(address, 8), 0); }

        public void MemoryWrite(ulong address, byte[] value)
        {
            Native.Interface.Checked(Native.Interface.uc_mem_write(uc, address, value, (ulong)value.Length));
        }

        public void MemoryWrite8 (ulong address, byte value)   { MemoryWrite(address, new byte[]{value}); }
        public void MemoryWrite16(ulong address, Int16 value)  { MemoryWrite(address, BitConverter.GetBytes(value)); }
        public void MemoryWrite16(ulong address, UInt16 value) { MemoryWrite(address, BitConverter.GetBytes(value)); }
        public void MemoryWrite32(ulong address, Int32 value)  { MemoryWrite(address, BitConverter.GetBytes(value)); }
        public void MemoryWrite32(ulong address, UInt32 value) { MemoryWrite(address, BitConverter.GetBytes(value)); }
        public void MemoryWrite64(ulong address, Int64 value)  { MemoryWrite(address, BitConverter.GetBytes(value)); }
        public void MemoryWrite64(ulong address, UInt64 value) { MemoryWrite(address, BitConverter.GetBytes(value)); }

        public void MemoryMap(ulong address, ulong size, MemoryPermission permissions)
        {
            Native.Interface.Checked(Native.Interface.uc_mem_map(uc, address, size, (uint)permissions));
        }

        public void MemoryUnmap(ulong address, ulong size)
        {
            Native.Interface.Checked(Native.Interface.uc_mem_unmap(uc, address, size));
        }

        public void MemoryProtect(ulong address, ulong size, MemoryPermission permissions)
        {
            Native.Interface.Checked(Native.Interface.uc_mem_protect(uc, address, size, (uint)permissions));
        }

        public void DumpMemoryInformation()
        {
            Native.Interface.Checked(Native.Interface.uc_mem_regions(uc, out IntPtr regions_raw, out uint length));
            Native.Interface.MarshalArrayOf<Native.UnicornMemoryRegion>(regions_raw, (int)length, out var regions);
            foreach (var region in regions)
            {
                Console.WriteLine("region: begin {0:X16} end {1:X16} perms {2:X8}", region.begin, region.end, region.perms);
            }
        }

        public static bool IsAvailable()
        {
            try
            {
                Native.Interface.uc_version(out uint major, out uint minor);
                return true;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
        }
    }
}