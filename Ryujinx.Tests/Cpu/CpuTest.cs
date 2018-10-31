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
        private long Size;

        private long EntryPoint;

        private IntPtr RamPointer;

        private MemoryManager Memory;
        private CpuThread     Thread;

        private static bool UnicornAvailable;
        private UnicornAArch64 UnicornEmu;

        static CpuTest()
        {
            UnicornAvailable = UnicornAArch64.IsAvailable();

            if (!UnicornAvailable)
            {
                Console.WriteLine("WARNING: Could not find Unicorn.");
            }
        }

        [SetUp]
        public void Setup()
        {
            Position = 0x1000;
            Size     = 0x1000;

            EntryPoint = Position;

            Translator Translator = new Translator();
            RamPointer = Marshal.AllocHGlobal(new IntPtr(Size));
            Memory = new MemoryManager(RamPointer);
            Memory.Map(Position, 0, Size);
            Thread = new CpuThread(Translator, Memory, EntryPoint);

            if (UnicornAvailable)
            {
                UnicornEmu = new UnicornAArch64();
                UnicornEmu.MemoryMap((ulong)Position, (ulong)Size, MemoryPermission.READ | MemoryPermission.EXEC);
                UnicornEmu.PC = (ulong)EntryPoint;
            }
        }

        [TearDown]
        public void Teardown()
        {
            Marshal.FreeHGlobal(RamPointer);
            Memory = null;
            Thread = null;
            UnicornEmu = null;
        }

        protected void Reset()
        {
            Teardown();
            Setup();
        }

        protected void Opcode(uint Opcode)
        {
            Thread.Memory.WriteUInt32(Position, Opcode);

            if (UnicornAvailable)
            {
                UnicornEmu.MemoryWrite32((ulong)Position, Opcode);
            }

            Position += 4;
        }

        protected void SetThreadState(ulong X0 = 0, ulong X1 = 0, ulong X2 = 0, ulong X3 = 0, ulong X31 = 0,
                                      Vector128<float> V0 = default(Vector128<float>),
                                      Vector128<float> V1 = default(Vector128<float>),
                                      Vector128<float> V2 = default(Vector128<float>),
                                      Vector128<float> V3 = default(Vector128<float>),
                                      bool Overflow = false, bool Carry = false, bool Zero = false, bool Negative = false,
                                      int Fpcr = 0x0, int Fpsr = 0x0)
        {
            Thread.ThreadState.X0 = X0;
            Thread.ThreadState.X1 = X1;
            Thread.ThreadState.X2 = X2;
            Thread.ThreadState.X3 = X3;

            Thread.ThreadState.X31 = X31;

            Thread.ThreadState.V0 = V0;
            Thread.ThreadState.V1 = V1;
            Thread.ThreadState.V2 = V2;
            Thread.ThreadState.V3 = V3;

            Thread.ThreadState.Overflow = Overflow;
            Thread.ThreadState.Carry    = Carry;
            Thread.ThreadState.Zero     = Zero;
            Thread.ThreadState.Negative = Negative;

            Thread.ThreadState.Fpcr = Fpcr;
            Thread.ThreadState.Fpsr = Fpsr;

            if (UnicornAvailable)
            {
                UnicornEmu.X[0] = X0;
                UnicornEmu.X[1] = X1;
                UnicornEmu.X[2] = X2;
                UnicornEmu.X[3] = X3;

                UnicornEmu.SP = X31;

                UnicornEmu.Q[0] = V0;
                UnicornEmu.Q[1] = V1;
                UnicornEmu.Q[2] = V2;
                UnicornEmu.Q[3] = V3;

                UnicornEmu.OverflowFlag = Overflow;
                UnicornEmu.CarryFlag    = Carry;
                UnicornEmu.ZeroFlag     = Zero;
                UnicornEmu.NegativeFlag = Negative;

                UnicornEmu.Fpcr = Fpcr;
                UnicornEmu.Fpsr = Fpsr;
            }
        }

        protected void ExecuteOpcodes()
        {
            using (ManualResetEvent Wait = new ManualResetEvent(false))
            {
                Thread.ThreadState.Break += (sender, e) => Thread.StopExecution();
                Thread.WorkFinished += (sender, e) => Wait.Set();

                Thread.Execute();
                Wait.WaitOne();
            }

            if (UnicornAvailable)
            {
                UnicornEmu.RunForCount((ulong)(Position - EntryPoint - 8) / 4);
            }
        }

        protected CpuThreadState GetThreadState() => Thread.ThreadState;

        protected CpuThreadState SingleOpcode(uint Opcode,
                                            ulong X0 = 0, ulong X1 = 0, ulong X2 = 0, ulong X3 = 0, ulong X31 = 0,
                                            Vector128<float> V0 = default(Vector128<float>),
                                            Vector128<float> V1 = default(Vector128<float>),
                                            Vector128<float> V2 = default(Vector128<float>),
                                            Vector128<float> V3 = default(Vector128<float>),
                                            bool Overflow = false, bool Carry = false, bool Zero = false, bool Negative = false,
                                            int Fpcr = 0x0, int Fpsr = 0x0)
        {
            this.Opcode(Opcode);
            this.Opcode(0xD4200000); // BRK #0
            this.Opcode(0xD65F03C0); // RET
            SetThreadState(X0, X1, X2, X3, X31, V0, V1, V2, V3, Overflow, Carry, Zero, Negative, Fpcr, Fpsr);
            ExecuteOpcodes();

            return GetThreadState();
        }

        /// <summary>Rounding Mode control field.</summary>
        public enum RMode
        {
            /// <summary>Round to Nearest (RN) mode.</summary>
            RN,
            /// <summary>Round towards Plus Infinity (RP) mode.</summary>
            RP,
            /// <summary>Round towards Minus Infinity (RM) mode.</summary>
            RM,
            /// <summary>Round towards Zero (RZ) mode.</summary>
            RZ
        };

        /// <summary>Floating-point Control Register.</summary>
        protected enum FPCR
        {
            /// <summary>Rounding Mode control field.</summary>
            RMode = 22,
            /// <summary>Flush-to-zero mode control bit.</summary>
            FZ    = 24,
            /// <summary>Default NaN mode control bit.</summary>
            DN    = 25,
            /// <summary>Alternative half-precision control bit.</summary>
            AHP   = 26
        }

        /// <summary>Floating-point Status Register.</summary>
        [Flags] protected enum FPSR
        {
            None = 0,

            /// <summary>Invalid Operation cumulative floating-point exception bit.</summary>
            IOC = 1 << 0,
            /// <summary>Divide by Zero cumulative floating-point exception bit.</summary>
            DZC = 1 << 1,
            /// <summary>Overflow cumulative floating-point exception bit.</summary>
            OFC = 1 << 2,
            /// <summary>Underflow cumulative floating-point exception bit.</summary>
            UFC = 1 << 3,
            /// <summary>Inexact cumulative floating-point exception bit.</summary>
            IXC = 1 << 4,
            /// <summary>Input Denormal cumulative floating-point exception bit.</summary>
            IDC = 1 << 7,

            /// <summary>Cumulative saturation bit.</summary>
            QC = 1 << 27
        }

        [Flags] protected enum FpSkips
        {
            None = 0,

            IfNaN_S = 1,
            IfNaN_D = 2,

            IfUnderflow = 4,
            IfOverflow  = 8
        }

        protected enum FpTolerances
        {
            None,

            UpToOneUlps_S,
            UpToOneUlps_D
        }

        protected void CompareAgainstUnicorn(
            FPSR         FpsrMask     = FPSR.None,
            FpSkips      FpSkips      = FpSkips.None,
            FpTolerances FpTolerances = FpTolerances.None)
        {
            if (!UnicornAvailable)
            {
                return;
            }

            if (FpSkips != FpSkips.None)
            {
                ManageFpSkips(FpSkips);
            }

            Assert.That(Thread.ThreadState.X0,  Is.EqualTo(UnicornEmu.X[0]));
            Assert.That(Thread.ThreadState.X1,  Is.EqualTo(UnicornEmu.X[1]));
            Assert.That(Thread.ThreadState.X2,  Is.EqualTo(UnicornEmu.X[2]));
            Assert.That(Thread.ThreadState.X3,  Is.EqualTo(UnicornEmu.X[3]));
            Assert.That(Thread.ThreadState.X4,  Is.EqualTo(UnicornEmu.X[4]));
            Assert.That(Thread.ThreadState.X5,  Is.EqualTo(UnicornEmu.X[5]));
            Assert.That(Thread.ThreadState.X6,  Is.EqualTo(UnicornEmu.X[6]));
            Assert.That(Thread.ThreadState.X7,  Is.EqualTo(UnicornEmu.X[7]));
            Assert.That(Thread.ThreadState.X8,  Is.EqualTo(UnicornEmu.X[8]));
            Assert.That(Thread.ThreadState.X9,  Is.EqualTo(UnicornEmu.X[9]));
            Assert.That(Thread.ThreadState.X10, Is.EqualTo(UnicornEmu.X[10]));
            Assert.That(Thread.ThreadState.X11, Is.EqualTo(UnicornEmu.X[11]));
            Assert.That(Thread.ThreadState.X12, Is.EqualTo(UnicornEmu.X[12]));
            Assert.That(Thread.ThreadState.X13, Is.EqualTo(UnicornEmu.X[13]));
            Assert.That(Thread.ThreadState.X14, Is.EqualTo(UnicornEmu.X[14]));
            Assert.That(Thread.ThreadState.X15, Is.EqualTo(UnicornEmu.X[15]));
            Assert.That(Thread.ThreadState.X16, Is.EqualTo(UnicornEmu.X[16]));
            Assert.That(Thread.ThreadState.X17, Is.EqualTo(UnicornEmu.X[17]));
            Assert.That(Thread.ThreadState.X18, Is.EqualTo(UnicornEmu.X[18]));
            Assert.That(Thread.ThreadState.X19, Is.EqualTo(UnicornEmu.X[19]));
            Assert.That(Thread.ThreadState.X20, Is.EqualTo(UnicornEmu.X[20]));
            Assert.That(Thread.ThreadState.X21, Is.EqualTo(UnicornEmu.X[21]));
            Assert.That(Thread.ThreadState.X22, Is.EqualTo(UnicornEmu.X[22]));
            Assert.That(Thread.ThreadState.X23, Is.EqualTo(UnicornEmu.X[23]));
            Assert.That(Thread.ThreadState.X24, Is.EqualTo(UnicornEmu.X[24]));
            Assert.That(Thread.ThreadState.X25, Is.EqualTo(UnicornEmu.X[25]));
            Assert.That(Thread.ThreadState.X26, Is.EqualTo(UnicornEmu.X[26]));
            Assert.That(Thread.ThreadState.X27, Is.EqualTo(UnicornEmu.X[27]));
            Assert.That(Thread.ThreadState.X28, Is.EqualTo(UnicornEmu.X[28]));
            Assert.That(Thread.ThreadState.X29, Is.EqualTo(UnicornEmu.X[29]));
            Assert.That(Thread.ThreadState.X30, Is.EqualTo(UnicornEmu.X[30]));

            Assert.That(Thread.ThreadState.X31, Is.EqualTo(UnicornEmu.SP));

            if (FpTolerances == FpTolerances.None)
            {
                Assert.That(Thread.ThreadState.V0, Is.EqualTo(UnicornEmu.Q[0]));
            }
            else
            {
                ManageFpTolerances(FpTolerances);
            }
            Assert.That(Thread.ThreadState.V1,  Is.EqualTo(UnicornEmu.Q[1]));
            Assert.That(Thread.ThreadState.V2,  Is.EqualTo(UnicornEmu.Q[2]));
            Assert.That(Thread.ThreadState.V3,  Is.EqualTo(UnicornEmu.Q[3]));
            Assert.That(Thread.ThreadState.V4,  Is.EqualTo(UnicornEmu.Q[4]));
            Assert.That(Thread.ThreadState.V5,  Is.EqualTo(UnicornEmu.Q[5]));
            Assert.That(Thread.ThreadState.V6,  Is.EqualTo(UnicornEmu.Q[6]));
            Assert.That(Thread.ThreadState.V7,  Is.EqualTo(UnicornEmu.Q[7]));
            Assert.That(Thread.ThreadState.V8,  Is.EqualTo(UnicornEmu.Q[8]));
            Assert.That(Thread.ThreadState.V9,  Is.EqualTo(UnicornEmu.Q[9]));
            Assert.That(Thread.ThreadState.V10, Is.EqualTo(UnicornEmu.Q[10]));
            Assert.That(Thread.ThreadState.V11, Is.EqualTo(UnicornEmu.Q[11]));
            Assert.That(Thread.ThreadState.V12, Is.EqualTo(UnicornEmu.Q[12]));
            Assert.That(Thread.ThreadState.V13, Is.EqualTo(UnicornEmu.Q[13]));
            Assert.That(Thread.ThreadState.V14, Is.EqualTo(UnicornEmu.Q[14]));
            Assert.That(Thread.ThreadState.V15, Is.EqualTo(UnicornEmu.Q[15]));
            Assert.That(Thread.ThreadState.V16, Is.EqualTo(UnicornEmu.Q[16]));
            Assert.That(Thread.ThreadState.V17, Is.EqualTo(UnicornEmu.Q[17]));
            Assert.That(Thread.ThreadState.V18, Is.EqualTo(UnicornEmu.Q[18]));
            Assert.That(Thread.ThreadState.V19, Is.EqualTo(UnicornEmu.Q[19]));
            Assert.That(Thread.ThreadState.V20, Is.EqualTo(UnicornEmu.Q[20]));
            Assert.That(Thread.ThreadState.V21, Is.EqualTo(UnicornEmu.Q[21]));
            Assert.That(Thread.ThreadState.V22, Is.EqualTo(UnicornEmu.Q[22]));
            Assert.That(Thread.ThreadState.V23, Is.EqualTo(UnicornEmu.Q[23]));
            Assert.That(Thread.ThreadState.V24, Is.EqualTo(UnicornEmu.Q[24]));
            Assert.That(Thread.ThreadState.V25, Is.EqualTo(UnicornEmu.Q[25]));
            Assert.That(Thread.ThreadState.V26, Is.EqualTo(UnicornEmu.Q[26]));
            Assert.That(Thread.ThreadState.V27, Is.EqualTo(UnicornEmu.Q[27]));
            Assert.That(Thread.ThreadState.V28, Is.EqualTo(UnicornEmu.Q[28]));
            Assert.That(Thread.ThreadState.V29, Is.EqualTo(UnicornEmu.Q[29]));
            Assert.That(Thread.ThreadState.V30, Is.EqualTo(UnicornEmu.Q[30]));
            Assert.That(Thread.ThreadState.V31, Is.EqualTo(UnicornEmu.Q[31]));
            Assert.That(Thread.ThreadState.V31, Is.EqualTo(UnicornEmu.Q[31]));

            Assert.That(Thread.ThreadState.Fpcr,                 Is.EqualTo(UnicornEmu.Fpcr));
            Assert.That(Thread.ThreadState.Fpsr & (int)FpsrMask, Is.EqualTo(UnicornEmu.Fpsr & (int)FpsrMask));

            Assert.That(Thread.ThreadState.Overflow, Is.EqualTo(UnicornEmu.OverflowFlag));
            Assert.That(Thread.ThreadState.Carry,    Is.EqualTo(UnicornEmu.CarryFlag));
            Assert.That(Thread.ThreadState.Zero,     Is.EqualTo(UnicornEmu.ZeroFlag));
            Assert.That(Thread.ThreadState.Negative, Is.EqualTo(UnicornEmu.NegativeFlag));
        }

        private void ManageFpSkips(FpSkips FpSkips)
        {
            if (FpSkips.HasFlag(FpSkips.IfNaN_S))
            {
                if (float.IsNaN(VectorExtractSingle(UnicornEmu.Q[0], (byte)0)))
                {
                    Assert.Ignore("NaN test.");
                }
            }
            else if (FpSkips.HasFlag(FpSkips.IfNaN_D))
            {
                if (double.IsNaN(VectorExtractDouble(UnicornEmu.Q[0], (byte)0)))
                {
                    Assert.Ignore("NaN test.");
                }
            }

            if (FpSkips.HasFlag(FpSkips.IfUnderflow))
            {
                if ((UnicornEmu.Fpsr & (int)FPSR.UFC) != 0)
                {
                    Assert.Ignore("Underflow test.");
                }
            }

            if (FpSkips.HasFlag(FpSkips.IfOverflow))
            {
                if ((UnicornEmu.Fpsr & (int)FPSR.OFC) != 0)
                {
                    Assert.Ignore("Overflow test.");
                }
            }
        }

        private void ManageFpTolerances(FpTolerances FpTolerances)
        {
            if (!Is.EqualTo(UnicornEmu.Q[0]).ApplyTo(Thread.ThreadState.V0).IsSuccess)
            {
                if (FpTolerances == FpTolerances.UpToOneUlps_S)
                {
                    if (IsNormalOrSubnormal_S(VectorExtractSingle(UnicornEmu.Q[0],       (byte)0)) &&
                        IsNormalOrSubnormal_S(VectorExtractSingle(Thread.ThreadState.V0, (byte)0)))
                    {
                        Assert.That   (VectorExtractSingle(Thread.ThreadState.V0, (byte)0),
                            Is.EqualTo(VectorExtractSingle(UnicornEmu.Q[0],       (byte)0)).Within(1).Ulps);
                        Assert.That   (VectorExtractSingle(Thread.ThreadState.V0, (byte)1),
                            Is.EqualTo(VectorExtractSingle(UnicornEmu.Q[0],       (byte)1)).Within(1).Ulps);
                        Assert.That   (VectorExtractSingle(Thread.ThreadState.V0, (byte)2),
                            Is.EqualTo(VectorExtractSingle(UnicornEmu.Q[0],       (byte)2)).Within(1).Ulps);
                        Assert.That   (VectorExtractSingle(Thread.ThreadState.V0, (byte)3),
                            Is.EqualTo(VectorExtractSingle(UnicornEmu.Q[0],       (byte)3)).Within(1).Ulps);

                        Console.WriteLine(FpTolerances);
                    }
                    else
                    {
                        Assert.That(Thread.ThreadState.V0, Is.EqualTo(UnicornEmu.Q[0]));
                    }
                }

                if (FpTolerances == FpTolerances.UpToOneUlps_D)
                {
                    if (IsNormalOrSubnormal_D(VectorExtractDouble(UnicornEmu.Q[0],       (byte)0)) &&
                        IsNormalOrSubnormal_D(VectorExtractDouble(Thread.ThreadState.V0, (byte)0)))
                    {
                        Assert.That   (VectorExtractDouble(Thread.ThreadState.V0, (byte)0),
                            Is.EqualTo(VectorExtractDouble(UnicornEmu.Q[0],       (byte)0)).Within(1).Ulps);
                        Assert.That   (VectorExtractDouble(Thread.ThreadState.V0, (byte)1),
                            Is.EqualTo(VectorExtractDouble(UnicornEmu.Q[0],       (byte)1)).Within(1).Ulps);

                        Console.WriteLine(FpTolerances);
                    }
                    else
                    {
                        Assert.That(Thread.ThreadState.V0, Is.EqualTo(UnicornEmu.Q[0]));
                    }
                }
            }

            bool IsNormalOrSubnormal_S(float f)  => float.IsNormal(f)  || float.IsSubnormal(f);

            bool IsNormalOrSubnormal_D(double d) => double.IsNormal(d) || double.IsSubnormal(d);
        }

        protected static Vector128<float> MakeVectorE0(double E0)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<long, float>(Sse2.SetVector128(0, BitConverter.DoubleToInt64Bits(E0)));
        }

        protected static Vector128<float> MakeVectorE0E1(double E0, double E1)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<long, float>(
                Sse2.SetVector128(BitConverter.DoubleToInt64Bits(E1), BitConverter.DoubleToInt64Bits(E0)));
        }

        protected static Vector128<float> MakeVectorE1(double E1)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<long, float>(Sse2.SetVector128(BitConverter.DoubleToInt64Bits(E1), 0));
        }

        protected static float VectorExtractSingle(Vector128<float> Vector, byte Index)
        {
            if (!Sse41.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            int Value = Sse41.Extract(Sse.StaticCast<float, int>(Vector), Index);

            return BitConverter.Int32BitsToSingle(Value);
        }

        protected static double VectorExtractDouble(Vector128<float> Vector, byte Index)
        {
            if (!Sse41.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            long Value = Sse41.Extract(Sse.StaticCast<float, long>(Vector), Index);

            return BitConverter.Int64BitsToDouble(Value);
        }

        protected static Vector128<float> MakeVectorE0(ulong E0)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<ulong, float>(Sse2.SetVector128(0, E0));
        }

        protected static Vector128<float> MakeVectorE0E1(ulong E0, ulong E1)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<ulong, float>(Sse2.SetVector128(E1, E0));
        }

        protected static Vector128<float> MakeVectorE1(ulong E1)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse.StaticCast<ulong, float>(Sse2.SetVector128(E1, 0));
        }

        protected static ulong GetVectorE0(Vector128<float> Vector)
        {
            if (!Sse41.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse41.Extract(Sse.StaticCast<float, ulong>(Vector), (byte)0);
        }

        protected static ulong GetVectorE1(Vector128<float> Vector)
        {
            if (!Sse41.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return Sse41.Extract(Sse.StaticCast<float, ulong>(Vector), (byte)1);
        }

        protected static ushort GenNormal_H()
        {
            uint Rnd;

            do       Rnd = TestContext.CurrentContext.Random.NextUShort();
            while (( Rnd & 0x7C00u) == 0u ||
                   (~Rnd & 0x7C00u) == 0u);

            return (ushort)Rnd;
        }

        protected static ushort GenSubnormal_H()
        {
            uint Rnd;

            do      Rnd = TestContext.CurrentContext.Random.NextUShort();
            while ((Rnd & 0x03FFu) == 0u);

            return (ushort)(Rnd & 0x83FFu);
        }

        protected static uint GenNormal_S()
        {
            uint Rnd;

            do       Rnd = TestContext.CurrentContext.Random.NextUInt();
            while (( Rnd & 0x7F800000u) == 0u ||
                   (~Rnd & 0x7F800000u) == 0u);

            return Rnd;
        }

        protected static uint GenSubnormal_S()
        {
            uint Rnd;

            do      Rnd = TestContext.CurrentContext.Random.NextUInt();
            while ((Rnd & 0x007FFFFFu) == 0u);

            return Rnd & 0x807FFFFFu;
        }

        protected static ulong GenNormal_D()
        {
            ulong Rnd;

            do       Rnd = TestContext.CurrentContext.Random.NextULong();
            while (( Rnd & 0x7FF0000000000000ul) == 0ul ||
                   (~Rnd & 0x7FF0000000000000ul) == 0ul);

            return Rnd;
        }

        protected static ulong GenSubnormal_D()
        {
            ulong Rnd;

            do      Rnd = TestContext.CurrentContext.Random.NextULong();
            while ((Rnd & 0x000FFFFFFFFFFFFFul) == 0ul);

            return Rnd & 0x800FFFFFFFFFFFFFul;
        }
    }
}
