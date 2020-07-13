using ARMeilleure.State;
using NUnit.Framework;
using Ryujinx.Cpu;
using Ryujinx.Memory;
using Ryujinx.Tests.Unicorn;
using System;

using MemoryPermission = Ryujinx.Tests.Unicorn.MemoryPermission;

namespace Ryujinx.Tests.Cpu
{
    [TestFixture]
    public class CpuTest
    {
        private ulong _currAddress;
        private ulong _size;

        private ulong _entryPoint;

        private MemoryBlock _ram;

        private MemoryManager _memory;

        private ExecutionContext _context;

        private CpuContext _cpuContext;

        private static bool _unicornAvailable;
        private UnicornAArch64 _unicornEmu;

        static CpuTest()
        {
            _unicornAvailable = UnicornAArch64.IsAvailable();

            if (!_unicornAvailable)
            {
                Console.WriteLine("WARNING: Could not find Unicorn.");
            }
        }

        [SetUp]
        public void Setup()
        {
            _currAddress = 0x1000;
            _size        = 0x1000;

            _entryPoint = _currAddress;

            _ram = new MemoryBlock(_size);
            _memory = new MemoryManager(_ram, 1UL << 16);
            _memory.Map(_currAddress, 0, _size);

            _context = CpuContext.CreateExecutionContext();

            _cpuContext = new CpuContext(_memory);

            if (_unicornAvailable)
            {
                _unicornEmu = new UnicornAArch64();
                _unicornEmu.MemoryMap(_currAddress, _size, MemoryPermission.READ | MemoryPermission.EXEC);
                _unicornEmu.PC = _entryPoint;
            }
        }

        [TearDown]
        public void Teardown()
        {
            _memory.Dispose();
            _context.Dispose();
            _ram.Dispose();

            _memory     = null;
            _context    = null;
            _cpuContext = null;
            _unicornEmu = null;
        }

        protected void Reset()
        {
            Teardown();
            Setup();
        }

        protected void Opcode(uint opcode)
        {
            _memory.Write(_currAddress, opcode);

            if (_unicornAvailable)
            {
                _unicornEmu.MemoryWrite32(_currAddress, opcode);
            }

            _currAddress += 4;
        }

        protected ExecutionContext GetContext() => _context;

        protected void SetContext(ulong x0       = 0,
                                  ulong x1       = 0,
                                  ulong x2       = 0,
                                  ulong x3       = 0,
                                  ulong x31      = 0,
                                  V128  v0       = default,
                                  V128  v1       = default,
                                  V128  v2       = default,
                                  V128  v3       = default,
                                  V128  v4       = default,
                                  V128  v5       = default,
                                  V128  v30      = default,
                                  V128  v31      = default,
                                  bool  overflow = false,
                                  bool  carry    = false,
                                  bool  zero     = false,
                                  bool  negative = false,
                                  int   fpcr     = 0,
                                  int   fpsr     = 0)
        {
            _context.SetX(0, x0);
            _context.SetX(1, x1);
            _context.SetX(2, x2);
            _context.SetX(3, x3);

            _context.SetX(31, x31);

            _context.SetV(0,  v0);
            _context.SetV(1,  v1);
            _context.SetV(2,  v2);
            _context.SetV(3,  v3);
            _context.SetV(4,  v4);
            _context.SetV(5,  v5);
            _context.SetV(30, v30);
            _context.SetV(31, v31);

            _context.SetPstateFlag(PState.VFlag, overflow);
            _context.SetPstateFlag(PState.CFlag, carry);
            _context.SetPstateFlag(PState.ZFlag, zero);
            _context.SetPstateFlag(PState.NFlag, negative);

            _context.Fpcr = (FPCR)fpcr;
            _context.Fpsr = (FPSR)fpsr;

            if (_unicornAvailable)
            {
                _unicornEmu.X[0] = x0;
                _unicornEmu.X[1] = x1;
                _unicornEmu.X[2] = x2;
                _unicornEmu.X[3] = x3;

                _unicornEmu.SP = x31;

                _unicornEmu.Q[0]  = V128ToSimdValue(v0);
                _unicornEmu.Q[1]  = V128ToSimdValue(v1);
                _unicornEmu.Q[2]  = V128ToSimdValue(v2);
                _unicornEmu.Q[3]  = V128ToSimdValue(v3);
                _unicornEmu.Q[4]  = V128ToSimdValue(v4);
                _unicornEmu.Q[5]  = V128ToSimdValue(v5);
                _unicornEmu.Q[30] = V128ToSimdValue(v30);
                _unicornEmu.Q[31] = V128ToSimdValue(v31);

                _unicornEmu.OverflowFlag = overflow;
                _unicornEmu.CarryFlag    = carry;
                _unicornEmu.ZeroFlag     = zero;
                _unicornEmu.NegativeFlag = negative;

                _unicornEmu.Fpcr = fpcr;
                _unicornEmu.Fpsr = fpsr;
            }
        }

