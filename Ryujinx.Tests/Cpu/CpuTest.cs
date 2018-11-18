using ChocolArm64;
using ChocolArm64.Memory;
using ChocolArm64.State;

using NUnit.Framework;

using Ryujinx.Tests.Unicorn;

using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading;

namespace Ryujinx.Tests.Cpu
{
    [TestFixture]
    public class CpuTest
    {
        protected long Position { get; private set; }
        private long _size;

        private long _entryPoint;

        private IntPtr _ramPointer;

        private MemoryManager _memory;
        private CpuThread     _thread;

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
            Position = 0x1000;
            _size    = 0x1000;

            _entryPoint = Position;

            Translator translator = new Translator();
            _ramPointer = Marshal.AllocHGlobal(new IntPtr(_size));
            _memory = new MemoryManager(_ramPointer);
            _memory.Map(Position, 0, _size);
            _thread = new CpuThread(translator, _memory, _entryPoint);

            if (_unicornAvailable)
            {
                _unicornEmu = new UnicornAArch64();
                _unicornEmu.MemoryMap((ulong)Position, (ulong)_size, MemoryPermission.READ | MemoryPermission.EXEC);
                _unicornEmu.PC = (ulong)_entryPoint;
            }
        }

        [TearDown]
        public void Teardown()
        {
            Marshal.FreeHGlobal(_ramPointer);
            _memory     = null;
            _thread     = null;
            _unicornEmu = null;
        }

        protected void Reset()
        {
            Teardown();
            Setup();
        }

        protected void Opcode(uint opcode)
        {
            _thread.Memory.WriteUInt32(Position, opcode);

            if (_unicornAvailable)
            {
                _unicornEmu.MemoryWrite32((ulong)Position, opcode);
            }

            Position += 4;
        }

        protected void SetThreadState(ulong x0 = 0, ulong x1 = 0, ulong x2 = 0, ulong x3 = 0, ulong x31 = 0,
                                      Vector128<float> v0 = default(Vector128<float>),
                                      Vector128<float> v1 = default(Vector128<float>),
                                      Vector128<float> v2 = default(Vector128<float>),
                                      Vector128<float> v3 = default(Vector128<float>),
                                      bool overflow = false, bool carry = false, bool zero = false, bool negative = false,
                                      int fpcr = 0x0, int fpsr = 0x0)
        {
            _thread.ThreadState.X0 = x0;
            _thread.ThreadState.X1 = x1;
            _thread.ThreadState.X2 = x2;
            _thread.ThreadState.X3 = x3;

            _thread.ThreadState.X31 = x31;

            _thread.ThreadState.V0 = v0;
            _thread.ThreadState.V1 = v1;
            _thread.ThreadState.V2 = v2;
            _thread.ThreadState.V3 = v3;

            _thread.ThreadState.Overflow = overflow;
            _thread.ThreadState.Carry    = carry;
            _thread.ThreadState.Zero     = zero;
            _thread.ThreadState.Negative = negative;

            _thread.ThreadState.Fpcr = fpcr;
            _thread.ThreadState.Fpsr = fpsr;

            if (_unicornAvailable)
            {
                _unicornEmu.X[0] = x0;
                _unicornEmu.X[1] = x1;
                _unicornEmu.X[2] = x2;
                _unicornEmu.X[3] = x3;

                _unicornEmu.SP = x31;

                _unicornEmu.Q[0] = v0;
                _unicornEmu.Q[1] = v1;
                _unicornEmu.Q[2] = v2;
                _unicornEmu.Q[3] = v3;

                _unicornEmu.OverflowFlag = overflow;
                _unicornEmu.CarryFlag    = carry;
                _unicornEmu.ZeroFlag     = zero;
                _unicornEmu.NegativeFlag = negative;

                _unicornEmu.Fpcr = fpcr;
                _unicornEmu.Fpsr = fpsr;
            }
        }

        protected void ExecuteOpcodes()
        {
            using (ManualResetEvent wait = new ManualResetEvent(false))
            {
                _thread.ThreadState.Break += (sender, e) => _thread.StopExecution();
                _thread.WorkFinished += (sender, e) => wait.Set();

                _thread.Execute();
                wait.WaitOne();
            }

            if (_unicornAvailable)
            {
                _unicornEmu.RunForCount((ulong)(Position - _entryPoint - 8) / 4);
            }
        }

        protected CpuThreadState GetThreadState() => _thread.ThreadState;

