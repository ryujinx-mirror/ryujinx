using ChocolArm64;
using ChocolArm64.Memory;
using ChocolArm64.State;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.Tests.Cpu
{
    [TestFixture]
    public class CpuTest
    {
        protected long Position { get; private set; }
        private long Size;

        private long EntryPoint;

        private IntPtr Ram;
        private AMemory Memory;
        private AThread Thread;

        [SetUp]
        public void Setup()
        {
            Position = 0x0;
            Size = 0x1000;

            EntryPoint = Position;

            Ram = Marshal.AllocHGlobal((IntPtr)AMemoryMgr.RamSize);
            ATranslator Translator = new ATranslator();
            Memory = new AMemory(Ram);
            Memory.Manager.Map(Position, Size, 2, AMemoryPerm.Read | AMemoryPerm.Write | AMemoryPerm.Execute);
            Thread = new AThread(Translator, Memory, ThreadPriority.Normal, EntryPoint);
        }

        [TearDown]
        public void Teardown()
        {
            Thread = null;
            Memory = null;
            Marshal.FreeHGlobal(Ram);
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

        protected void SetThreadState(ulong X0 = 0, ulong X1 = 0, ulong X2 = 0, ulong X31 = 0,
                                      AVec V0 = default(AVec), AVec V1 = default(AVec), AVec V2 = default(AVec),
                                      bool Overflow = false, bool Carry = false, bool Zero = false, bool Negative = false)
        {
            Thread.ThreadState.X0 = X0;
            Thread.ThreadState.X1 = X1;
            Thread.ThreadState.X2 = X2;
            Thread.ThreadState.X31 = X31;
            Thread.ThreadState.V0 = V0;
            Thread.ThreadState.V1 = V1;
            Thread.ThreadState.V2 = V2;
            Thread.ThreadState.Overflow = Overflow;
            Thread.ThreadState.Carry = Carry;
            Thread.ThreadState.Zero = Zero;
            Thread.ThreadState.Negative = Negative;
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
                                            ulong X0 = 0, ulong X1 = 0, ulong X2 = 0, ulong X31 = 0,
                                            AVec V0 = default(AVec), AVec V1 = default(AVec), AVec V2 = default(AVec),
                                            bool Overflow = false, bool Carry = false, bool Zero = false, bool Negative = false)
        {
            this.Opcode(Opcode);
            this.Opcode(0xD4200000); // BRK #0
            this.Opcode(0xD65F03C0); // RET
            SetThreadState(X0, X1, X2, X31, V0, V1, V2, Overflow, Carry, Zero, Negative);
            ExecuteOpcodes();

            return GetThreadState();
        }
    }
}
