using Ryujinx.Tests.Unicorn.Native;
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
            get { return GetRegister(ArmRegister.LR); }
            set { SetRegister(ArmRegister.LR, value); }
        }

        public ulong SP
        {
            get { return GetRegister(ArmRegister.SP); }
            set { SetRegister(ArmRegister.SP, value); }
        }

        public ulong PC
        {
            get { return GetRegister(ArmRegister.PC); }
            set { SetRegister(ArmRegister.PC, value); }
        }

        public uint Pstate
        {
            get { return (uint)GetRegister(ArmRegister.PSTATE); }
            set { SetRegister(ArmRegister.PSTATE, (uint)value); }
        }

        public int Fpcr
        {
            get { return (int)GetRegister(ArmRegister.FPCR); }
            set { SetRegister(ArmRegister.FPCR, (uint)value); }
        }

        public int Fpsr
        {
            get { return (int)GetRegister(ArmRegister.FPSR); }
            set { SetRegister(ArmRegister.FPSR, (uint)value); }
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
            Interface.Checked(Interface.uc_open((uint)UnicornArch.UC_ARCH_ARM64, (uint)UnicornMode.UC_MODE_LITTLE_ENDIAN, out uc));
            SetRegister(ArmRegister.CPACR_EL1, 0x00300000);
        }

        ~UnicornAArch64()
        {
            Interface.Checked(Interface.uc_close(uc));
        }

        public void RunForCount(ulong count)
        {
            Interface.Checked(Interface.uc_emu_start(uc, PC, 0xFFFFFFFFFFFFFFFFu, 0, count));
        }

        public void Step()
        {
            RunForCount(1);
        }

        internal static ArmRegister[] X_registers = new ArmRegister[31]
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

        internal static ArmRegister[] Q_registers = new ArmRegister[32]
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

        internal ulong GetRegister(ArmRegister register)
        {
            byte[] value_bytes = new byte[8];
            Interface.Checked(Interface.uc_reg_read(uc, (int)register, value_bytes));
            return (ulong)BitConverter.ToInt64(value_bytes, 0);
        }

        internal void SetRegister(ArmRegister register, ulong value)
        {
            byte[] value_bytes = BitConverter.GetBytes(value);
            Interface.Checked(Interface.uc_reg_write(uc, (int)register, value_bytes));
        }

        internal Vector128<float> GetVector(ArmRegister register)
        {
            byte[] value_bytes = new byte[16];
            Interface.Checked(Interface.uc_reg_read(uc, (int)register, value_bytes));
            unsafe
            {
                fixed (byte* p = &value_bytes[0])
                {
                    return Sse.LoadVector128((float*)p);
                }
            }
        }

        internal void SetVector(ArmRegister register, Vector128<float> value)
        {
            byte[] value_bytes = new byte[16];
            unsafe
            {
                fixed (byte* p = &value_bytes[0])
                {
                    Sse.Store((float*)p, value);
                }
            }
            Interface.Checked(Interface.uc_reg_write(uc, (int)register, value_bytes));
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
            Interface.Checked(Interface.uc_mem_read(uc, address, value, size));
            return value;
        }

        public byte   MemoryRead8 (ulong address) { return MemoryRead(address, 1)[0]; }
        public UInt16 MemoryRead16(ulong address) { return (UInt16)BitConverter.ToInt16(MemoryRead(address, 2), 0); }
        public UInt32 MemoryRead32(ulong address) { return (UInt32)BitConverter.ToInt32(MemoryRead(address, 4), 0); }
        public UInt64 MemoryRead64(ulong address) { return (UInt64)BitConverter.ToInt64(MemoryRead(address, 8), 0); }

        public void MemoryWrite(ulong address, byte[] value)
        {
            Interface.Checked(Interface.uc_mem_write(uc, address, value, (ulong)value.Length));
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

        public void DumpMemoryInformation()
        {
            Interface.Checked(Interface.uc_mem_regions(uc, out IntPtr regions_raw, out uint length));
            Interface.MarshalArrayOf<UnicornMemoryRegion>(regions_raw, (int)length, out var regions);
            foreach (var region in regions)
            {
                Console.WriteLine("region: begin {0:X16} end {1:X16} perms {2:X8}", region.begin, region.end, region.perms);
            }
        }

        public static bool IsAvailable()
        {
            try
            {
                Interface.uc_version(out uint major, out uint minor);
                return true;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
        }
    }
}