        protected void ExecuteOpcodes(bool runUnicorn = true)
        {
            _cpuContext.Execute(_context, _entryPoint);

            if (_unicornAvailable && runUnicorn)
            {
                _unicornEmu.RunForCount((_currAddress - _entryPoint - 4) / 4);
            }
        }

        protected ExecutionContext SingleOpcode(uint  opcode,
                                                ulong x0         = 0,
                                                ulong x1         = 0,
                                                ulong x2         = 0,
                                                ulong x3         = 0,
                                                ulong x31        = 0,
                                                V128  v0         = default,
                                                V128  v1         = default,
                                                V128  v2         = default,
                                                V128  v3         = default,
                                                V128  v4         = default,
                                                V128  v5         = default,
                                                V128  v30        = default,
                                                V128  v31        = default,
                                                bool  overflow   = false,
                                                bool  carry      = false,
                                                bool  zero       = false,
                                                bool  negative   = false,
                                                int   fpcr       = 0,
                                                int   fpsr       = 0,
                                                bool  runUnicorn = true)
        {
            Opcode(opcode);
            Opcode(0xD65F03C0); // RET
            SetContext(x0, x1, x2, x3, x31, v0, v1, v2, v3, v4, v5, v30, v31, overflow, carry, zero, negative, fpcr, fpsr);
            ExecuteOpcodes(runUnicorn);

            return GetContext();
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
            Fz    = 24,
            /// <summary>Default NaN mode control bit.</summary>
            Dn    = 25,
            /// <summary>Alternative half-precision control bit.</summary>
            Ahp   = 26
        }

        /// <summary>Floating-point Status Register.</summary>
        [Flags] protected enum Fpsr
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
            Qc = 1 << 27
        }

        [Flags] protected enum FpSkips
        {
            None = 0,

            IfNaNS = 1,
            IfNaND = 2,

            IfUnderflow = 4,
            IfOverflow  = 8
        }

        protected enum FpTolerances
        {
            None,

            UpToOneUlpsS,
            UpToOneUlpsD
        }

