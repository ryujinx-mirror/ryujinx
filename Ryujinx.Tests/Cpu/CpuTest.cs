using ChocolArm64;
using ChocolArm64.Memory;
using ChocolArm64.State;

using NUnit.Framework;

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

        private AMemory Memory;
        private AThread Thread;

        [SetUp]
        public void Setup()
        {
            Position = 0x0;
            Size = 0x1000;

            EntryPoint = Position;

            ATranslator Translator = new ATranslator();
            RamPointer = Marshal.AllocHGlobal(new IntPtr(Size));
            Memory = new AMemory(RamPointer);
            Memory.Map(Position, 0, Size);
            Thread = new AThread(Translator, Memory, EntryPoint);
        }

        [TearDown]
        public void Teardown()
        {
            Marshal.FreeHGlobal(RamPointer);
            Memory = null;
            Thread = null;
        }

        protected void Reset()
        {
            Teardown();
            Setup();
        }

        protected void Opcode(uint Opcode)
        {
            Thread.Memory.WriteUInt32(Position, Opcode);
            Position += 4;
        }

        protected void SetThreadState(ulong X0 = 0, ulong X1 = 0, ulong X2 = 0, ulong X3 = 0, ulong X31 = 0,
                                      Vector128<float> V0 = default(Vector128<float>),
                                      Vector128<float> V1 = default(Vector128<float>),
                                      Vector128<float> V2 = default(Vector128<float>),
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
            Thread.ThreadState.Overflow = Overflow;
            Thread.ThreadState.Carry = Carry;
            Thread.ThreadState.Zero = Zero;
            Thread.ThreadState.Negative = Negative;
            Thread.ThreadState.Fpcr = Fpcr;
            Thread.ThreadState.Fpsr = Fpsr;
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
        }

        protected AThreadState GetThreadState()
        {
            return Thread.ThreadState;
        }

        protected AThreadState SingleOpcode(uint Opcode,
                                            ulong X0 = 0, ulong X1 = 0, ulong X2 = 0, ulong X3 = 0, ulong X31 = 0,
                                            Vector128<float> V0 = default(Vector128<float>),
                                            Vector128<float> V1 = default(Vector128<float>),
                                            Vector128<float> V2 = default(Vector128<float>),
                                            bool Overflow = false, bool Carry = false, bool Zero = false, bool Negative = false,
                                            int Fpcr = 0x0, int Fpsr = 0x0)
        {
            this.Opcode(Opcode);
            this.Opcode(0xD4200000); // BRK #0
            this.Opcode(0xD65F03C0); // RET
            SetThreadState(X0, X1, X2, X3, X31, V0, V1, V2, Overflow, Carry, Zero, Negative, Fpcr, Fpsr);
            ExecuteOpcodes();

            return GetThreadState();
        }

        protected static Vector128<float> MakeVectorE0(double E0)
        {
            return Sse.StaticCast<long, float>(Sse2.SetVector128(0, BitConverter.DoubleToInt64Bits(E0)));
        }

        protected static Vector128<float> MakeVectorE0E1(double E0, double E1)
        {
            return Sse.StaticCast<long, float>(Sse2.SetVector128(BitConverter.DoubleToInt64Bits(E1),
                                                                 BitConverter.DoubleToInt64Bits(E0)));
        }

        protected static Vector128<float> MakeVectorE1(double E1)
        {
            return Sse.StaticCast<long, float>(Sse2.SetVector128(BitConverter.DoubleToInt64Bits(E1), 0));
        }

        protected static double VectorExtractDouble(Vector128<float> Vector, byte Index)
        {
            long Value = Sse41.Extract(Sse.StaticCast<float, long>(Vector), Index);

            return BitConverter.Int64BitsToDouble(Value);
        }

        protected static Vector128<float> MakeVectorE0(ulong E0)
        {
            return Sse.StaticCast<ulong, float>(Sse2.SetVector128(0, E0));
        }

        protected static Vector128<float> MakeVectorE0E1(ulong E0, ulong E1)
        {
            return Sse.StaticCast<ulong, float>(Sse2.SetVector128(E1, E0));
        }

        protected static Vector128<float> MakeVectorE1(ulong E1)
        {
            return Sse.StaticCast<ulong, float>(Sse2.SetVector128(E1, 0));
        }

        protected static ulong GetVectorE0(Vector128<float> Vector)
        {
            return Sse41.Extract(Sse.StaticCast<float, ulong>(Vector), (byte)0);
        }

        protected static ulong GetVectorE1(Vector128<float> Vector)
        {
            return Sse41.Extract(Sse.StaticCast<float, ulong>(Vector), (byte)1);
        }
    }
}