        protected CpuThreadState SingleOpcode(uint opcode,
                                              ulong x0 = 0, ulong x1 = 0, ulong x2 = 0, ulong x3 = 0, ulong x31 = 0,
                                              Vector128<float> v0 = default(Vector128<float>),
                                              Vector128<float> v1 = default(Vector128<float>),
                                              Vector128<float> v2 = default(Vector128<float>),
                                              Vector128<float> v3 = default(Vector128<float>),
                                              bool overflow = false, bool carry = false, bool zero = false, bool negative = false,
                                              int fpcr = 0x0, int fpsr = 0x0)
        {
            Opcode(opcode);
            Opcode(0xD4200000); // BRK #0
            Opcode(0xD65F03C0); // RET
            SetThreadState(x0, x1, x2, x3, x31, v0, v1, v2, v3, overflow, carry, zero, negative, fpcr, fpsr);
            ExecuteOpcodes();

            return GetThreadState();
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

            Assert.That(_thread.ThreadState.X0,  Is.EqualTo(_unicornEmu.X[0]));
            Assert.That(_thread.ThreadState.X1,  Is.EqualTo(_unicornEmu.X[1]));
            Assert.That(_thread.ThreadState.X2,  Is.EqualTo(_unicornEmu.X[2]));
            Assert.That(_thread.ThreadState.X3,  Is.EqualTo(_unicornEmu.X[3]));
            Assert.That(_thread.ThreadState.X4,  Is.EqualTo(_unicornEmu.X[4]));
            Assert.That(_thread.ThreadState.X5,  Is.EqualTo(_unicornEmu.X[5]));
            Assert.That(_thread.ThreadState.X6,  Is.EqualTo(_unicornEmu.X[6]));
            Assert.That(_thread.ThreadState.X7,  Is.EqualTo(_unicornEmu.X[7]));
            Assert.That(_thread.ThreadState.X8,  Is.EqualTo(_unicornEmu.X[8]));
            Assert.That(_thread.ThreadState.X9,  Is.EqualTo(_unicornEmu.X[9]));
            Assert.That(_thread.ThreadState.X10, Is.EqualTo(_unicornEmu.X[10]));
            Assert.That(_thread.ThreadState.X11, Is.EqualTo(_unicornEmu.X[11]));
            Assert.That(_thread.ThreadState.X12, Is.EqualTo(_unicornEmu.X[12]));
            Assert.That(_thread.ThreadState.X13, Is.EqualTo(_unicornEmu.X[13]));
            Assert.That(_thread.ThreadState.X14, Is.EqualTo(_unicornEmu.X[14]));
            Assert.That(_thread.ThreadState.X15, Is.EqualTo(_unicornEmu.X[15]));
            Assert.That(_thread.ThreadState.X16, Is.EqualTo(_unicornEmu.X[16]));
            Assert.That(_thread.ThreadState.X17, Is.EqualTo(_unicornEmu.X[17]));
            Assert.That(_thread.ThreadState.X18, Is.EqualTo(_unicornEmu.X[18]));
            Assert.That(_thread.ThreadState.X19, Is.EqualTo(_unicornEmu.X[19]));
            Assert.That(_thread.ThreadState.X20, Is.EqualTo(_unicornEmu.X[20]));
            Assert.That(_thread.ThreadState.X21, Is.EqualTo(_unicornEmu.X[21]));
            Assert.That(_thread.ThreadState.X22, Is.EqualTo(_unicornEmu.X[22]));
            Assert.That(_thread.ThreadState.X23, Is.EqualTo(_unicornEmu.X[23]));
            Assert.That(_thread.ThreadState.X24, Is.EqualTo(_unicornEmu.X[24]));
            Assert.That(_thread.ThreadState.X25, Is.EqualTo(_unicornEmu.X[25]));
            Assert.That(_thread.ThreadState.X26, Is.EqualTo(_unicornEmu.X[26]));
            Assert.That(_thread.ThreadState.X27, Is.EqualTo(_unicornEmu.X[27]));
            Assert.That(_thread.ThreadState.X28, Is.EqualTo(_unicornEmu.X[28]));
            Assert.That(_thread.ThreadState.X29, Is.EqualTo(_unicornEmu.X[29]));
            Assert.That(_thread.ThreadState.X30, Is.EqualTo(_unicornEmu.X[30]));

            Assert.That(_thread.ThreadState.X31, Is.EqualTo(_unicornEmu.SP));

            if (fpTolerances == FpTolerances.None)
            {
                Assert.That(_thread.ThreadState.V0, Is.EqualTo(_unicornEmu.Q[0]));
            }
            else
            {
                ManageFpTolerances(fpTolerances);
            }
            Assert.That(_thread.ThreadState.V1,  Is.EqualTo(_unicornEmu.Q[1]));
            Assert.That(_thread.ThreadState.V2,  Is.EqualTo(_unicornEmu.Q[2]));
            Assert.That(_thread.ThreadState.V3,  Is.EqualTo(_unicornEmu.Q[3]));
            Assert.That(_thread.ThreadState.V4,  Is.EqualTo(_unicornEmu.Q[4]));
            Assert.That(_thread.ThreadState.V5,  Is.EqualTo(_unicornEmu.Q[5]));
            Assert.That(_thread.ThreadState.V6,  Is.EqualTo(_unicornEmu.Q[6]));
            Assert.That(_thread.ThreadState.V7,  Is.EqualTo(_unicornEmu.Q[7]));
            Assert.That(_thread.ThreadState.V8,  Is.EqualTo(_unicornEmu.Q[8]));
            Assert.That(_thread.ThreadState.V9,  Is.EqualTo(_unicornEmu.Q[9]));
            Assert.That(_thread.ThreadState.V10, Is.EqualTo(_unicornEmu.Q[10]));
            Assert.That(_thread.ThreadState.V11, Is.EqualTo(_unicornEmu.Q[11]));
            Assert.That(_thread.ThreadState.V12, Is.EqualTo(_unicornEmu.Q[12]));
            Assert.That(_thread.ThreadState.V13, Is.EqualTo(_unicornEmu.Q[13]));
            Assert.That(_thread.ThreadState.V14, Is.EqualTo(_unicornEmu.Q[14]));
            Assert.That(_thread.ThreadState.V15, Is.EqualTo(_unicornEmu.Q[15]));
            Assert.That(_thread.ThreadState.V16, Is.EqualTo(_unicornEmu.Q[16]));
            Assert.That(_thread.ThreadState.V17, Is.EqualTo(_unicornEmu.Q[17]));
            Assert.That(_thread.ThreadState.V18, Is.EqualTo(_unicornEmu.Q[18]));
            Assert.That(_thread.ThreadState.V19, Is.EqualTo(_unicornEmu.Q[19]));
            Assert.That(_thread.ThreadState.V20, Is.EqualTo(_unicornEmu.Q[20]));
            Assert.That(_thread.ThreadState.V21, Is.EqualTo(_unicornEmu.Q[21]));
            Assert.That(_thread.ThreadState.V22, Is.EqualTo(_unicornEmu.Q[22]));
            Assert.That(_thread.ThreadState.V23, Is.EqualTo(_unicornEmu.Q[23]));
            Assert.That(_thread.ThreadState.V24, Is.EqualTo(_unicornEmu.Q[24]));
            Assert.That(_thread.ThreadState.V25, Is.EqualTo(_unicornEmu.Q[25]));
            Assert.That(_thread.ThreadState.V26, Is.EqualTo(_unicornEmu.Q[26]));
            Assert.That(_thread.ThreadState.V27, Is.EqualTo(_unicornEmu.Q[27]));
            Assert.That(_thread.ThreadState.V28, Is.EqualTo(_unicornEmu.Q[28]));
            Assert.That(_thread.ThreadState.V29, Is.EqualTo(_unicornEmu.Q[29]));
            Assert.That(_thread.ThreadState.V30, Is.EqualTo(_unicornEmu.Q[30]));
            Assert.That(_thread.ThreadState.V31, Is.EqualTo(_unicornEmu.Q[31]));

            Assert.That(_thread.ThreadState.Fpcr,                 Is.EqualTo(_unicornEmu.Fpcr));
            Assert.That(_thread.ThreadState.Fpsr & (int)fpsrMask, Is.EqualTo(_unicornEmu.Fpsr & (int)fpsrMask));

            Assert.That(_thread.ThreadState.Overflow, Is.EqualTo(_unicornEmu.OverflowFlag));
            Assert.That(_thread.ThreadState.Carry,    Is.EqualTo(_unicornEmu.CarryFlag));
            Assert.That(_thread.ThreadState.Zero,     Is.EqualTo(_unicornEmu.ZeroFlag));
            Assert.That(_thread.ThreadState.Negative, Is.EqualTo(_unicornEmu.NegativeFlag));
        }

