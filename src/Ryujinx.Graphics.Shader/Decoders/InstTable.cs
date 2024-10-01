using Ryujinx.Graphics.Shader.Instructions;
using System;

namespace Ryujinx.Graphics.Shader.Decoders
{
    static class InstTable
    {
        private const int EncodingBits = 14;

        private readonly struct TableEntry
        {
            public InstName Name { get; }
            public InstEmitter Emitter { get; }
            public InstProps Props { get; }

            public int XBits { get; }

            public TableEntry(InstName name, InstEmitter emitter, InstProps props, int xBits)
            {
                Name = name;
                Emitter = emitter;
                Props = props;
                XBits = xBits;
            }
        }

        private static readonly TableEntry[] _opCodes;

        static InstTable()
        {
            _opCodes = new TableEntry[1 << EncodingBits];

            #region Instructions
#pragma warning disable IDE0055 // Disable formatting
            Add("1110111110100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Al2p,        InstEmit.Al2p,        InstProps.Rd  | InstProps.Ra);
            Add("1110111111011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ald,         InstEmit.Ald,         InstProps.Rd  | InstProps.Ra);
            Add("1110111111110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ast,         InstEmit.Ast,         InstProps.Ra  | InstProps.Rb2 | InstProps.Rc);
            Add("11101101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Atom,        InstEmit.Atom,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("111011101111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.AtomCas,     InstEmit.AtomCas,     InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("11101100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Atoms,       InstEmit.Atoms,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("111011100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.AtomsCas,    InstEmit.AtomsCas,    InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("1111000010111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.B2r,         InstEmit.B2r,         InstProps.Rd  | InstProps.Ra  | InstProps.VPd);
            Add("1111000010101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bar,         InstEmit.Bar,         InstProps.Ra  | InstProps.Ps);
            Add("0101110000000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bfe,         InstEmit.BfeR,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x00000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bfe,         InstEmit.BfeI,        InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110000000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bfe,         InstEmit.BfeC,        InstProps.Rd  | InstProps.Ra);
            Add("0101101111110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bfi,         InstEmit.BfiR,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011011x11110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bfi,         InstEmit.BfiI,        InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("0100101111110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bfi,         InstEmit.BfiC,        InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("0101001111110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bfi,         InstEmit.BfiRc,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("111000111010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bpt,         InstEmit.Bpt,         InstProps.NoPred);
            Add("111000100100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bra,         InstEmit.Bra,         InstProps.Bra);
            Add("111000110100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Brk,         InstEmit.Brk,         InstProps.Bra);
            Add("111000100101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Brx,         InstEmit.Brx,         InstProps.Ra  | InstProps.Bra);
            Add("111000100110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Cal,         InstEmit.Cal,         InstProps.Bra | InstProps.NoPred);
            Add("11101111011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Cctl,        InstEmit.Cctl,        InstProps.Ra);
            Add("1110111110000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Cctll,       InstEmit.Cctll,       InstProps.Ra);
            Add("1110101111110xx0000000000000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Cctlt,       InstEmit.Cctlt);
            Add("1110101111101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Cctlt,       InstEmit.Cctlt,       InstProps.Rc);
            Add("111000110101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Cont,        InstEmit.Cont,        InstProps.Bra);
            Add("0101000010011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Cset,        InstEmit.Cset,        InstProps.Rd  | InstProps.Ps);
            Add("0101000010100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Csetp,       InstEmit.Csetp,       InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("0101000011001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Cs2r,        InstEmit.Cs2r,        InstProps.Rd);
            Add("0101110001110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dadd,        InstEmit.DaddR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x01110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dadd,        InstEmit.DaddI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110001110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dadd,        InstEmit.DaddC,       InstProps.Rd  | InstProps.Ra);
            Add("1111000011110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Depbar,      InstEmit.Depbar);
            Add("010110110111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dfma,        InstEmit.DfmaR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011011x0111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dfma,        InstEmit.DfmaI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("010010110111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dfma,        InstEmit.DfmaC,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010100110111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dfma,        InstEmit.DfmaRc,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("0101110001010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dmnmx,       InstEmit.DmnmxR,      InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Ps);
            Add("0011100x01010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dmnmx,       InstEmit.DmnmxI,      InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Ps);
            Add("0100110001010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dmnmx,       InstEmit.DmnmxC,      InstProps.Rd  | InstProps.Ra  | InstProps.Ps);
            Add("0101110010000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dmul,        InstEmit.DmulR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x10000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dmul,        InstEmit.DmulI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110010000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dmul,        InstEmit.DmulC,       InstProps.Rd  | InstProps.Ra);
            Add("010110010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dset,        InstEmit.DsetR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Ps);
            Add("0011001x0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dset,        InstEmit.DsetI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Ps);
            Add("010010010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dset,        InstEmit.DsetC,       InstProps.Rd  | InstProps.Ra  | InstProps.Ps);
            Add("010110111000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dsetp,       InstEmit.DsetpR,      InstProps.Ra  | InstProps.Rb  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("0011011x1000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dsetp,       InstEmit.DsetpI,      InstProps.Ra  | InstProps.Ib  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("010010111000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Dsetp,       InstEmit.DsetpC,      InstProps.Ra  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("111000110000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Exit,        InstEmit.Exit,        InstProps.Bra);
            Add("0101110010101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.F2f,         InstEmit.F2fR,        InstProps.Rd  | InstProps.Rb);
            Add("0011100x10101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.F2f,         InstEmit.F2fI,        InstProps.Rd  | InstProps.Ib);
            Add("0100110010101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.F2f,         InstEmit.F2fC,        InstProps.Rd);
            Add("0101110010110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.F2i,         InstEmit.F2iR,        InstProps.Rd  | InstProps.Rb);
            Add("0011100x10110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.F2i,         InstEmit.F2iI,        InstProps.Rd  | InstProps.Ib);
            Add("0100110010110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.F2i,         InstEmit.F2iC,        InstProps.Rd);
            Add("0101110001011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fadd,        InstEmit.FaddR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x01011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fadd,        InstEmit.FaddI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110001011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fadd,        InstEmit.FaddC,       InstProps.Rd  | InstProps.Ra);
            Add("000010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fadd32i,     InstEmit.Fadd32i,     InstProps.Rd  | InstProps.Ra);
            Add("0101110010001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fchk,        InstEmit.FchkR,       InstProps.Ra  | InstProps.Rb  | InstProps.Pd);
            Add("0011100x10001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fchk,        InstEmit.FchkI,       InstProps.Ra  | InstProps.Ib  | InstProps.Pd);
            Add("0100110010001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fchk,        InstEmit.FchkC,       InstProps.Ra  | InstProps.Pd);
            Add("010110111010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fcmp,        InstEmit.FcmpR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011011x1010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fcmp,        InstEmit.FcmpI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("010010111010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fcmp,        InstEmit.FcmpC,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010100111010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fcmp,        InstEmit.FcmpRc,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010110011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ffma,        InstEmit.FfmaR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011001x1xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ffma,        InstEmit.FfmaI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("010010011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ffma,        InstEmit.FfmaC,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010100011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ffma,        InstEmit.FfmaRc,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("000011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ffma32i,     InstEmit.Ffma32i,     InstProps.Rd  | InstProps.Ra);
            Add("0101110000110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Flo,         InstEmit.FloR,        InstProps.Rd  | InstProps.Rb);
            Add("0011100x00110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Flo,         InstEmit.FloI,        InstProps.Rd  | InstProps.Ib);
            Add("0100110000110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Flo,         InstEmit.FloC,        InstProps.Rd);
            Add("0101110001100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fmnmx,       InstEmit.FmnmxR,      InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Ps);
            Add("0011100x01100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fmnmx,       InstEmit.FmnmxI,      InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Ps);
            Add("0100110001100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fmnmx,       InstEmit.FmnmxC,      InstProps.Rd  | InstProps.Ra  | InstProps.Ps);
            Add("0101110001101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fmul,        InstEmit.FmulR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x01101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fmul,        InstEmit.FmulI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110001101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fmul,        InstEmit.FmulC,       InstProps.Rd  | InstProps.Ra);
            Add("00011110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fmul32i,     InstEmit.Fmul32i,     InstProps.Rd  | InstProps.Ra);
            Add("01011000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fset,        InstEmit.FsetR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Ps);
            Add("0011000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fset,        InstEmit.FsetI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Ps);
            Add("01001000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fset,        InstEmit.FsetC,       InstProps.Rd  | InstProps.Ra  | InstProps.Ps);
            Add("010110111011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fsetp,       InstEmit.FsetpR,      InstProps.Ra  | InstProps.Rb  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("0011011x1011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fsetp,       InstEmit.FsetpI,      InstProps.Ra  | InstProps.Ib  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("010010111011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fsetp,       InstEmit.FsetpC,      InstProps.Ra  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("0101000011111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Fswzadd,     InstEmit.Fswzadd,     InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("111000101100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Getcrsptr,   InstEmit.Getcrsptr,   InstProps.Rd  | InstProps.NoPred);
            Add("111000101101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Getlmembase, InstEmit.Getlmembase, InstProps.Rd  | InstProps.NoPred);
            Add("0101110100010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hadd2,       InstEmit.Hadd2R,      InstProps.Rd  | InstProps.Ra);
            Add("0111101x0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hadd2,       InstEmit.Hadd2I,      InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0111101x1xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hadd2,       InstEmit.Hadd2C,      InstProps.Rd  | InstProps.Ra);
            Add("0010110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hadd232i,    InstEmit.Hadd232i,    InstProps.Rd  | InstProps.Ra);
            Add("0101110100000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hfma2,       InstEmit.Hfma2R,      InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("01110xxx0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hfma2,       InstEmit.Hfma2I,      InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("01110xxx1xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hfma2,       InstEmit.Hfma2C,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("01100xxx1xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hfma2,       InstEmit.Hfma2Rc,     InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("0010100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hfma2,       InstEmit.Hfma232i,    InstProps.Rd  | InstProps.Ra);
            Add("0101110100001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hmul2,       InstEmit.Hmul2R,      InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0111100x0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hmul2,       InstEmit.Hmul2I,      InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0111100x1xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hmul2,       InstEmit.Hmul2C,      InstProps.Rd  | InstProps.Ra);
            Add("0010101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hmul232i,    InstEmit.Hmul232i,    InstProps.Rd  | InstProps.Ra);
            Add("0101110100011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hset2,       InstEmit.Hset2R,      InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Ps);
            Add("0111110x0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hset2,       InstEmit.Hset2I,      InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Ps);
            Add("0111110x1xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hset2,       InstEmit.Hset2C,      InstProps.Rd  | InstProps.Ra  | InstProps.Ps);
            Add("0101110100100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hsetp2,      InstEmit.Hsetp2R,     InstProps.Ra  | InstProps.Rb  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("0111111x0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hsetp2,      InstEmit.Hsetp2I,     InstProps.Ra  | InstProps.Ib  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("0111111x1xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Hsetp2,      InstEmit.Hsetp2C,     InstProps.Ra  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("0101110010111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.I2f,         InstEmit.I2fR,        InstProps.Rd  | InstProps.Rb);
            Add("0011100x10111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.I2f,         InstEmit.I2fI,        InstProps.Rd  | InstProps.Ib);
            Add("0100110010111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.I2f,         InstEmit.I2fC,        InstProps.Rd);
            Add("0101110011100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.I2i,         InstEmit.I2iR,        InstProps.Rd  | InstProps.Rb);
            Add("0011100x11100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.I2i,         InstEmit.I2iI,        InstProps.Rd  | InstProps.Ib);
            Add("0100110011100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.I2i,         InstEmit.I2iC,        InstProps.Rd);
            Add("0101110000010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iadd,        InstEmit.IaddR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x00010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iadd,        InstEmit.IaddI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110000010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iadd,        InstEmit.IaddC,       InstProps.Rd  | InstProps.Ra);
            Add("0001110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iadd32i,     InstEmit.Iadd32i,     InstProps.Rd  | InstProps.Ra);
            Add("010111001100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iadd3,       InstEmit.Iadd3R,      InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011100x1100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iadd3,       InstEmit.Iadd3I,      InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("010011001100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iadd3,       InstEmit.Iadd3C,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010110110100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Icmp,        InstEmit.IcmpR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011011x0100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Icmp,        InstEmit.IcmpI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("010010110100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Icmp,        InstEmit.IcmpC,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010100110100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Icmp,        InstEmit.IcmpRc,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("111000111001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ide,         InstEmit.Ide,         InstProps.NoPred);
            Add("0101001111111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Idp,         InstEmit.IdpR,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0101001111011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Idp,         InstEmit.IdpC,        InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010110100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imad,        InstEmit.ImadR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011010x0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imad,        InstEmit.ImadI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("010010100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imad,        InstEmit.ImadC,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010100100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imad,        InstEmit.ImadRc,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("000100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imad32i,     InstEmit.Imad32i,     InstProps.Rd  | InstProps.Ra);
            Add("010110101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imadsp,      InstEmit.ImadspR,     InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011010x1xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imadsp,      InstEmit.ImadspI,     InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("010010101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imadsp,      InstEmit.ImadspC,     InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010100101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imadsp,      InstEmit.ImadspRc,    InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("0101110000100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imnmx,       InstEmit.ImnmxR,      InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Ps);
            Add("0011100x00100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imnmx,       InstEmit.ImnmxI,      InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Ps);
            Add("0100110000100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imnmx,       InstEmit.ImnmxC,      InstProps.Rd  | InstProps.Ra  | InstProps.Ps);
            Add("0101110000111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imul,        InstEmit.ImulR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x00111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imul,        InstEmit.ImulI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110000111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imul,        InstEmit.ImulC,       InstProps.Rd  | InstProps.Ra);
            Add("00011111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Imul32i,     InstEmit.Imul32i,     InstProps.Rd  | InstProps.Ra);
            Add("11100000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ipa,         InstEmit.Ipa,         InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("1110111111010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Isberd,      InstEmit.Isberd,      InstProps.Rd  | InstProps.Ra);
            Add("0101110000011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iscadd,      InstEmit.IscaddR,     InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x00011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iscadd,      InstEmit.IscaddI,     InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110000011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iscadd,      InstEmit.IscaddC,     InstProps.Rd  | InstProps.Ra);
            Add("000101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iscadd32i,   InstEmit.Iscadd32i,   InstProps.Rd  | InstProps.Ra);
            Add("010110110101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iset,        InstEmit.IsetR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Ps);
            Add("0011011x0101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iset,        InstEmit.IsetI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Ps);
            Add("010010110101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Iset,        InstEmit.IsetC,       InstProps.Rd  | InstProps.Ra  | InstProps.Ps);
            Add("010110110110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Isetp,       InstEmit.IsetpR,      InstProps.Ra  | InstProps.Rb  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("0011011x0110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Isetp,       InstEmit.IsetpI,      InstProps.Ra  | InstProps.Ib  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("010010110110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Isetp,       InstEmit.IsetpC,      InstProps.Ra  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("111000100010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Jcal,        InstEmit.Jcal,        InstProps.Bra);
            Add("111000100001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Jmp,         InstEmit.Jmp,         InstProps.Ra  | InstProps.Bra);
            Add("111000100000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Jmx,         InstEmit.Jmx,         InstProps.Ra  | InstProps.Bra);
            Add("111000110011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Kil,         InstEmit.Kil,         InstProps.Bra);
            Add("100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ld,          InstEmit.Ld,          InstProps.Rd  | InstProps.Ra);
            Add("1110111110010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ldc,         InstEmit.Ldc,         InstProps.Rd  | InstProps.Ra);
            Add("1110111011010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ldg,         InstEmit.Ldg,         InstProps.Rd  | InstProps.Ra);
            Add("1110111101000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ldl,         InstEmit.Ldl,         InstProps.Rd  | InstProps.Ra);
            Add("1110111101001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lds,         InstEmit.Lds,         InstProps.Rd  | InstProps.Ra);
            Add("0101101111010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lea,         InstEmit.LeaR,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.LPd);
            Add("0011011x11010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lea,         InstEmit.LeaI,        InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.LPd);
            Add("0100101111010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lea,         InstEmit.LeaC,        InstProps.Rd  | InstProps.Ra  | InstProps.LPd);
            Add("0101101111011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.LeaHi,       InstEmit.LeaHiR,      InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc  | InstProps.LPd);
            Add("000110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.LeaHi,       InstEmit.LeaHiC,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc  | InstProps.LPd);
            Add("0101000011010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lepc,        InstEmit.Lepc);
            Add("111000110001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Longjmp,     InstEmit.Longjmp,     InstProps.Bra);
            Add("0101110001000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lop,         InstEmit.LopR,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.LPd);
            Add("0011100x01000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lop,         InstEmit.LopI,        InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.LPd);
            Add("0100110001000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lop,         InstEmit.LopC,        InstProps.Rd  | InstProps.Ra  | InstProps.LPd);
            Add("0101101111100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lop3,        InstEmit.Lop3R,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc  | InstProps.LPd);
            Add("001111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lop3,        InstEmit.Lop3I,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("0000001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lop3,        InstEmit.Lop3C,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("000001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Lop32i,      InstEmit.Lop32i,      InstProps.Rd  | InstProps.Ra);
            Add("1110111110011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Membar,      InstEmit.Membar);
            Add("0101110010011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Mov,         InstEmit.MovR,        InstProps.Rd  | InstProps.Ra);
            Add("0011100x10011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Mov,         InstEmit.MovI,        InstProps.Rd  | InstProps.Ib);
            Add("0100110010011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Mov,         InstEmit.MovC,        InstProps.Rd);
            Add("000000010000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Mov32i,      InstEmit.Mov32i,      InstProps.Rd);
            Add("0101000010000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Mufu,        InstEmit.Mufu,        InstProps.Rd  | InstProps.Ra);
            Add("0101000010110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Nop,         InstEmit.Nop);
            Add("1111101111100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Out,         InstEmit.OutR,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("1111011x11100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Out,         InstEmit.OutI,        InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("1110101111100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Out,         InstEmit.OutC,        InstProps.Rd  | InstProps.Ra);
            Add("0101110011101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.P2r,         InstEmit.P2rR,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x11101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.P2r,         InstEmit.P2rI,        InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110011101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.P2r,         InstEmit.P2rC,        InstProps.Rd  | InstProps.Ra);
            Add("111000101010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Pbk,         InstEmit.Pbk,         InstProps.NoPred);
            Add("111000101011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Pcnt,        InstEmit.Pcnt,        InstProps.NoPred);
            Add("111000100011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Pexit,       InstEmit.Pexit);
            Add("1110111111101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Pixld,       InstEmit.Pixld,       InstProps.Rd  | InstProps.Ra  | InstProps.VPd);
            Add("111000101000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Plongjmp,    InstEmit.Plongjmp,    InstProps.Bra | InstProps.NoPred);
            Add("0101110000001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Popc,        InstEmit.PopcR,       InstProps.Rd  | InstProps.Rb);
            Add("0011100x00001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Popc,        InstEmit.PopcI,       InstProps.Rd  | InstProps.Ib);
            Add("0100110000001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Popc,        InstEmit.PopcC,       InstProps.Rd);
            Add("111000100111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Pret,        InstEmit.Pret,        InstProps.NoPred);
            Add("010110111100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Prmt,        InstEmit.PrmtR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011011x1100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Prmt,        InstEmit.PrmtI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("010010111100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Prmt,        InstEmit.PrmtC,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010100111100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Prmt,        InstEmit.PrmtRc,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("0101000010001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Pset,        InstEmit.Pset,        InstProps.Rd  | InstProps.Ps);
            Add("0101000010010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Psetp,       InstEmit.Psetp,       InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("1111000011000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.R2b,         InstEmit.R2b,         InstProps.Rb);
            Add("0101110011110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.R2p,         InstEmit.R2pR,        InstProps.Ra  | InstProps.Rb);
            Add("0011100x11110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.R2p,         InstEmit.R2pI,        InstProps.Ra  | InstProps.Ib);
            Add("0100110011110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.R2p,         InstEmit.R2pC,        InstProps.Ra);
            Add("111000111000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ram,         InstEmit.Ram,         InstProps.NoPred);
            Add("1110101111111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Red,         InstEmit.Red,         InstProps.Ra  | InstProps.Rb2);
            Add("111000110010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ret,         InstEmit.Ret,         InstProps.Bra);
            Add("0101110010010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Rro,         InstEmit.RroR,        InstProps.Rd  | InstProps.Rb);
            Add("0011100x10010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Rro,         InstEmit.RroI,        InstProps.Rd  | InstProps.Ib);
            Add("0100110010010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Rro,         InstEmit.RroC,        InstProps.Rd);
            Add("111000110110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Rtt,         InstEmit.Rtt,         InstProps.NoPred);
            Add("1111000011001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.S2r,         InstEmit.S2r,         InstProps.Rd);
            Add("111000110111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Sam,         InstEmit.Sam,         InstProps.NoPred);
            Add("0101110010100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Sel,         InstEmit.SelR,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Ps);
            Add("0011100x10100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Sel,         InstEmit.SelI,        InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Ps);
            Add("0100110010100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Sel,         InstEmit.SelC,        InstProps.Rd  | InstProps.Ra  | InstProps.Ps);
            Add("111000101110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Setcrsptr,   InstEmit.Setcrsptr,   InstProps.Ra  | InstProps.NoPred);
            Add("111000101111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Setlmembase, InstEmit.Setlmembase, InstProps.Ra  | InstProps.NoPred);
            Add("0101101111111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shf,         InstEmit.ShfLR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0101110011111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shf,         InstEmit.ShfRR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011011x11111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shf,         InstEmit.ShfLI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("0011100x11111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shf,         InstEmit.ShfRI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("1110111100010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shfl,        InstEmit.Shfl,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc  | InstProps.LPd);
            Add("0101110001001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shl,         InstEmit.ShlR,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x01001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shl,         InstEmit.ShlI,        InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110001001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shl,         InstEmit.ShlC,        InstProps.Rd  | InstProps.Ra);
            Add("0101110000101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shr,         InstEmit.ShrR,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("0011100x00101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shr,         InstEmit.ShrI,        InstProps.Rd  | InstProps.Ra  | InstProps.Ib);
            Add("0100110000101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Shr,         InstEmit.ShrC,        InstProps.Rd  | InstProps.Ra);
            Add("111000101001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ssy,         InstEmit.Ssy,         InstProps.NoPred);
            Add("101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.St,          InstEmit.St,          InstProps.Rd  | InstProps.Ra);
            Add("1110111011011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Stg,         InstEmit.Stg,         InstProps.Rd  | InstProps.Ra);
            Add("1110111101010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Stl,         InstEmit.Stl,         InstProps.Rd  | InstProps.Ra);
            Add("1110111010100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Stp,         InstEmit.Stp,         InstProps.NoPred);
            Add("1110111101011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Sts,         InstEmit.Sts,         InstProps.Rd  | InstProps.Ra);
            Add("1110101001110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SuatomB,     InstEmit.SuatomB,     InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("11101010x0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Suatom,      InstEmit.Suatom,      InstProps.Rd  | InstProps.Ra  | InstProps.Rb);
            Add("1110101110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SuatomB2,    InstEmit.SuatomB2,    InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("1110101011010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SuatomCasB,  InstEmit.SuatomCasB,  InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc  | InstProps.SPd);
            Add("1110101x1xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SuatomCas,   InstEmit.SuatomCas,   InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.SPd);
            Add("1110101100010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SuldDB,      InstEmit.SuldDB,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc  | InstProps.SPd | InstProps.TexB);
            Add("1110101100011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SuldD,       InstEmit.SuldD,       InstProps.Rd  | InstProps.Ra  | InstProps.SPd | InstProps.Tex);
            Add("1110101100000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SuldB,       InstEmit.SuldB,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc  | InstProps.SPd | InstProps.TexB);
            Add("1110101100001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Suld,        InstEmit.Suld,        InstProps.Rd  | InstProps.Ra  | InstProps.SPd | InstProps.Tex);
            Add("1110101101010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SuredB,      InstEmit.SuredB,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("1110101101011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Sured,       InstEmit.Sured,       InstProps.Rd  | InstProps.Ra);
            Add("1110101100110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SustDB,      InstEmit.SustDB,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc  | InstProps.TexB);
            Add("1110101100111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SustD,       InstEmit.SustD,       InstProps.Rd  | InstProps.Ra  | InstProps.Tex);
            Add("1110101100100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.SustB,       InstEmit.SustB,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc  | InstProps.TexB);
            Add("1110101100101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Sust,        InstEmit.Sust,        InstProps.Rd  | InstProps.Ra  | InstProps.Tex);
            Add("1111000011111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Sync,        InstEmit.Sync,        InstProps.Bra);
            Add("11000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Tex,         InstEmit.Tex,         InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.TPd | InstProps.Tex);
            Add("1101111010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.TexB,        InstEmit.TexB,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.TPd | InstProps.TexB);
            Add("1101100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Texs,        InstEmit.Texs,        InstProps.Rd  | InstProps.Rd2 | InstProps.Ra  | InstProps.Rb  | InstProps.Tex);
            Add("1101000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.TexsF16,     InstEmit.TexsF16,     InstProps.Rd  | InstProps.Rd2 | InstProps.Ra  | InstProps.Rb  | InstProps.Tex);
            Add("11011100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Tld,         InstEmit.Tld,         InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.TPd | InstProps.Tex);
            Add("11011101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.TldB,        InstEmit.TldB,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.TPd | InstProps.TexB);
            Add("1101101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Tlds,        InstEmit.Tlds,        InstProps.Rd  | InstProps.Rd2 | InstProps.Ra  | InstProps.Rb  | InstProps.Tex);
            Add("1101001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.TldsF16,     InstEmit.TldsF16,     InstProps.Rd  | InstProps.Rd2 | InstProps.Ra  | InstProps.Rb  | InstProps.Tex);
            Add("110010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Tld4,        InstEmit.Tld4,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.TPd | InstProps.Tex);
            Add("1101111011xxxxxxxxxxxxx0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Tld4B,       InstEmit.Tld4B,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.TPd | InstProps.TexB);
            Add("1101111100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Tld4s,       InstEmit.Tld4s,       InstProps.Rd  | InstProps.Rd2 | InstProps.Ra  | InstProps.Rb  | InstProps.Tex);
            Add("1101111110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Tld4sF16,    InstEmit.Tld4sF16,    InstProps.Rd  | InstProps.Rd2 | InstProps.Ra  | InstProps.Rb  | InstProps.Tex);
            Add("1101111101011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Tmml,        InstEmit.Tmml,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Tex);
            Add("1101111101100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.TmmlB,       InstEmit.TmmlB,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.TexB);
            Add("1101111101000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Txa,         InstEmit.Txa,         InstProps.Rd  | InstProps.Ra  | InstProps.Tex);
            Add("110111100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Txd,         InstEmit.Txd,         InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.TPd | InstProps.Tex);
            Add("1101111001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.TxdB,        InstEmit.TxdB,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.TPd | InstProps.TexB);
            Add("1101111101001xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Txq,         InstEmit.Txq,         InstProps.Rd  | InstProps.Ra  | InstProps.Tex);
            Add("1101111101010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.TxqB,        InstEmit.TxqB,        InstProps.Rd  | InstProps.Ra  | InstProps.TexB);
            Add("01010100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Vabsdiff,    InstEmit.Vabsdiff,    InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("010100000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Vabsdiff4,   InstEmit.Vabsdiff4,   InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("001000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Vadd,        InstEmit.Vadd,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("01011111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Vmad,        InstEmit.Vmad,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011101xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Vmnmx,       InstEmit.Vmnmx,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0101000011011xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Vote,        InstEmit.Vote,        InstProps.Rd  | InstProps.VPd | InstProps.Ps);
            Add("0101000011100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Votevtg,     InstEmit.Votevtg);
            Add("0100000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Vset,        InstEmit.Vset,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0101000011110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Vsetp,       InstEmit.Vsetp,       InstProps.Ra  | InstProps.Rb  | InstProps.Pd  | InstProps.Pdn | InstProps.Ps);
            Add("01010111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Vshl,        InstEmit.Vshl,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("01010110xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Vshr,        InstEmit.Vshr,        InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0101101100xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Xmad,        InstEmit.XmadR,       InstProps.Rd  | InstProps.Ra  | InstProps.Rb  | InstProps.Rc);
            Add("0011011x00xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Xmad,        InstEmit.XmadI,       InstProps.Rd  | InstProps.Ra  | InstProps.Ib  | InstProps.Rc);
            Add("0100111xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Xmad,        InstEmit.XmadC,       InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
            Add("010100010xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Xmad,        InstEmit.XmadRc,      InstProps.Rd  | InstProps.Ra  | InstProps.Rc);
#pragma warning restore IDE0055
            #endregion
        }

        private static void Add(string encoding, InstName name, InstEmitter emitter, InstProps props = InstProps.None)
        {
            ReadOnlySpan<char> encodingPart = encoding.AsSpan(0, EncodingBits);

            int bit = encodingPart.Length - 1;
            int value = 0;
            int xMask = 0;
            int xBits = 0;

            int[] xPos = new int[encodingPart.Length];

            for (int index = 0; index < encodingPart.Length; index++, bit--)
            {
                char chr = encodingPart[index];

                if (chr == '1')
                {
                    value |= 1 << bit;
                }
                else if (chr == 'x')
                {
                    xMask |= 1 << bit;

                    xPos[xBits++] = bit;
                }
            }

            xMask = ~xMask;

            TableEntry entry = new(name, emitter, props, xBits);

            for (int index = 0; index < (1 << xBits); index++)
            {
                value &= xMask;

                for (int x = 0; x < xBits; x++)
                {
                    value |= ((index >> x) & 1) << xPos[x];
                }

                if (_opCodes[value].Emitter == null || _opCodes[value].XBits > xBits)
                {
                    _opCodes[value] = entry;
                }
            }
        }

        public static InstOp GetOp(ulong address, ulong opCode)
        {
            ref TableEntry entry = ref _opCodes[opCode >> (64 - EncodingBits)];

            if (entry.Emitter != null)
            {
                return new InstOp(address, opCode, entry.Name, entry.Emitter, entry.Props);
            }

            return new InstOp(address, opCode, InstName.Invalid, null, InstProps.None);
        }
    }
}
