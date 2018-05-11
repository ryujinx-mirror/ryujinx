//#define AluRx

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("AluRx"), Ignore("Tested: first half of 2018.")]
    public sealed class CpuTestAluRx : CpuTest
    {
#if AluRx
        [SetUp]
        public void SetupTester()
        {
            AArch64.TakeReset(false);
        }

        [Test, Description("ADD <Xd|SP>, <Xn|SP>, <X><m>{, <extend> {#<amount>}}")]
        public void Add_X_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                [Values((ulong)0x0000000000000000, (ulong)0x7FFFFFFFFFFFFFFF,
                                        (ulong)0x8000000000000000, (ulong)0xFFFFFFFFFFFFFFFF)] [Random(2)] ulong Xm,
                                [Values(0b011u, 0b111u)] uint extend, // <LSL|UXTX, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x8B206000; // ADD X0, X0, X0, UXTX #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ulong _X31 = TestContext.CurrentContext.Random.NextULong();
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Xm, X31: _X31);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP, X2: Xm);

                AArch64.SP(new Bits(Xn_SP));
            }

            AArch64.X((int)Rm, new Bits(Xm));
            Base.Add_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
        }

        [Test, Description("ADD <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Add_W_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                        (uint)0x80000000, (uint)0xFFFFFFFF)] [Random(2)] uint Wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x8B200000; // ADD X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ulong _X31 = TestContext.CurrentContext.Random.NextULong();
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: _X31);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP, X2: Wm);

                AArch64.SP(new Bits(Xn_SP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Add_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
        }

        [Test, Description("ADD <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Add_H_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                [Values((ushort)0x0000, (ushort)0x7FFF,
                                        (ushort)0x8000, (ushort)0xFFFF)] [Random(2)] ushort Wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x8B200000; // ADD X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ulong _X31 = TestContext.CurrentContext.Random.NextULong();
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: _X31);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP, X2: Wm);

                AArch64.SP(new Bits(Xn_SP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Add_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
        }

        [Test, Description("ADD <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Add_B_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                [Values((byte)0x00, (byte)0x7F,
                                        (byte)0x80, (byte)0xFF)] [Random(2)] byte Wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x8B200000; // ADD X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ulong _X31 = TestContext.CurrentContext.Random.NextULong();
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: _X31);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP, X2: Wm);

                AArch64.SP(new Bits(Xn_SP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Add_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
        }

        [Test, Description("ADD <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Add_W_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                        (uint)0x80000000, (uint)0xFFFFFFFF)] [Random(2)] uint Wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x0B200000; // ADD W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                uint _W31 = TestContext.CurrentContext.Random.NextUInt();
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: _W31);

                AArch64.X((int)Rn, new Bits(Wn_WSP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP, X2: Wm);

                AArch64.SP(new Bits(Wn_WSP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Add_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
        }

        [Test, Description("ADD <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Add_H_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                [Values((ushort)0x0000, (ushort)0x7FFF,
                                        (ushort)0x8000, (ushort)0xFFFF)] [Random(2)] ushort Wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x0B200000; // ADD W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                uint _W31 = TestContext.CurrentContext.Random.NextUInt();
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: _W31);

                AArch64.X((int)Rn, new Bits(Wn_WSP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP, X2: Wm);

                AArch64.SP(new Bits(Wn_WSP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Add_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
        }

        [Test, Description("ADD <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Add_B_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                [Values((byte)0x00, (byte)0x7F,
                                        (byte)0x80, (byte)0xFF)] [Random(2)] byte Wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x0B200000; // ADD W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                uint _W31 = TestContext.CurrentContext.Random.NextUInt();
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: _W31);

                AArch64.X((int)Rn, new Bits(Wn_WSP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP, X2: Wm);

                AArch64.SP(new Bits(Wn_WSP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Add_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
        }

        [Test, Description("ADDS <Xd>, <Xn|SP>, <X><m>{, <extend> {#<amount>}}")]
        public void Adds_X_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                 [Values((ulong)0x0000000000000000, (ulong)0x7FFFFFFFFFFFFFFF,
                                         (ulong)0x8000000000000000, (ulong)0xFFFFFFFFFFFFFFFF)] [Random(2)] ulong Xm,
                                 [Values(0b011u, 0b111u)] uint extend, // <LSL|UXTX, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xAB206000; // ADDS X0, X0, X0, UXTX #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Xm, X31: Xn_SP);

            AArch64.X((int)Rn, new Bits(Xn_SP));
            AArch64.X((int)Rm, new Bits(Xm));
            AArch64.SP(new Bits(Xn_SP));
            Base.Adds_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong _X31 = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ADDS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Adds_W_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                 [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                         (uint)0x80000000, (uint)0xFFFFFFFF)] [Random(2)] uint Wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xAB200000; // ADDS X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: Xn_SP);

            AArch64.X((int)Rn, new Bits(Xn_SP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Xn_SP));
            Base.Adds_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong _X31 = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ADDS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Adds_H_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                 [Values((ushort)0x0000, (ushort)0x7FFF,
                                         (ushort)0x8000, (ushort)0xFFFF)] [Random(2)] ushort Wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xAB200000; // ADDS X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: Xn_SP);

            AArch64.X((int)Rn, new Bits(Xn_SP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Xn_SP));
            Base.Adds_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong _X31 = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ADDS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Adds_B_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                 [Values((byte)0x00, (byte)0x7F,
                                         (byte)0x80, (byte)0xFF)] [Random(2)] byte Wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xAB200000; // ADDS X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: Xn_SP);

            AArch64.X((int)Rn, new Bits(Xn_SP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Xn_SP));
            Base.Adds_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong _X31 = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ADDS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Adds_W_32bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                 [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                         (uint)0x80000000, (uint)0xFFFFFFFF)] [Random(2)] uint Wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x2B200000; // ADDS W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: Wn_WSP);

            AArch64.X((int)Rn, new Bits(Wn_WSP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Wn_WSP));
            Base.Adds_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint _W31 = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ADDS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Adds_H_32bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                 [Values((ushort)0x0000, (ushort)0x7FFF,
                                         (ushort)0x8000, (ushort)0xFFFF)] [Random(2)] ushort Wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x2B200000; // ADDS W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: Wn_WSP);

            AArch64.X((int)Rn, new Bits(Wn_WSP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Wn_WSP));
            Base.Adds_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint _W31 = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ADDS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Adds_B_32bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                 [Values((byte)0x00, (byte)0x7F,
                                         (byte)0x80, (byte)0xFF)] [Random(2)] byte Wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x2B200000; // ADDS W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: Wn_WSP);

            AArch64.X((int)Rn, new Bits(Wn_WSP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Wn_WSP));
            Base.Adds_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint _W31 = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("SUB <Xd|SP>, <Xn|SP>, <X><m>{, <extend> {#<amount>}}")]
        public void Sub_X_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                [Values((ulong)0x0000000000000000, (ulong)0x7FFFFFFFFFFFFFFF,
                                        (ulong)0x8000000000000000, (ulong)0xFFFFFFFFFFFFFFFF)] [Random(2)] ulong Xm,
                                [Values(0b011u, 0b111u)] uint extend, // <LSL|UXTX, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xCB206000; // SUB X0, X0, X0, UXTX #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ulong _X31 = TestContext.CurrentContext.Random.NextULong();
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Xm, X31: _X31);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP, X2: Xm);

                AArch64.SP(new Bits(Xn_SP));
            }

            AArch64.X((int)Rm, new Bits(Xm));
            Base.Sub_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
        }

        [Test, Description("SUB <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Sub_W_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                        (uint)0x80000000, (uint)0xFFFFFFFF)] [Random(2)] uint Wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xCB200000; // SUB X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ulong _X31 = TestContext.CurrentContext.Random.NextULong();
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: _X31);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP, X2: Wm);

                AArch64.SP(new Bits(Xn_SP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Sub_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
        }

        [Test, Description("SUB <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Sub_H_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                [Values((ushort)0x0000, (ushort)0x7FFF,
                                        (ushort)0x8000, (ushort)0xFFFF)] [Random(2)] ushort Wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xCB200000; // SUB X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ulong _X31 = TestContext.CurrentContext.Random.NextULong();
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: _X31);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP, X2: Wm);

                AArch64.SP(new Bits(Xn_SP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Sub_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
        }

        [Test, Description("SUB <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Sub_B_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                [Values((byte)0x00, (byte)0x7F,
                                        (byte)0x80, (byte)0xFF)] [Random(2)] byte Wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xCB200000; // SUB X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ulong _X31 = TestContext.CurrentContext.Random.NextULong();
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: _X31);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP, X2: Wm);

                AArch64.SP(new Bits(Xn_SP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Sub_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
        }

        [Test, Description("SUB <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Sub_W_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                        (uint)0x80000000, (uint)0xFFFFFFFF)] [Random(2)] uint Wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x4B200000; // SUB W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                uint _W31 = TestContext.CurrentContext.Random.NextUInt();
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: _W31);

                AArch64.X((int)Rn, new Bits(Wn_WSP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP, X2: Wm);

                AArch64.SP(new Bits(Wn_WSP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Sub_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
        }

        [Test, Description("SUB <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Sub_H_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                [Values((ushort)0x0000, (ushort)0x7FFF,
                                        (ushort)0x8000, (ushort)0xFFFF)] [Random(2)] ushort Wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x4B200000; // SUB W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                uint _W31 = TestContext.CurrentContext.Random.NextUInt();
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: _W31);

                AArch64.X((int)Rn, new Bits(Wn_WSP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP, X2: Wm);

                AArch64.SP(new Bits(Wn_WSP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Sub_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
        }

        [Test, Description("SUB <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Sub_B_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                [Values((byte)0x00, (byte)0x7F,
                                        (byte)0x80, (byte)0xFF)] [Random(2)] byte Wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x4B200000; // SUB W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                uint _W31 = TestContext.CurrentContext.Random.NextUInt();
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: _W31);

                AArch64.X((int)Rn, new Bits(Wn_WSP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP, X2: Wm);

                AArch64.SP(new Bits(Wn_WSP));
            }

            AArch64.X((int)Rm, new Bits(Wm));
            Base.Sub_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
        }

        [Test, Description("SUBS <Xd>, <Xn|SP>, <X><m>{, <extend> {#<amount>}}")]
        public void Subs_X_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                 [Values((ulong)0x0000000000000000, (ulong)0x7FFFFFFFFFFFFFFF,
                                         (ulong)0x8000000000000000, (ulong)0xFFFFFFFFFFFFFFFF)] [Random(2)] ulong Xm,
                                 [Values(0b011u, 0b111u)] uint extend, // <LSL|UXTX, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xEB206000; // SUBS X0, X0, X0, UXTX #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Xm, X31: Xn_SP);

            AArch64.X((int)Rn, new Bits(Xn_SP));
            AArch64.X((int)Rm, new Bits(Xm));
            AArch64.SP(new Bits(Xn_SP));
            Base.Subs_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong _X31 = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("SUBS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Subs_W_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                 [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                         (uint)0x80000000, (uint)0xFFFFFFFF)] [Random(2)] uint Wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xEB200000; // SUBS X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: Xn_SP);

            AArch64.X((int)Rn, new Bits(Xn_SP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Xn_SP));
            Base.Subs_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong _X31 = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("SUBS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Subs_H_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                 [Values((ushort)0x0000, (ushort)0x7FFF,
                                         (ushort)0x8000, (ushort)0xFFFF)] [Random(2)] ushort Wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xEB200000; // SUBS X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: Xn_SP);

            AArch64.X((int)Rn, new Bits(Xn_SP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Xn_SP));
            Base.Subs_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong _X31 = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("SUBS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Subs_B_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn_SP,
                                 [Values((byte)0x00, (byte)0x7F,
                                         (byte)0x80, (byte)0xFF)] [Random(2)] byte Wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0xEB200000; // SUBS X0, X0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn_SP, X2: Wm, X31: Xn_SP);

            AArch64.X((int)Rn, new Bits(Xn_SP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Xn_SP));
            Base.Subs_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong _X31 = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("SUBS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Subs_W_32bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                 [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                         (uint)0x80000000, (uint)0xFFFFFFFF)] [Random(2)] uint Wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x6B200000; // SUBS W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: Wn_WSP);

            AArch64.X((int)Rn, new Bits(Wn_WSP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Wn_WSP));
            Base.Subs_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint _W31 = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("SUBS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Subs_H_32bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                 [Values((ushort)0x0000, (ushort)0x7FFF,
                                         (ushort)0x8000, (ushort)0xFFFF)] [Random(2)] ushort Wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x6B200000; // SUBS W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: Wn_WSP);

            AArch64.X((int)Rn, new Bits(Wn_WSP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Wn_WSP));
            Base.Subs_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint _W31 = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("SUBS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Subs_B_32bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn_WSP,
                                 [Values((byte)0x00, (byte)0x7F,
                                         (byte)0x80, (byte)0xFF)] [Random(2)] byte Wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint Opcode = 0x6B200000; // SUBS W0, W0, W0, UXTB #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn_WSP, X2: Wm, X31: Wn_WSP);

            AArch64.X((int)Rn, new Bits(Wn_WSP));
            AArch64.X((int)Rm, new Bits(Wm));
            AArch64.SP(new Bits(Wn_WSP));
            Base.Subs_Rx(Op[31], Op[20, 16], Op[15, 13], Op[12, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint _W31 = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }
#endif
    }
}