        private void ManageFpSkips(FpSkips fpSkips)
        {
            if (fpSkips.HasFlag(FpSkips.IfNaNS))
            {
                if (float.IsNaN(VectorExtractSingle(_unicornEmu.Q[0], (byte)0)))
                {
                    Assert.Ignore("NaN test.");
                }
            }
            else if (fpSkips.HasFlag(FpSkips.IfNaND))
            {
                if (double.IsNaN(VectorExtractDouble(_unicornEmu.Q[0], (byte)0)))
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
            if (!Is.EqualTo(_unicornEmu.Q[0]).ApplyTo(_thread.ThreadState.V0).IsSuccess)
            {
                if (fpTolerances == FpTolerances.UpToOneUlpsS)
                {
                    if (IsNormalOrSubnormalS(VectorExtractSingle(_unicornEmu.Q[0],       (byte)0)) &&
                        IsNormalOrSubnormalS(VectorExtractSingle(_thread.ThreadState.V0, (byte)0)))
                    {
                        Assert.That   (VectorExtractSingle(_thread.ThreadState.V0, (byte)0),
                            Is.EqualTo(VectorExtractSingle(_unicornEmu.Q[0],       (byte)0)).Within(1).Ulps);
                        Assert.That   (VectorExtractSingle(_thread.ThreadState.V0, (byte)1),
                            Is.EqualTo(VectorExtractSingle(_unicornEmu.Q[0],       (byte)1)).Within(1).Ulps);
                        Assert.That   (VectorExtractSingle(_thread.ThreadState.V0, (byte)2),
                            Is.EqualTo(VectorExtractSingle(_unicornEmu.Q[0],       (byte)2)).Within(1).Ulps);
                        Assert.That   (VectorExtractSingle(_thread.ThreadState.V0, (byte)3),
                            Is.EqualTo(VectorExtractSingle(_unicornEmu.Q[0],       (byte)3)).Within(1).Ulps);

                        Console.WriteLine(fpTolerances);
                    }
                    else
                    {
                        Assert.That(_thread.ThreadState.V0, Is.EqualTo(_unicornEmu.Q[0]));
                    }
                }

                if (fpTolerances == FpTolerances.UpToOneUlpsD)
                {
                    if (IsNormalOrSubnormalD(VectorExtractDouble(_unicornEmu.Q[0],       (byte)0)) &&
                        IsNormalOrSubnormalD(VectorExtractDouble(_thread.ThreadState.V0, (byte)0)))
                    {
                        Assert.That   (VectorExtractDouble(_thread.ThreadState.V0, (byte)0),
                            Is.EqualTo(VectorExtractDouble(_unicornEmu.Q[0],       (byte)0)).Within(1).Ulps);
                        Assert.That   (VectorExtractDouble(_thread.ThreadState.V0, (byte)1),
                            Is.EqualTo(VectorExtractDouble(_unicornEmu.Q[0],       (byte)1)).Within(1).Ulps);

                        Console.WriteLine(fpTolerances);
                    }
                    else
                    {
                        Assert.That(_thread.ThreadState.V0, Is.EqualTo(_unicornEmu.Q[0]));
                    }
                }
            }

            bool IsNormalOrSubnormalS(float f)  => float.IsNormal(f)  || float.IsSubnormal(f);

            bool IsNormalOrSubnormalD(double d) => double.IsNormal(d) || double.IsSubnormal(d);
        }

        protected static Vector128<float> MakeVectorE0(double e0)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<long, float>(Sse2.SetVector128(0, BitConverter.DoubleToInt64Bits(e0)));
        }