        protected void CompareAgainstUnicorn(
            Fpsr         fpsrMask     = Fpsr.None,
            FpSkips      fpSkips      = FpSkips.None,
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

            Assert.That(_context.GetX(0),  Is.EqualTo(_unicornEmu.X[0]));
            Assert.That(_context.GetX(1),  Is.EqualTo(_unicornEmu.X[1]));
            Assert.That(_context.GetX(2),  Is.EqualTo(_unicornEmu.X[2]));
            Assert.That(_context.GetX(3),  Is.EqualTo(_unicornEmu.X[3]));
            Assert.That(_context.GetX(4),  Is.EqualTo(_unicornEmu.X[4]));
            Assert.That(_context.GetX(5),  Is.EqualTo(_unicornEmu.X[5]));
            Assert.That(_context.GetX(6),  Is.EqualTo(_unicornEmu.X[6]));
            Assert.That(_context.GetX(7),  Is.EqualTo(_unicornEmu.X[7]));
            Assert.That(_context.GetX(8),  Is.EqualTo(_unicornEmu.X[8]));
            Assert.That(_context.GetX(9),  Is.EqualTo(_unicornEmu.X[9]));
            Assert.That(_context.GetX(10), Is.EqualTo(_unicornEmu.X[10]));
            Assert.That(_context.GetX(11), Is.EqualTo(_unicornEmu.X[11]));
            Assert.That(_context.GetX(12), Is.EqualTo(_unicornEmu.X[12]));
            Assert.That(_context.GetX(13), Is.EqualTo(_unicornEmu.X[13]));
            Assert.That(_context.GetX(14), Is.EqualTo(_unicornEmu.X[14]));
            Assert.That(_context.GetX(15), Is.EqualTo(_unicornEmu.X[15]));
            Assert.That(_context.GetX(16), Is.EqualTo(_unicornEmu.X[16]));
            Assert.That(_context.GetX(17), Is.EqualTo(_unicornEmu.X[17]));
            Assert.That(_context.GetX(18), Is.EqualTo(_unicornEmu.X[18]));
            Assert.That(_context.GetX(19), Is.EqualTo(_unicornEmu.X[19]));
            Assert.That(_context.GetX(20), Is.EqualTo(_unicornEmu.X[20]));
            Assert.That(_context.GetX(21), Is.EqualTo(_unicornEmu.X[21]));
            Assert.That(_context.GetX(22), Is.EqualTo(_unicornEmu.X[22]));
            Assert.That(_context.GetX(23), Is.EqualTo(_unicornEmu.X[23]));
            Assert.That(_context.GetX(24), Is.EqualTo(_unicornEmu.X[24]));
            Assert.That(_context.GetX(25), Is.EqualTo(_unicornEmu.X[25]));
            Assert.That(_context.GetX(26), Is.EqualTo(_unicornEmu.X[26]));
            Assert.That(_context.GetX(27), Is.EqualTo(_unicornEmu.X[27]));
            Assert.That(_context.GetX(28), Is.EqualTo(_unicornEmu.X[28]));
            Assert.That(_context.GetX(29), Is.EqualTo(_unicornEmu.X[29]));
            Assert.That(_context.GetX(30), Is.EqualTo(_unicornEmu.X[30]));

            Assert.That(_context.GetX(31), Is.EqualTo(_unicornEmu.SP));

            if (fpTolerances == FpTolerances.None)
            {
                Assert.That(V128ToSimdValue(_context.GetV(0)), Is.EqualTo(_unicornEmu.Q[0]));
            }
            else
            {
                ManageFpTolerances(fpTolerances);
            }
            Assert.That(V128ToSimdValue(_context.GetV(1)),  Is.EqualTo(_unicornEmu.Q[1]));
            Assert.That(V128ToSimdValue(_context.GetV(2)),  Is.EqualTo(_unicornEmu.Q[2]));
            Assert.That(V128ToSimdValue(_context.GetV(3)),  Is.EqualTo(_unicornEmu.Q[3]));
            Assert.That(V128ToSimdValue(_context.GetV(4)),  Is.EqualTo(_unicornEmu.Q[4]));
            Assert.That(V128ToSimdValue(_context.GetV(5)),  Is.EqualTo(_unicornEmu.Q[5]));
            Assert.That(V128ToSimdValue(_context.GetV(6)),  Is.EqualTo(_unicornEmu.Q[6]));
            Assert.That(V128ToSimdValue(_context.GetV(7)),  Is.EqualTo(_unicornEmu.Q[7]));
            Assert.That(V128ToSimdValue(_context.GetV(8)),  Is.EqualTo(_unicornEmu.Q[8]));
            Assert.That(V128ToSimdValue(_context.GetV(9)),  Is.EqualTo(_unicornEmu.Q[9]));
            Assert.That(V128ToSimdValue(_context.GetV(10)), Is.EqualTo(_unicornEmu.Q[10]));
            Assert.That(V128ToSimdValue(_context.GetV(11)), Is.EqualTo(_unicornEmu.Q[11]));
            Assert.That(V128ToSimdValue(_context.GetV(12)), Is.EqualTo(_unicornEmu.Q[12]));
            Assert.That(V128ToSimdValue(_context.GetV(13)), Is.EqualTo(_unicornEmu.Q[13]));
            Assert.That(V128ToSimdValue(_context.GetV(14)), Is.EqualTo(_unicornEmu.Q[14]));
            Assert.That(V128ToSimdValue(_context.GetV(15)), Is.EqualTo(_unicornEmu.Q[15]));
            Assert.That(V128ToSimdValue(_context.GetV(16)), Is.EqualTo(_unicornEmu.Q[16]));
            Assert.That(V128ToSimdValue(_context.GetV(17)), Is.EqualTo(_unicornEmu.Q[17]));
            Assert.That(V128ToSimdValue(_context.GetV(18)), Is.EqualTo(_unicornEmu.Q[18]));
            Assert.That(V128ToSimdValue(_context.GetV(19)), Is.EqualTo(_unicornEmu.Q[19]));
            Assert.That(V128ToSimdValue(_context.GetV(20)), Is.EqualTo(_unicornEmu.Q[20]));
            Assert.That(V128ToSimdValue(_context.GetV(21)), Is.EqualTo(_unicornEmu.Q[21]));
            Assert.That(V128ToSimdValue(_context.GetV(22)), Is.EqualTo(_unicornEmu.Q[22]));
            Assert.That(V128ToSimdValue(_context.GetV(23)), Is.EqualTo(_unicornEmu.Q[23]));
            Assert.That(V128ToSimdValue(_context.GetV(24)), Is.EqualTo(_unicornEmu.Q[24]));
            Assert.That(V128ToSimdValue(_context.GetV(25)), Is.EqualTo(_unicornEmu.Q[25]));
            Assert.That(V128ToSimdValue(_context.GetV(26)), Is.EqualTo(_unicornEmu.Q[26]));
            Assert.That(V128ToSimdValue(_context.GetV(27)), Is.EqualTo(_unicornEmu.Q[27]));
            Assert.That(V128ToSimdValue(_context.GetV(28)), Is.EqualTo(_unicornEmu.Q[28]));
            Assert.That(V128ToSimdValue(_context.GetV(29)), Is.EqualTo(_unicornEmu.Q[29]));
            Assert.That(V128ToSimdValue(_context.GetV(30)), Is.EqualTo(_unicornEmu.Q[30]));
            Assert.That(V128ToSimdValue(_context.GetV(31)), Is.EqualTo(_unicornEmu.Q[31]));

            Assert.That((int)_context.Fpcr,                 Is.EqualTo(_unicornEmu.Fpcr));
            Assert.That((int)_context.Fpsr & (int)fpsrMask, Is.EqualTo(_unicornEmu.Fpsr & (int)fpsrMask));

            Assert.That(_context.GetPstateFlag(PState.VFlag), Is.EqualTo(_unicornEmu.OverflowFlag));
            Assert.That(_context.GetPstateFlag(PState.CFlag), Is.EqualTo(_unicornEmu.CarryFlag));
            Assert.That(_context.GetPstateFlag(PState.ZFlag), Is.EqualTo(_unicornEmu.ZeroFlag));
            Assert.That(_context.GetPstateFlag(PState.NFlag), Is.EqualTo(_unicornEmu.NegativeFlag));
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
                if ((_unicornEmu.Fpsr & (int)Fpsr.Ufc) != 0)
                {
                    Assert.Ignore("Underflow test.");
                }
            }

