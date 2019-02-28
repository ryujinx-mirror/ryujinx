using Ryujinx.Graphics.Texture;
using System;

using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        private const int ____ = 0x0;
        private const int R___ = 0x1;
        private const int _G__ = 0x2;
        private const int RG__ = 0x3;
        private const int __B_ = 0x4;
        private const int RGB_ = 0x7;
        private const int ___A = 0x8;
        private const int R__A = 0x9;
        private const int _G_A = 0xa;
        private const int RG_A = 0xb;
        private const int __BA = 0xc;
        private const int R_BA = 0xd;
        private const int _GBA = 0xe;
        private const int RGBA = 0xf;

        private static int[,] MaskLut = new int[,]
        {
            { ____, ____, ____, ____, ____, ____, ____, ____ },
            { R___, _G__, __B_, ___A, RG__, R__A, _G_A, __BA },
            { R___, _G__, __B_, ___A, RG__, ____, ____, ____ },
            { RGB_, RG_A, R_BA, _GBA, RGBA, ____, ____, ____ }
        };

        private static GalTextureTarget TexToTextureTarget(int TexType, bool IsArray)
        {
            switch (TexType)
            {
                case 0:
                    return IsArray ? GalTextureTarget.OneDArray : GalTextureTarget.OneD;
                case 2:
                    return IsArray ? GalTextureTarget.TwoDArray : GalTextureTarget.TwoD;
                case 4:
                    if (IsArray)
                        throw new InvalidOperationException($"ARRAY bit set on a TEX with 3D texture!");
                    return GalTextureTarget.ThreeD;
                case 6:
                    return IsArray ? GalTextureTarget.CubeArray : GalTextureTarget.CubeMap;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static GalTextureTarget TexsToTextureTarget(int TexType)
        {
            switch (TexType)
            {
                case 0:
                    return GalTextureTarget.OneD;
                case 2:
                case 4:
                case 6:
                case 8:
                case 0xa:
                case 0xc:
                    return GalTextureTarget.TwoD;
                case 0xe:
                case 0x10:
                case 0x12:
                    return GalTextureTarget.TwoDArray;
                case 0x14:
                case 0x16:
                    return GalTextureTarget.ThreeD;
                case 0x18:
                case 0x1a:
                    return GalTextureTarget.CubeMap;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static GalTextureTarget TldsToTextureTarget(int TexType)
        {
            switch (TexType)
            {
                case 0:
                case 2:
                    return GalTextureTarget.OneD;
                case 4:
                case 8:
                case 0xa:
                case 0xc:
                case 0x18:
                    return GalTextureTarget.TwoD;
                case 0x10:
                    return GalTextureTarget.TwoDArray;
                case 0xe:
                    return GalTextureTarget.ThreeD;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static void Ld_A(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode[] Opers = OpCode.Abuf20();

            //Used by GS
            ShaderIrOperGpr Vertex = OpCode.Gpr39();

            int Index = 0;

            foreach (ShaderIrNode OperA in Opers)
            {
                ShaderIrOperGpr OperD = OpCode.Gpr0();

                OperD.Index += Index++;

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OperD, OperA)));
            }
        }

        public static void Ld_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            int CbufPos   = OpCode.Read(22, 0x3fff);
            int CbufIndex = OpCode.Read(36, 0x1f);
            int Type      = OpCode.Read(48, 7);

            if (Type > 5)
            {
                throw new InvalidOperationException();
            }

            ShaderIrOperGpr Temp = ShaderIrOperGpr.MakeTemporary();

            Block.AddNode(new ShaderIrAsg(Temp, OpCode.Gpr8()));

            int Count = Type == 5 ? 2 : 1;

            for (int Index = 0; Index < Count; Index++)
            {
                ShaderIrOperCbuf OperA = new ShaderIrOperCbuf(CbufIndex, CbufPos, Temp);

                ShaderIrOperGpr OperD = OpCode.Gpr0();

                OperA.Pos   += Index;
                OperD.Index += Index;

                if (!OperD.IsValidRegister)
                {
                    break;
                }

                ShaderIrNode Node = OperA;

                if (Type < 4)
                {
                    //This is a 8 or 16 bits type.
                    bool Signed = (Type & 1) != 0;

                    int Size = 8 << (Type >> 1);

                    Node = ExtendTo32(Node, Signed, Size);
                }

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OperD, Node)));
            }
        }

        public static void St_A(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode[] Opers = OpCode.Abuf20();

            int Index = 0;

            foreach (ShaderIrNode OperA in Opers)
            {
                ShaderIrOperGpr OperD = OpCode.Gpr0();

                OperD.Index += Index++;

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OperA, OperD)));
            }
        }

        public static void Texq(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode OperD = OpCode.Gpr0();
            ShaderIrNode OperA = OpCode.Gpr8();

            ShaderTexqInfo Info = (ShaderTexqInfo)(OpCode.Read(22, 0x1f));

            ShaderIrMetaTexq Meta0 = new ShaderIrMetaTexq(Info, 0);
            ShaderIrMetaTexq Meta1 = new ShaderIrMetaTexq(Info, 1);

            ShaderIrNode OperC = OpCode.Imm13_36();

            ShaderIrOp Op0 = new ShaderIrOp(ShaderIrInst.Texq, OperA, null, OperC, Meta0);
            ShaderIrOp Op1 = new ShaderIrOp(ShaderIrInst.Texq, OperA, null, OperC, Meta1);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OperD, Op0)));
            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OperA, Op1))); //Is this right?
        }

        public static void Tex(ShaderIrBlock Block, long OpCode, int Position)
        {
            TextureInstructionSuffix Suffix;

            int RawSuffix = OpCode.Read(0x34, 0x38);

            switch (RawSuffix)
            {
                case 0:
                    Suffix = TextureInstructionSuffix.None;
                    break;
                case 0x8:
                    Suffix = TextureInstructionSuffix.LZ;
                    break;
                case 0x10:
                    Suffix = TextureInstructionSuffix.LB;
                    break;
                case 0x18:
                    Suffix = TextureInstructionSuffix.LL;
                    break;
                case 0x30:
                    Suffix = TextureInstructionSuffix.LBA;
                    break;
                case 0x38:
                    Suffix = TextureInstructionSuffix.LLA;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Suffix for TEX instruction {RawSuffix}");
            }

            bool IsOffset = OpCode.Read(0x36);

            if (IsOffset)
                Suffix |= TextureInstructionSuffix.AOffI;

            EmitTex(Block, OpCode, Suffix, GprHandle: false);
        }

        public static void Tex_B(ShaderIrBlock Block, long OpCode, int Position)
        {
            TextureInstructionSuffix Suffix;

            int RawSuffix = OpCode.Read(0x24, 0xe);

            switch (RawSuffix)
            {
                case 0:
                    Suffix = TextureInstructionSuffix.None;
                    break;
                case 0x2:
                    Suffix = TextureInstructionSuffix.LZ;
                    break;
                case 0x4:
                    Suffix = TextureInstructionSuffix.LB;
                    break;
                case 0x6:
                    Suffix = TextureInstructionSuffix.LL;
                    break;
                case 0xc:
                    Suffix = TextureInstructionSuffix.LBA;
                    break;
                case 0xe:
                    Suffix = TextureInstructionSuffix.LLA;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Suffix for TEX.B instruction {RawSuffix}");
            }

            bool IsOffset = OpCode.Read(0x23);

            if (IsOffset)
                Suffix |= TextureInstructionSuffix.AOffI;

            EmitTex(Block, OpCode, Suffix, GprHandle: true);
        }

        private static void EmitTex(ShaderIrBlock Block, long OpCode, TextureInstructionSuffix TextureInstructionSuffix, bool GprHandle)
        {
            bool IsArray = OpCode.HasArray();

            GalTextureTarget TextureTarget = TexToTextureTarget(OpCode.Read(28, 6), IsArray);

            bool HasDepthCompare = OpCode.Read(0x32);

            if (HasDepthCompare)
            {
                TextureInstructionSuffix |= TextureInstructionSuffix.DC;
            }

            ShaderIrOperGpr[] Coords = new ShaderIrOperGpr[ImageUtils.GetCoordsCountTextureTarget(TextureTarget)];

            int IndexExtraCoord = 0;

            if (IsArray)
            {
                IndexExtraCoord++;

                Coords[Coords.Length - 1] = OpCode.Gpr8();
            }


            for (int Index = 0; Index < Coords.Length - IndexExtraCoord; Index++)
            {
                ShaderIrOperGpr CoordReg = OpCode.Gpr8();

                CoordReg.Index += Index;

                CoordReg.Index += IndexExtraCoord;

                if (!CoordReg.IsValidRegister)
                {
                    CoordReg.Index = ShaderIrOperGpr.ZRIndex;
                }

                Coords[Index] = CoordReg;
            }

            int ChMask = OpCode.Read(31, 0xf);

            ShaderIrOperGpr LevelOfDetail = null;
            ShaderIrOperGpr Offset        = null;
            ShaderIrOperGpr DepthCompare  = null;

            // TODO: determine first argument when TEX.B is used
            int OperBIndex = GprHandle ? 1 : 0;

            if ((TextureInstructionSuffix & TextureInstructionSuffix.LL) != 0 ||
                (TextureInstructionSuffix & TextureInstructionSuffix.LB) != 0 ||
                (TextureInstructionSuffix & TextureInstructionSuffix.LBA) != 0 ||
                (TextureInstructionSuffix & TextureInstructionSuffix.LLA) != 0)
            {
                LevelOfDetail        = OpCode.Gpr20();
                LevelOfDetail.Index += OperBIndex;

                OperBIndex++;
            }

            if ((TextureInstructionSuffix & TextureInstructionSuffix.AOffI) != 0)
            {
                Offset        = OpCode.Gpr20();
                Offset.Index += OperBIndex;

                OperBIndex++;
            }

            if ((TextureInstructionSuffix & TextureInstructionSuffix.DC) != 0)
            {
                DepthCompare        = OpCode.Gpr20();
                DepthCompare.Index += OperBIndex;

                OperBIndex++;
            }

            // ???
            ShaderIrNode OperC = GprHandle
                ? (ShaderIrNode)OpCode.Gpr20()
                : (ShaderIrNode)OpCode.Imm13_36();

            ShaderIrInst Inst = GprHandle ? ShaderIrInst.Texb : ShaderIrInst.Texs;

            Coords = CoordsRegistersToTempRegisters(Block, Coords);

            int RegInc = 0;

            for (int Ch = 0; Ch < 4; Ch++)
            {
                if (!IsChannelUsed(ChMask, Ch))
                {
                    continue;
                }

                ShaderIrOperGpr Dst = OpCode.Gpr0();

                Dst.Index += RegInc++;

                if (!Dst.IsValidRegister || Dst.IsConst)
                {
                    continue;
                }

                ShaderIrMetaTex Meta = new ShaderIrMetaTex(Ch, TextureTarget, TextureInstructionSuffix, Coords)
                {
                    LevelOfDetail = LevelOfDetail,
                    Offset        = Offset,
                    DepthCompare  = DepthCompare
                };

                ShaderIrOp Op = new ShaderIrOp(Inst, Coords[0], Coords.Length > 1 ? Coords[1] : null, OperC, Meta);

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(Dst, Op)));
            }
        }

        public static void Texs(ShaderIrBlock Block, long OpCode, int Position)
        {
            TextureInstructionSuffix Suffix;

            int RawSuffix = OpCode.Read(0x34, 0x1e);

            switch (RawSuffix)
            {
                case 0:
                case 0x4:
                case 0x10:
                case 0x16:
                    Suffix = TextureInstructionSuffix.LZ;
                    break;
                case 0x6:
                case 0x1a:
                    Suffix = TextureInstructionSuffix.LL;
                    break;
                case 0x8:
                    Suffix = TextureInstructionSuffix.DC;
                    break;
                case 0x2:
                case 0xe:
                case 0x14:
                case 0x18:
                    Suffix = TextureInstructionSuffix.None;
                    break;
                case 0xa:
                    Suffix = TextureInstructionSuffix.LL | TextureInstructionSuffix.DC;
                    break;
                case 0xc:
                case 0x12:
                    Suffix = TextureInstructionSuffix.LZ | TextureInstructionSuffix.DC;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Suffix for TEXS instruction {RawSuffix}");
            }

            GalTextureTarget TextureTarget = TexsToTextureTarget(OpCode.Read(52, 0x1e));

            EmitTexs(Block, OpCode, ShaderIrInst.Texs, TextureTarget, Suffix);
        }

        public static void Tlds(ShaderIrBlock Block, long OpCode, int Position)
        {
            TextureInstructionSuffix Suffix;

            int RawSuffix = OpCode.Read(0x34, 0x1e);

            switch (RawSuffix)
            {
                case 0:
                case 0x4:
                case 0x8:
                    Suffix = TextureInstructionSuffix.LZ | TextureInstructionSuffix.AOffI;
                    break;
                case 0xc:
                    Suffix = TextureInstructionSuffix.LZ | TextureInstructionSuffix.MZ;
                    break;
                case 0xe:
                case 0x10:
                    Suffix = TextureInstructionSuffix.LZ;
                    break;
                case 0x2:
                case 0xa:
                    Suffix = TextureInstructionSuffix.LL;
                    break;
                case 0x18:
                    Suffix = TextureInstructionSuffix.LL | TextureInstructionSuffix.AOffI;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Suffix for TLDS instruction {RawSuffix}");
            }

            GalTextureTarget TextureTarget = TldsToTextureTarget(OpCode.Read(52, 0x1e));

            EmitTexs(Block, OpCode, ShaderIrInst.Txlf, TextureTarget, Suffix);
        }

        public static void Tld4(ShaderIrBlock Block, long OpCode, int Position)
        {
            TextureInstructionSuffix Suffix;

            int RawSuffix = OpCode.Read(0x34, 0xc);

            switch (RawSuffix)
            {
                case 0:
                    Suffix = TextureInstructionSuffix.None;
                    break;
                case 0x4:
                    Suffix = TextureInstructionSuffix.AOffI;
                    break;
                case 0x8:
                    Suffix = TextureInstructionSuffix.PTP;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Suffix for TLD4 instruction {RawSuffix}");
            }

            bool IsShadow = OpCode.Read(0x32);

            bool IsArray = OpCode.HasArray();
            int  ChMask  = OpCode.Read(31, 0xf);

            GalTextureTarget TextureTarget = TexToTextureTarget(OpCode.Read(28, 6), IsArray);

            if (IsShadow)
            {
                Suffix |= TextureInstructionSuffix.DC;
            }

            EmitTld4(Block, OpCode, TextureTarget, Suffix, ChMask, OpCode.Read(0x38, 0x3), false);
        }

        public static void Tld4s(ShaderIrBlock Block, long OpCode, int Position)
        {
            TextureInstructionSuffix Suffix = TextureInstructionSuffix.None;

            bool IsOffset = OpCode.Read(0x33);
            bool IsShadow = OpCode.Read(0x32);

            if (IsOffset)
            {
                Suffix |= TextureInstructionSuffix.AOffI;
            }

            if (IsShadow)
            {
                Suffix |= TextureInstructionSuffix.DC;
            }

            // TLD4S seems to only support 2D textures with RGBA mask?
            EmitTld4(Block, OpCode, GalTextureTarget.TwoD, Suffix, RGBA, OpCode.Read(0x34, 0x3), true);
        }

        private static void EmitTexs(ShaderIrBlock            Block,
                                     long                     OpCode,
                                     ShaderIrInst             Inst,
                                     GalTextureTarget         TextureTarget,
                                     TextureInstructionSuffix TextureInstructionSuffix)
        {
            if (Inst == ShaderIrInst.Txlf && TextureTarget == GalTextureTarget.CubeArray)
            {
                throw new InvalidOperationException("TLDS instructions cannot use CUBE modifier!");
            }

            bool IsArray = ImageUtils.IsArray(TextureTarget);

            ShaderIrOperGpr[] Coords = new ShaderIrOperGpr[ImageUtils.GetCoordsCountTextureTarget(TextureTarget)];

            ShaderIrOperGpr OperA = OpCode.Gpr8();
            ShaderIrOperGpr OperB = OpCode.Gpr20();

            ShaderIrOperGpr SuffixExtra = OpCode.Gpr20();
            SuffixExtra.Index += 1;

            int CoordStartIndex = 0;

            if (IsArray)
            {
                CoordStartIndex++;
                Coords[Coords.Length - 1] = OpCode.Gpr8();
            }

            switch (Coords.Length - CoordStartIndex)
            {
                case 1:
                    Coords[0] = OpCode.Gpr8();

                    break;
                case 2:
                    Coords[0] = OpCode.Gpr8();
                    Coords[0].Index += CoordStartIndex;

                    break;
                case 3:
                    Coords[0] = OpCode.Gpr8();
                    Coords[0].Index += CoordStartIndex;

                    Coords[1] = OpCode.Gpr8();
                    Coords[1].Index += 1 + CoordStartIndex;

                    break;
                default:
                    throw new NotSupportedException($"{Coords.Length - CoordStartIndex} coords textures aren't supported in TEXS");
            }

            int OperBIndex = 0;

            ShaderIrOperGpr LevelOfDetail = null;
            ShaderIrOperGpr Offset        = null;
            ShaderIrOperGpr DepthCompare  = null;

            // OperB is always the last value
            // Not applicable to 1d textures
            if (Coords.Length - CoordStartIndex != 1)
            {
                Coords[Coords.Length - CoordStartIndex -  1] = OperB;
                OperBIndex++;
            }

            // Encoding of TEXS/TLDS is a bit special and change for 2d textures
            // NOTE: OperA seems to hold at best two args.
            // On 2D textures, if no suffix need an additional values, Y is stored in OperB, otherwise coords are in OperA and the additional values is in OperB.
            if (TextureInstructionSuffix != TextureInstructionSuffix.None && TextureInstructionSuffix != TextureInstructionSuffix.LZ && TextureTarget == GalTextureTarget.TwoD)
            {
                Coords[Coords.Length - CoordStartIndex - 1]        = OpCode.Gpr8();
                Coords[Coords.Length - CoordStartIndex - 1].Index += Coords.Length - CoordStartIndex - 1;
                OperBIndex--;
            }

            // TODO: Find what MZ does and what changes about the encoding (Maybe Multisample?)
            if ((TextureInstructionSuffix & TextureInstructionSuffix.LL) != 0)
            {
                LevelOfDetail        = OpCode.Gpr20();
                LevelOfDetail.Index += OperBIndex;
                OperBIndex++;
            }

            if ((TextureInstructionSuffix & TextureInstructionSuffix.AOffI) != 0)
            {
                Offset        = OpCode.Gpr20();
                Offset.Index += OperBIndex;
                OperBIndex++;
            }

            if ((TextureInstructionSuffix & TextureInstructionSuffix.DC) != 0)
            {
                DepthCompare        = OpCode.Gpr20();
                DepthCompare.Index += OperBIndex;
                OperBIndex++;
            }

            int LutIndex;

            LutIndex  = !OpCode.Gpr0().IsConst  ? 1 : 0;
            LutIndex |= !OpCode.Gpr28().IsConst ? 2 : 0;

            if (LutIndex == 0)
            {
                //Both destination registers are RZ, do nothing.
                return;
            }

            bool Fp16 = !OpCode.Read(59);

            int DstIncrement = 0;

            ShaderIrOperGpr GetDst()
            {
                ShaderIrOperGpr Dst;

                if (Fp16)
                {
                    //FP16 mode, two components are packed on the two
                    //halfs of a 32-bits register, as two half-float values.
                    int HalfPart = DstIncrement & 1;

                    switch (LutIndex)
                    {
                        case 1: Dst = OpCode.GprHalf0(HalfPart);  break;
                        case 2: Dst = OpCode.GprHalf28(HalfPart); break;
                        case 3: Dst = (DstIncrement >> 1) != 0
                            ? OpCode.GprHalf28(HalfPart)
                            : OpCode.GprHalf0(HalfPart); break;

                        default: throw new InvalidOperationException();
                    }
                }
                else
                {
                    //32-bits mode, each component uses one register.
                    //Two components uses two consecutive registers.
                    switch (LutIndex)
                    {
                        case 1: Dst = OpCode.Gpr0();  break;
                        case 2: Dst = OpCode.Gpr28(); break;
                        case 3: Dst = (DstIncrement >> 1) != 0
                            ? OpCode.Gpr28()
                            : OpCode.Gpr0(); break;

                        default: throw new InvalidOperationException();
                    }

                    Dst.Index += DstIncrement & 1;
                }

                DstIncrement++;

                return Dst;
            }

            int ChMask = MaskLut[LutIndex, OpCode.Read(50, 7)];

            if (ChMask == 0)
            {
                //All channels are disabled, do nothing.
                return;
            }

            ShaderIrNode OperC = OpCode.Imm13_36();
            Coords = CoordsRegistersToTempRegisters(Block, Coords);

            for (int Ch = 0; Ch < 4; Ch++)
            {
                if (!IsChannelUsed(ChMask, Ch))
                {
                    continue;
                }

                ShaderIrMetaTex Meta = new ShaderIrMetaTex(Ch, TextureTarget, TextureInstructionSuffix, Coords)
                {
                    LevelOfDetail = LevelOfDetail,
                    Offset        = Offset,
                    DepthCompare  = DepthCompare
                };
                ShaderIrOp Op = new ShaderIrOp(Inst, OperA, OperB, OperC, Meta);

                ShaderIrOperGpr Dst = GetDst();

                if (Dst.IsValidRegister && !Dst.IsConst)
                {
                    Block.AddNode(OpCode.PredNode(new ShaderIrAsg(Dst, Op)));
                }
            }
        }

        private static void EmitTld4(ShaderIrBlock Block, long OpCode, GalTextureTarget TextureType, TextureInstructionSuffix TextureInstructionSuffix, int ChMask, int Component, bool Scalar)
        {
            ShaderIrOperGpr OperA = OpCode.Gpr8();
            ShaderIrOperGpr OperB = OpCode.Gpr20();
            ShaderIrOperImm OperC = OpCode.Imm13_36();

            ShaderIrOperGpr[] Coords = new ShaderIrOperGpr[ImageUtils.GetCoordsCountTextureTarget(TextureType)];

            ShaderIrOperGpr Offset       = null;
            ShaderIrOperGpr DepthCompare = null;

            bool IsArray = ImageUtils.IsArray(TextureType);

            int OperBIndex = 0;

            if (Scalar)
            {
                int CoordStartIndex = 0;

                if (IsArray)
                {
                    CoordStartIndex++;
                    Coords[Coords.Length - 1] = OperB;
                }

                switch (Coords.Length - CoordStartIndex)
                {
                    case 1:
                        Coords[0] = OpCode.Gpr8();

                        break;
                    case 2:
                        Coords[0] = OpCode.Gpr8();
                        Coords[0].Index += CoordStartIndex;

                        break;
                    case 3:
                        Coords[0] = OpCode.Gpr8();
                        Coords[0].Index += CoordStartIndex;

                        Coords[1] = OpCode.Gpr8();
                        Coords[1].Index += 1 + CoordStartIndex;

                        break;
                    default:
                        throw new NotSupportedException($"{Coords.Length - CoordStartIndex} coords textures aren't supported in TLD4S");
                }

                if (Coords.Length - CoordStartIndex != 1)
                {
                    Coords[Coords.Length - CoordStartIndex - 1] = OperB;
                    OperBIndex++;
                }

                if (TextureInstructionSuffix != TextureInstructionSuffix.None && TextureType == GalTextureTarget.TwoD)
                {
                    Coords[Coords.Length - CoordStartIndex - 1] = OpCode.Gpr8();
                    Coords[Coords.Length - CoordStartIndex - 1].Index += Coords.Length - CoordStartIndex - 1;
                    OperBIndex--;
                }
            }
            else
            {
                int IndexExtraCoord = 0;

                if (IsArray)
                {
                    IndexExtraCoord++;

                    Coords[Coords.Length - 1] = OpCode.Gpr8();
                }

                for (int Index = 0; Index < Coords.Length - IndexExtraCoord; Index++)
                {
                    Coords[Index] = OpCode.Gpr8();

                    Coords[Index].Index += Index;

                    Coords[Index].Index += IndexExtraCoord;

                    if (Coords[Index].Index > ShaderIrOperGpr.ZRIndex)
                    {
                        Coords[Index].Index = ShaderIrOperGpr.ZRIndex;
                    }
                }
            }

            if ((TextureInstructionSuffix & TextureInstructionSuffix.AOffI) != 0)
            {
                Offset = OpCode.Gpr20();
                Offset.Index += OperBIndex;
                OperBIndex++;
            }

            if ((TextureInstructionSuffix & TextureInstructionSuffix.DC) != 0)
            {
                DepthCompare = OpCode.Gpr20();
                DepthCompare.Index += OperBIndex;
                OperBIndex++;
            }

            Coords = CoordsRegistersToTempRegisters(Block, Coords);

            int RegInc = 0;

            for (int Ch = 0; Ch < 4; Ch++)
            {
                if (!IsChannelUsed(ChMask, Ch))
                {
                    continue;
                }

                ShaderIrOperGpr Dst = OpCode.Gpr0();

                Dst.Index += RegInc++;

                if (!Dst.IsValidRegister || Dst.IsConst)
                {
                    continue;
                }

                ShaderIrMetaTex Meta = new ShaderIrMetaTex(Ch, TextureType, TextureInstructionSuffix, Coords)
                {
                    Component = Component,
                    Offset = Offset,
                    DepthCompare = DepthCompare
                };

                ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Tld4, OperA, OperB, OperC, Meta);

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(Dst, Op)));
            }
        }

        private static bool IsChannelUsed(int ChMask, int Ch)
        {
            return (ChMask & (1 << Ch)) != 0;
        }

        private static ShaderIrOperGpr[] CoordsRegistersToTempRegisters(ShaderIrBlock Block, params ShaderIrOperGpr[] Registers)
        {
            ShaderIrOperGpr[] Res = new ShaderIrOperGpr[Registers.Length];

            for (int Index = 0; Index < Res.Length; Index++)
            {
                Res[Index] = ShaderIrOperGpr.MakeTemporary(Index);
                Block.AddNode(new ShaderIrAsg(Res[Index], Registers[Index]));
            }

            return Res;
        }
    }
}