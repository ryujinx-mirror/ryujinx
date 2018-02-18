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
    public partial class CpuTest
    {
        IntPtr Ram;
        AMemoryAlloc Allocator;
        AMemory Memory;

        [SetUp]
        public void Setup()
        {
            Ram = Marshal.AllocHGlobal((IntPtr)AMemoryMgr.RamSize);
            Allocator = new AMemoryAlloc();
            Memory = new AMemory(Ram, Allocator);
            Memory.Manager.MapPhys(0x1000, 0x1000, 2, AMemoryPerm.Read | AMemoryPerm.Write | AMemoryPerm.Execute);
        }

        [TearDown]
        public void Teardown()
        {
            Marshal.FreeHGlobal(Ram);
        }

        private void Execute(AThread Thread)
        {
            AutoResetEvent Wait = new AutoResetEvent(false);
            Thread.ThreadState.Break += (sender, e) => Thread.StopExecution();
            Thread.WorkFinished += (sender, e) => Wait.Set();

            Wait.Reset();
            Thread.Execute();
            Wait.WaitOne();
        }

        private AThreadState SingleOpcode(uint Opcode, 
                                          ulong X0 = 0, ulong X1 = 0, ulong X2 = 0, 
                                          AVec V0 = new AVec(), AVec V1 = new AVec(), AVec V2 = new AVec())
        {
            Memory.WriteUInt32(0x1000, Opcode);
            Memory.WriteUInt32(0x1004, 0xD4200000); // BRK #0
            Memory.WriteUInt32(0x1008, 0xD65F03C0); // RET

            AThread Thread = new AThread(Memory, ThreadPriority.Normal, 0x1000);
            Thread.ThreadState.X0 = X0;
            Thread.ThreadState.X1 = X1;
            Thread.ThreadState.X2 = X2;
            Thread.ThreadState.V0 = V0;
            Thread.ThreadState.V1 = V1;
            Thread.ThreadState.V2 = V2;
            Execute(Thread);
            return Thread.ThreadState;
        }

        [Test]
        public void SanityCheck()
        {
            uint Opcode = 0xD503201F; // NOP
            Assert.AreEqual(SingleOpcode(Opcode, X0: 0).X0, 0);
            Assert.AreEqual(SingleOpcode(Opcode, X0: 1).X0, 1);
            Assert.AreEqual(SingleOpcode(Opcode, X0: 2).X0, 2);
            Assert.AreEqual(SingleOpcode(Opcode, X0: 42).X0, 42);
        }
    }
}