        protected static Vector128<float> MakeVectorE0E1(double e0, double e1)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<long, float>(
                Sse2.SetVector128(BitConverter.DoubleToInt64Bits(e1), BitConverter.DoubleToInt64Bits(e0)));
        }

        protected static Vector128<float> MakeVectorE1(double e1)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<long, float>(Sse2.SetVector128(BitConverter.DoubleToInt64Bits(e1), 0));
        }

        protected static float VectorExtractSingle(Vector128<float> vector, byte index)
        {
            if (!Sse41.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            int value = Sse41.Extract(Sse.StaticCast<float, int>(vector), index);

            return BitConverter.Int32BitsToSingle(value);
        }

        protected static double VectorExtractDouble(Vector128<float> vector, byte index)
        {
            if (!Sse41.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            long value = Sse41.Extract(Sse.StaticCast<float, long>(vector), index);

            return BitConverter.Int64BitsToDouble(value);
        }

        protected static Vector128<float> MakeVectorE0(ulong e0)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<ulong, float>(Sse2.SetVector128(0, e0));
        }

        protected static Vector128<float> MakeVectorE0E1(ulong e0, ulong e1)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<ulong, float>(Sse2.SetVector128(e1, e0));
        }

        protected static Vector128<float> MakeVectorE1(ulong e1)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<ulong, float>(Sse2.SetVector128(e1, 0));
        }

        protected static ulong GetVectorE0(Vector128<float> vector)
        {
            if (!Sse41.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse41.Extract(Sse.StaticCast<float, ulong>(vector), (byte)0);
        }

        protected static ulong GetVectorE1(Vector128<float> vector)
        {
            if (!Sse41.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse41.Extract(Sse.StaticCast<float, ulong>(vector), (byte)1);
        }

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
