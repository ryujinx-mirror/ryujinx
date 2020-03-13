using ARMeilleure.Memory;
using ARMeilleure.State;
using ARMeilleure.Translation;
using NUnit.Framework;
using Ryujinx.Tests.Unicorn;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Tests.Cpu
{
    [TestFixture]
    public class CpuTest32
    {
        private uint _currAddress;
        private long _size;

        private uint _entryPoint;

        private IntPtr _ramPointer;

        private MemoryManager _memory;

        private ExecutionContext _context;

        private Translator _translator;

        private static bool _unicornAvailable;
        private UnicornAArch32 _unicornEmu;

        private bool usingMemory;

        static CpuTest32()
        {
            _unicornAvailable = UnicornAArch32.IsAvailable();

            if (!_unicornAvailable)
            {
                Console.WriteLine("WARNING: Could not find Unicorn.");
            }
        }

        [SetUp]
        public void Setup()
        {
            _currAddress = 0x1000;
            _size = 0x1000;

            _entryPoint = _currAddress;

            _ramPointer = Marshal.AllocHGlobal(new IntPtr(_size * 2));
            _memory = new MemoryManager(_ramPointer, addressSpaceBits: 16, useFlatPageTable: true);
            _memory.Map((long)_currAddress, 0, _size*2);

            _context = new ExecutionContext();
            _context.IsAarch32 = true;

            _translator = new Translator(_memory);

            if (_unicornAvailable)
            {
                _unicornEmu = new UnicornAArch32();
                _unicornEmu.MemoryMap(_currAddress, (ulong)_size, MemoryPermission.READ | MemoryPermission.EXEC);
                _unicornEmu.MemoryMap((ulong)(_currAddress + _size), (ulong)_size, MemoryPermission.READ | MemoryPermission.WRITE);
                _unicornEmu.PC = _entryPoint;
            }
        }

        [TearDown]
        public void Teardown()
        {
            Marshal.FreeHGlobal(_ramPointer);
            _memory = null;
            _context = null;
            _translator = null;
            _unicornEmu = null;
        }

        protected void Reset()
        {
            Teardown();
            Setup();
        }

        protected void Opcode(uint opcode)
        {
            _memory.WriteUInt32((long)_currAddress, opcode);

            if (_unicornAvailable)
            {
                _unicornEmu.MemoryWrite32((ulong)_currAddress, opcode);
            }

            _currAddress += 4;
        }

        protected ExecutionContext GetContext() => _context;
        protected void SetContext(uint r0 = 0,
                                  uint r1 = 0,
                                  uint r2 = 0,
                                  uint r3 = 0,
                                  uint sp = 0,
                                  V128 v0 = default,
                                  V128 v1 = default,
                                  V128 v2 = default,
                                  V128 v3 = default,
                                  V128 v4 = default,
                                  V128 v5 = default,
                                  V128 v14 = default,
                                  V128 v15 = default,
                                  bool overflow = false,
                                  bool carry = false,
                                  bool zero = false,
                                  bool negative = false,
                                  int fpscr = 0)
        {
            _context.SetX(0, r0);
            _context.SetX(1, r1);
            _context.SetX(2, r2);
            _context.SetX(3, r3);

            _context.SetX(0xd, sp);

            _context.SetV(0, v0);
            _context.SetV(1, v1);
            _context.SetV(2, v2);
            _context.SetV(3, v3);
            _context.SetV(4, v4);
            _context.SetV(5, v5);
            _context.SetV(14, v14);
            _context.SetV(15, v15);

            _context.SetPstateFlag(PState.VFlag, overflow);
            _context.SetPstateFlag(PState.CFlag, carry);
            _context.SetPstateFlag(PState.ZFlag, zero);
            _context.SetPstateFlag(PState.NFlag, negative);

            _context.Fpsr = FPSR.A32Mask & (FPSR)fpscr;
            _context.Fpcr = FPCR.A32Mask & (FPCR)fpscr;

            if (_unicornAvailable)
            {
                _unicornEmu.R[0] = r0;
                _unicornEmu.R[1] = r1;
                _unicornEmu.R[2] = r2;
                _unicornEmu.R[3] = r3;

                _unicornEmu.SP = sp;

                _unicornEmu.Q[0] = V128ToSimdValue(v0);
                _unicornEmu.Q[1] = V128ToSimdValue(v1);
                _unicornEmu.Q[2] = V128ToSimdValue(v2);
                _unicornEmu.Q[3] = V128ToSimdValue(v3);
                _unicornEmu.Q[4] = V128ToSimdValue(v4);
                _unicornEmu.Q[5] = V128ToSimdValue(v5);
                _unicornEmu.Q[14] = V128ToSimdValue(v14);
                _unicornEmu.Q[15] = V128ToSimdValue(v15);

                _unicornEmu.OverflowFlag = overflow;
                _unicornEmu.CarryFlag = carry;
                _unicornEmu.ZeroFlag = zero;
                _unicornEmu.NegativeFlag = negative;

                _unicornEmu.Fpscr = fpscr;
            }
        }

        protected void ExecuteOpcodes(bool runUnicorn = true)
        {
            _translator.Execute(_context, _entryPoint);

            if (_unicornAvailable && runUnicorn)
            {
                _unicornEmu.RunForCount((ulong)(_currAddress - _entryPoint - 4) / 4);
            }
        }

        protected ExecutionContext SingleOpcode(uint opcode,
                                                uint r0 = 0,
                                                uint r1 = 0,
                                                uint r2 = 0,
                                                uint r3 = 0,
                                                uint sp = 0,
                                                V128 v0 = default,
                                                V128 v1 = default,
                                                V128 v2 = default,
                                                V128 v3 = default,
                                                V128 v4 = default,
                                                V128 v5 = default,
                                                V128 v14 = default,
                                                V128 v15 = default,
                                                bool overflow = false,
                                                bool carry = false,
                                                bool zero = false,
                                                bool negative = false,
                                                int fpscr = 0,
                                                bool copyFpFlags = false,
                                                bool runUnicorn = true)
        {
            Opcode(opcode);
            if (copyFpFlags)
            {
                Opcode(0xeef1fa10);
            }
            Opcode(0xe12fff1e); // BX LR
            SetContext(r0, r1, r2, r3, sp, v0, v1, v2, v3, v4, v5, v14, v15, overflow, carry, zero, negative, fpscr);
            ExecuteOpcodes(runUnicorn);

            return GetContext();
        }

        protected void SetWorkingMemory(byte[] data)
        {
            _memory.WriteBytes(0x2000, data);

            if (_unicornAvailable)
            {
                _unicornEmu.MemoryWrite((ulong)(0x2000), data);
            }

            usingMemory = true; // When true, CompareAgainstUnicorn checks the working memory for equality too.
        }

        /// <summary>Rounding Mode control field.</summary>
        public enum RMode
        {
            /// <summary>Round to Nearest mode.</summary>
            Rn,
            /// <summary>Round towards Plus Infinity mode.</summary>
            Rp,
            /// <summary>Round towards Minus Infinity mode.</summary>
            Rm,
            /// <summary>Round towards Zero mode.</summary>
            Rz
        };

        /// <summary>Floating-point Control Register.</summary>
        protected enum Fpcr
        {
            /// <summary>Rounding Mode control field.</summary>
            RMode = 22,
            /// <summary>Flush-to-zero mode control bit.</summary>
            Fz = 24,
            /// <summary>Default NaN mode control bit.</summary>
            Dn = 25,
            /// <summary>Alternative half-precision control bit.</summary>
            Ahp = 26
        }

        /// <summary>Floating-point Status Register.</summary>
        [Flags]
        protected enum Fpsr
        {
            None = 0,

            /// <summary>Invalid Operation cumulative floating-point exception bit.</summary>
            Ioc = 1 << 0,
            /// <summary>Divide by Zero cumulative floating-point exception bit.</summary>
            Dzc = 1 << 1,
            /// <summary>Overflow cumulative floating-point exception bit.</summary>
            Ofc = 1 << 2,
            /// <summary>Underflow cumulative floating-point exception bit.</summary>
            Ufc = 1 << 3,
            /// <summary>Inexact cumulative floating-point exception bit.</summary>
            Ixc = 1 << 4,
            /// <summary>Input Denormal cumulative floating-point exception bit.</summary>
            Idc = 1 << 7,

            /// <summary>Cumulative saturation bit.</summary>
            Qc = 1 << 27,

            /// <summary>NZCV flags</summary>
            Nzcv = (1 << 28) | (1 << 29) | (1 << 30) | (1 << 31)
        }

        [Flags]
        protected enum FpSkips
        {
            None = 0,

            IfNaNS = 1,
            IfNaND = 2,

            IfUnderflow = 4,
            IfOverflow = 8
        }

        protected enum FpTolerances
        {
            None,

            UpToOneUlpsS,
            UpToOneUlpsD
        }

        protected void CompareAgainstUnicorn(
            Fpsr fpsrMask = Fpsr.None,
            FpSkips fpSkips = FpSkips.None,
            FpTolerances fpTolerances = FpTolerances.None)
        {
            if (!_unicornAvailable)
            {
                return;
            }

            if (fpSkips != FpSkips.None)
            {
                ManageFpSkips(fpSkips);
            }

            Assert.That(_context.GetX(0), Is.EqualTo(_unicornEmu.R[0]));
            Assert.That(_context.GetX(1), Is.EqualTo(_unicornEmu.R[1]));
            Assert.That(_context.GetX(2), Is.EqualTo(_unicornEmu.R[2]));
            Assert.That(_context.GetX(3), Is.EqualTo(_unicornEmu.R[3]));
            Assert.That(_context.GetX(4), Is.EqualTo(_unicornEmu.R[4]));
            Assert.That(_context.GetX(5), Is.EqualTo(_unicornEmu.R[5]));
            Assert.That(_context.GetX(6), Is.EqualTo(_unicornEmu.R[6]));
            Assert.That(_context.GetX(7), Is.EqualTo(_unicornEmu.R[7]));
            Assert.That(_context.GetX(8), Is.EqualTo(_unicornEmu.R[8]));
            Assert.That(_context.GetX(9), Is.EqualTo(_unicornEmu.R[9]));
            Assert.That(_context.GetX(10), Is.EqualTo(_unicornEmu.R[10]));
            Assert.That(_context.GetX(11), Is.EqualTo(_unicornEmu.R[11]));
            Assert.That(_context.GetX(12), Is.EqualTo(_unicornEmu.R[12]));
            Assert.That(_context.GetX(13), Is.EqualTo(_unicornEmu.R[13]));
            Assert.That(_context.GetX(14), Is.EqualTo(_unicornEmu.R[14]));

            if (fpTolerances == FpTolerances.None)
            {
                Assert.That(V128ToSimdValue(_context.GetV(0)), Is.EqualTo(_unicornEmu.Q[0]));
            }
            else
            {
                ManageFpTolerances(fpTolerances);
            }
            Assert.That(V128ToSimdValue(_context.GetV(1)), Is.EqualTo(_unicornEmu.Q[1]));
            Assert.That(V128ToSimdValue(_context.GetV(2)), Is.EqualTo(_unicornEmu.Q[2]));
            Assert.That(V128ToSimdValue(_context.GetV(3)), Is.EqualTo(_unicornEmu.Q[3]));
            Assert.That(V128ToSimdValue(_context.GetV(4)), Is.EqualTo(_unicornEmu.Q[4]));
            Assert.That(V128ToSimdValue(_context.GetV(5)), Is.EqualTo(_unicornEmu.Q[5]));
            Assert.That(V128ToSimdValue(_context.GetV(6)), Is.EqualTo(_unicornEmu.Q[6]));
            Assert.That(V128ToSimdValue(_context.GetV(7)), Is.EqualTo(_unicornEmu.Q[7]));
            Assert.That(V128ToSimdValue(_context.GetV(8)), Is.EqualTo(_unicornEmu.Q[8]));
            Assert.That(V128ToSimdValue(_context.GetV(9)), Is.EqualTo(_unicornEmu.Q[9]));
            Assert.That(V128ToSimdValue(_context.GetV(10)), Is.EqualTo(_unicornEmu.Q[10]));
            Assert.That(V128ToSimdValue(_context.GetV(11)), Is.EqualTo(_unicornEmu.Q[11]));
            Assert.That(V128ToSimdValue(_context.GetV(12)), Is.EqualTo(_unicornEmu.Q[12]));
            Assert.That(V128ToSimdValue(_context.GetV(13)), Is.EqualTo(_unicornEmu.Q[13]));
            Assert.That(V128ToSimdValue(_context.GetV(14)), Is.EqualTo(_unicornEmu.Q[14]));
            Assert.That(V128ToSimdValue(_context.GetV(15)), Is.EqualTo(_unicornEmu.Q[15]));

            Assert.That((int)_context.Fpcr | ((int)_context.Fpsr & (int)fpsrMask), Is.EqualTo(_unicornEmu.Fpscr));

            Assert.That(_context.GetPstateFlag(PState.QFlag), Is.EqualTo(_unicornEmu.QFlag));
            Assert.That(_context.GetPstateFlag(PState.VFlag), Is.EqualTo(_unicornEmu.OverflowFlag));
            Assert.That(_context.GetPstateFlag(PState.CFlag), Is.EqualTo(_unicornEmu.CarryFlag));
            Assert.That(_context.GetPstateFlag(PState.ZFlag), Is.EqualTo(_unicornEmu.ZeroFlag));
            Assert.That(_context.GetPstateFlag(PState.NFlag), Is.EqualTo(_unicornEmu.NegativeFlag));

            if (usingMemory)
            {
                byte[] meilleureMem = _memory.ReadBytes((long)(0x2000), _size);
                byte[] unicornMem = _unicornEmu.MemoryRead((ulong)(0x2000), (ulong)_size);

                for (int i = 0; i < _size; i++)
                {
                    Assert.AreEqual(meilleureMem[i], unicornMem[i]);
                }
            }
        }

        private void ManageFpSkips(FpSkips fpSkips)
        {
            if (fpSkips.HasFlag(FpSkips.IfNaNS))
            {
                if (float.IsNaN(_unicornEmu.Q[0].AsFloat()))
                {
                    Assert.Ignore("NaN test.");
                }
            }
            else if (fpSkips.HasFlag(FpSkips.IfNaND))
            {
                if (double.IsNaN(_unicornEmu.Q[0].AsDouble()))
                {
                    Assert.Ignore("NaN test.");
                }
            }

            if (fpSkips.HasFlag(FpSkips.IfUnderflow))
            {
                if ((_unicornEmu.Fpscr & (int)Fpsr.Ufc) != 0)
                {
                    Assert.Ignore("Underflow test.");
                }
            }

            if (fpSkips.HasFlag(FpSkips.IfOverflow))
            {
                if ((_unicornEmu.Fpscr & (int)Fpsr.Ofc) != 0)
                {
                    Assert.Ignore("Overflow test.");
                }
            }
        }

        private void ManageFpTolerances(FpTolerances fpTolerances)
        {
            bool IsNormalOrSubnormalS(float f) => float.IsNormal(f) || float.IsSubnormal(f);
            bool IsNormalOrSubnormalD(double d) => double.IsNormal(d) || double.IsSubnormal(d);

            if (!Is.EqualTo(_unicornEmu.Q[0]).ApplyTo(V128ToSimdValue(_context.GetV(0))).IsSuccess)
            {
                if (fpTolerances == FpTolerances.UpToOneUlpsS)
                {
                    if (IsNormalOrSubnormalS(_unicornEmu.Q[0].AsFloat()) &&
                        IsNormalOrSubnormalS(_context.GetV(0).AsFloat()))
                    {
                        Assert.That(_context.GetV(0).GetFloat(0),
                            Is.EqualTo(_unicornEmu.Q[0].GetFloat(0)).Within(1).Ulps);
                        Assert.That(_context.GetV(0).GetFloat(1),
                            Is.EqualTo(_unicornEmu.Q[0].GetFloat(1)).Within(1).Ulps);
                        Assert.That(_context.GetV(0).GetFloat(2),
                            Is.EqualTo(_unicornEmu.Q[0].GetFloat(2)).Within(1).Ulps);
                        Assert.That(_context.GetV(0).GetFloat(3),
                            Is.EqualTo(_unicornEmu.Q[0].GetFloat(3)).Within(1).Ulps);

                        Console.WriteLine(fpTolerances);
                    }
                    else
                    {
                        Assert.That(V128ToSimdValue(_context.GetV(0)), Is.EqualTo(_unicornEmu.Q[0]));
                    }
                }

                if (fpTolerances == FpTolerances.UpToOneUlpsD)
                {
                    if (IsNormalOrSubnormalD(_unicornEmu.Q[0].AsDouble()) &&
                        IsNormalOrSubnormalD(_context.GetV(0).AsDouble()))
                    {
                        Assert.That(_context.GetV(0).GetDouble(0),
                            Is.EqualTo(_unicornEmu.Q[0].GetDouble(0)).Within(1).Ulps);
                        Assert.That(_context.GetV(0).GetDouble(1),
                            Is.EqualTo(_unicornEmu.Q[0].GetDouble(1)).Within(1).Ulps);

                        Console.WriteLine(fpTolerances);
                    }
                    else
                    {
                        Assert.That(V128ToSimdValue(_context.GetV(0)), Is.EqualTo(_unicornEmu.Q[0]));
                    }
                }
            }
        }

        private static SimdValue V128ToSimdValue(V128 value)
        {
            return new SimdValue(value.GetUInt64(0), value.GetUInt64(1));
        }

        protected static V128 MakeVectorScalar(float value) => new V128(value);
        protected static V128 MakeVectorScalar(double value) => new V128(value);

        protected static V128 MakeVectorE0(ulong e0) => new V128(e0, 0);
        protected static V128 MakeVectorE1(ulong e1) => new V128(0, e1);

        protected static V128 MakeVectorE0E1(ulong e0, ulong e1) => new V128(e0, e1);

        protected static ulong GetVectorE0(V128 vector) => vector.GetUInt64(0);
        protected static ulong GetVectorE1(V128 vector) => vector.GetUInt64(1);

        protected static ushort GenNormalH()
        {
            uint rnd;

            do rnd = TestContext.CurrentContext.Random.NextUShort();
            while ((rnd & 0x7C00u) == 0u ||
                   (~rnd & 0x7C00u) == 0u);

            return (ushort)rnd;
        }

        protected static ushort GenSubnormalH()
        {
            uint rnd;

            do rnd = TestContext.CurrentContext.Random.NextUShort();
            while ((rnd & 0x03FFu) == 0u);

            return (ushort)(rnd & 0x83FFu);
        }

        protected static uint GenNormalS()
        {
            uint rnd;

            do rnd = TestContext.CurrentContext.Random.NextUInt();
            while ((rnd & 0x7F800000u) == 0u ||
                   (~rnd & 0x7F800000u) == 0u);

            return rnd;
        }

        protected static uint GenSubnormalS()
        {
            uint rnd;

            do rnd = TestContext.CurrentContext.Random.NextUInt();
            while ((rnd & 0x007FFFFFu) == 0u);

            return rnd & 0x807FFFFFu;
        }

        protected static ulong GenNormalD()
        {
            ulong rnd;

            do rnd = TestContext.CurrentContext.Random.NextULong();
            while ((rnd & 0x7FF0000000000000ul) == 0ul ||
                   (~rnd & 0x7FF0000000000000ul) == 0ul);

            return rnd;
        }

        protected static ulong GenSubnormalD()
        {
            ulong rnd;

            do rnd = TestContext.CurrentContext.Random.NextULong();
            while ((rnd & 0x000FFFFFFFFFFFFFul) == 0ul);

            return rnd & 0x800FFFFFFFFFFFFFul;
        }
    }
}
