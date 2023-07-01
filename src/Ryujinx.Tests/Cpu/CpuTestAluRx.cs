#define AluRx

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("AluRx")]
    public sealed class CpuTestAluRx : CpuTest
    {
#if AluRx

        [Test, Pairwise, Description("ADD <Xd|SP>, <Xn|SP>, <X><m>{, <extend> {#<amount>}}")]
        public void Add_X_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                [Values((ulong)0x0000000000000000, (ulong)0x7FFFFFFFFFFFFFFF,
                                        0x8000000000000000, 0xFFFFFFFFFFFFFFFF)] ulong xm,
                                [Values(0b011u, 0b111u)] uint extend, // <LSL|UXTX, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x8B206000; // ADD X0, X0, X0, UXTX #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                ulong x31 = TestContext.CurrentContext.Random.NextULong();

                SingleOpcode(opcode, x1: xnSp, x2: xm, x31: x31);
            }
            else
            {
                SingleOpcode(opcode, x31: xnSp, x2: xm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Add_W_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                        0x80000000, 0xFFFFFFFF)] uint wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x8B200000; // ADD X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                ulong x31 = TestContext.CurrentContext.Random.NextULong();

                SingleOpcode(opcode, x1: xnSp, x2: wm, x31: x31);
            }
            else
            {
                SingleOpcode(opcode, x31: xnSp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Add_H_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                [Values((ushort)0x0000, (ushort)0x7FFF,
                                        (ushort)0x8000, (ushort)0xFFFF)] ushort wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x8B200000; // ADD X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                ulong x31 = TestContext.CurrentContext.Random.NextULong();

                SingleOpcode(opcode, x1: xnSp, x2: wm, x31: x31);
            }
            else
            {
                SingleOpcode(opcode, x31: xnSp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Add_B_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                [Values((byte)0x00, (byte)0x7F,
                                        (byte)0x80, (byte)0xFF)] byte wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x8B200000; // ADD X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                ulong x31 = TestContext.CurrentContext.Random.NextULong();

                SingleOpcode(opcode, x1: xnSp, x2: wm, x31: x31);
            }
            else
            {
                SingleOpcode(opcode, x31: xnSp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Add_W_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                        0x80000000, 0xFFFFFFFF)] uint wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x0B200000; // ADD W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                uint w31 = TestContext.CurrentContext.Random.NextUInt();

                SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: w31);
            }
            else
            {
                SingleOpcode(opcode, x31: wnWsp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Add_H_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                [Values((ushort)0x0000, (ushort)0x7FFF,
                                        (ushort)0x8000, (ushort)0xFFFF)] ushort wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x0B200000; // ADD W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                uint w31 = TestContext.CurrentContext.Random.NextUInt();

                SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: w31);
            }
            else
            {
                SingleOpcode(opcode, x31: wnWsp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Add_B_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                [Values((byte)0x00, (byte)0x7F,
                                        (byte)0x80, (byte)0xFF)] byte wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x0B200000; // ADD W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                uint w31 = TestContext.CurrentContext.Random.NextUInt();

                SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: w31);
            }
            else
            {
                SingleOpcode(opcode, x31: wnWsp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Xd>, <Xn|SP>, <X><m>{, <extend> {#<amount>}}")]
        public void Adds_X_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                 [Values((ulong)0x0000000000000000, (ulong)0x7FFFFFFFFFFFFFFF,
                                         0x8000000000000000, 0xFFFFFFFFFFFFFFFF)] ulong xm,
                                 [Values(0b011u, 0b111u)] uint extend, // <LSL|UXTX, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xAB206000; // ADDS X0, X0, X0, UXTX #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: xnSp, x2: xm, x31: xnSp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Adds_W_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                 [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                         0x80000000, 0xFFFFFFFF)] uint wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xAB200000; // ADDS X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: xnSp, x2: wm, x31: xnSp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Adds_H_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                 [Values((ushort)0x0000, (ushort)0x7FFF,
                                         (ushort)0x8000, (ushort)0xFFFF)] ushort wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xAB200000; // ADDS X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: xnSp, x2: wm, x31: xnSp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Adds_B_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                 [Values((byte)0x00, (byte)0x7F,
                                         (byte)0x80, (byte)0xFF)] byte wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xAB200000; // ADDS X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: xnSp, x2: wm, x31: xnSp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Adds_W_32bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                 [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                         0x80000000, 0xFFFFFFFF)] uint wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x2B200000; // ADDS W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: wnWsp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Adds_H_32bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                 [Values((ushort)0x0000, (ushort)0x7FFF,
                                         (ushort)0x8000, (ushort)0xFFFF)] ushort wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x2B200000; // ADDS W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: wnWsp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Adds_B_32bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                 [Values((byte)0x00, (byte)0x7F,
                                         (byte)0x80, (byte)0xFF)] byte wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x2B200000; // ADDS W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: wnWsp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Xd|SP>, <Xn|SP>, <X><m>{, <extend> {#<amount>}}")]
        public void Sub_X_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                [Values((ulong)0x0000000000000000, (ulong)0x7FFFFFFFFFFFFFFF,
                                        0x8000000000000000, 0xFFFFFFFFFFFFFFFF)] ulong xm,
                                [Values(0b011u, 0b111u)] uint extend, // <LSL|UXTX, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xCB206000; // SUB X0, X0, X0, UXTX #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                ulong x31 = TestContext.CurrentContext.Random.NextULong();

                SingleOpcode(opcode, x1: xnSp, x2: xm, x31: x31);
            }
            else
            {
                SingleOpcode(opcode, x31: xnSp, x2: xm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Sub_W_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                        0x80000000, 0xFFFFFFFF)] uint wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xCB200000; // SUB X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                ulong x31 = TestContext.CurrentContext.Random.NextULong();

                SingleOpcode(opcode, x1: xnSp, x2: wm, x31: x31);
            }
            else
            {
                SingleOpcode(opcode, x31: xnSp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Sub_H_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                [Values((ushort)0x0000, (ushort)0x7FFF,
                                        (ushort)0x8000, (ushort)0xFFFF)] ushort wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xCB200000; // SUB X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                ulong x31 = TestContext.CurrentContext.Random.NextULong();

                SingleOpcode(opcode, x1: xnSp, x2: wm, x31: x31);
            }
            else
            {
                SingleOpcode(opcode, x31: xnSp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Xd|SP>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Sub_B_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                [Values((byte)0x00, (byte)0x7F,
                                        (byte)0x80, (byte)0xFF)] byte wm,
                                [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                        0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xCB200000; // SUB X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                ulong x31 = TestContext.CurrentContext.Random.NextULong();

                SingleOpcode(opcode, x1: xnSp, x2: wm, x31: x31);
            }
            else
            {
                SingleOpcode(opcode, x31: xnSp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Sub_W_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                        0x80000000, 0xFFFFFFFF)] uint wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x4B200000; // SUB W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                uint w31 = TestContext.CurrentContext.Random.NextUInt();

                SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: w31);
            }
            else
            {
                SingleOpcode(opcode, x31: wnWsp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Sub_H_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                [Values((ushort)0x0000, (ushort)0x7FFF,
                                        (ushort)0x8000, (ushort)0xFFFF)] ushort wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x4B200000; // SUB W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                uint w31 = TestContext.CurrentContext.Random.NextUInt();

                SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: w31);
            }
            else
            {
                SingleOpcode(opcode, x31: wnWsp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Wd|WSP>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Sub_B_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                [Values((byte)0x00, (byte)0x7F,
                                        (byte)0x80, (byte)0xFF)] byte wm,
                                [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                        0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x4B200000; // SUB W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            if (rn != 31)
            {
                uint w31 = TestContext.CurrentContext.Random.NextUInt();

                SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: w31);
            }
            else
            {
                SingleOpcode(opcode, x31: wnWsp, x2: wm);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Xd>, <Xn|SP>, <X><m>{, <extend> {#<amount>}}")]
        public void Subs_X_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                 [Values((ulong)0x0000000000000000, (ulong)0x7FFFFFFFFFFFFFFF,
                                         0x8000000000000000, 0xFFFFFFFFFFFFFFFF)] ulong xm,
                                 [Values(0b011u, 0b111u)] uint extend, // <LSL|UXTX, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xEB206000; // SUBS X0, X0, X0, UXTX #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: xnSp, x2: xm, x31: xnSp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Subs_W_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                 [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                         0x80000000, 0xFFFFFFFF)] uint wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xEB200000; // SUBS X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: xnSp, x2: wm, x31: xnSp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Subs_H_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                 [Values((ushort)0x0000, (ushort)0x7FFF,
                                         (ushort)0x8000, (ushort)0xFFFF)] ushort wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xEB200000; // SUBS X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: xnSp, x2: wm, x31: xnSp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Xd>, <Xn|SP>, <W><m>{, <extend> {#<amount>}}")]
        public void Subs_B_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xnSp,
                                 [Values((byte)0x00, (byte)0x7F,
                                         (byte)0x80, (byte)0xFF)] byte wm,
                                 [Values(0b000u, 0b001u, 0b010u,               // <UXTB, UXTH, UXTW,
                                         0b100u, 0b101u, 0b110u)] uint extend, //  SXTB, SXTH, SXTW>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0xEB200000; // SUBS X0, X0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: xnSp, x2: wm, x31: xnSp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Subs_W_32bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                 [Values((uint)0x00000000, (uint)0x7FFFFFFF,
                                         0x80000000, 0xFFFFFFFF)] uint wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x6B200000; // SUBS W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: wnWsp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Subs_H_32bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                 [Values((ushort)0x0000, (ushort)0x7FFF,
                                         (ushort)0x8000, (ushort)0xFFFF)] ushort wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x6B200000; // SUBS W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: wnWsp);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Wd>, <Wn|WSP>, <Wm>{, <extend> {#<amount>}}")]
        public void Subs_B_32bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wnWsp,
                                 [Values((byte)0x00, (byte)0x7F,
                                         (byte)0x80, (byte)0xFF)] byte wm,
                                 [Values(0b000u, 0b001u, 0b010u, 0b011u,               // <UXTB, UXTH, LSL|UXTW, UXTX,
                                         0b100u, 0b101u, 0b110u, 0b111u)] uint extend, //  SXTB, SXTH, SXTW, SXTX>
                                 [Values(0u, 1u, 2u, 3u, 4u)] uint amount)
        {
            uint opcode = 0x6B200000; // SUBS W0, W0, W0, UXTB #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((extend & 7) << 13) | ((amount & 7) << 10);

            SingleOpcode(opcode, x1: wnWsp, x2: wm, x31: wnWsp);

            CompareAgainstUnicorn();
        }
#endif
    }
}