            if (fpSkips.HasFlag(FpSkips.IfOverflow))
            {
                if ((_unicornEmu.Fpsr & (int)Fpsr.Ofc) != 0)
                {
                    Assert.Ignore("Overflow test.");
                }
            }
        }

        private void ManageFpTolerances(FpTolerances fpTolerances)
        {
            bool IsNormalOrSubnormalS(float f)  => float.IsNormal(f)  || float.IsSubnormal(f);
            bool IsNormalOrSubnormalD(double d) => double.IsNormal(d) || double.IsSubnormal(d);

            if (!Is.EqualTo(_unicornEmu.Q[0]).ApplyTo(V128ToSimdValue(_context.GetV(0))).IsSuccess)
            {
                if (fpTolerances == FpTolerances.UpToOneUlpsS)
                {
                    if (IsNormalOrSubnormalS(_unicornEmu.Q[0].AsFloat()) &&
                        IsNormalOrSubnormalS(_context.GetV(0).As<float>()))
                    {
                        Assert.That   (_context.GetV(0).Extract<float>(0),
                            Is.EqualTo(_unicornEmu.Q[0].GetFloat(0)).Within(1).Ulps);
                        Assert.That   (_context.GetV(0).Extract<float>(1),
                            Is.EqualTo(_unicornEmu.Q[0].GetFloat(1)).Within(1).Ulps);
                        Assert.That   (_context.GetV(0).Extract<float>(2),
                            Is.EqualTo(_unicornEmu.Q[0].GetFloat(2)).Within(1).Ulps);
                        Assert.That   (_context.GetV(0).Extract<float>(3),
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
                        IsNormalOrSubnormalD(_context.GetV(0).As<double>()))
                    {
                        Assert.That   (_context.GetV(0).Extract<double>(0),
                            Is.EqualTo(_unicornEmu.Q[0].GetDouble(0)).Within(1).Ulps);
                        Assert.That   (_context.GetV(0).Extract<double>(1),
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
            return new SimdValue(value.Extract<ulong>(0), value.Extract<ulong>(1));
        }

        protected static V128 MakeVectorScalar(float value)  => new V128(value);
        protected static V128 MakeVectorScalar(double value) => new V128(value);

        protected static V128 MakeVectorE0(ulong e0) => new V128(e0, 0);
        protected static V128 MakeVectorE1(ulong e1) => new V128(0, e1);

        protected static V128 MakeVectorE0E1(ulong e0, ulong e1) => new V128(e0, e1);

        protected static ulong GetVectorE0(V128 vector) => vector.Extract<ulong>(0);
        protected static ulong GetVectorE1(V128 vector) => vector.Extract<ulong>(1);

        protected static ushort GenNormalH()
        {
            uint rnd;

            do       rnd = TestContext.CurrentContext.Random.NextUShort();
            while (( rnd & 0x7C00u) == 0u ||
                   (~rnd & 0x7C00u) == 0u);

            return (ushort)rnd;
        }

        protected static ushort GenSubnormalH()
        {
            uint rnd;

            do      rnd = TestContext.CurrentContext.Random.NextUShort();
            while ((rnd & 0x03FFu) == 0u);

            return (ushort)(rnd & 0x83FFu);
        }

        protected static uint GenNormalS()
        {
            uint rnd;

            do       rnd = TestContext.CurrentContext.Random.NextUInt();
            while (( rnd & 0x7F800000u) == 0u ||
                   (~rnd & 0x7F800000u) == 0u);

            return rnd;
        }

        protected static uint GenSubnormalS()
        {
            uint rnd;

            do      rnd = TestContext.CurrentContext.Random.NextUInt();
            while ((rnd & 0x007FFFFFu) == 0u);

            return rnd & 0x807FFFFFu;
        }

        protected static ulong GenNormalD()
        {
            ulong rnd;

            do       rnd = TestContext.CurrentContext.Random.NextULong();
            while (( rnd & 0x7FF0000000000000ul) == 0ul ||
                   (~rnd & 0x7FF0000000000000ul) == 0ul);

            return rnd;
        }

        protected static ulong GenSubnormalD()
        {
            ulong rnd;

            do      rnd = TestContext.CurrentContext.Random.NextULong();
            while ((rnd & 0x000FFFFFFFFFFFFFul) == 0ul);

            return rnd & 0x800FFFFFFFFFFFFFul;
        }
    }
}
