using Ryujinx.Graphics.Texture;
using System;

using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        // ReSharper disable InconsistentNaming
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
        // ReSharper restore InconsistentNaming

        private static int[,] _maskLut = new int[,]
        {
            { ____, ____, ____, ____, ____, ____, ____, ____ },
            { R___, _G__, __B_, ___A, RG__, R__A, _G_A, __BA },
            { R___, _G__, __B_, ___A, RG__, ____, ____, ____ },
            { RGB_, RG_A, R_BA, _GBA, RGBA, ____, ____, ____ }
        };

        private static GalTextureTarget TexToTextureTarget(int texType, bool isArray)
        {
            switch (texType)
            {
                case 0:
                    return isArray ? GalTextureTarget.OneDArray : GalTextureTarget.OneD;
                case 2:
                    return isArray ? GalTextureTarget.TwoDArray : GalTextureTarget.TwoD;
                case 4:
                    if (isArray)
                        throw new InvalidOperationException("ARRAY bit set on a TEX with 3D texture!");
                    return GalTextureTarget.ThreeD;
                case 6:
                    return isArray ? GalTextureTarget.CubeArray : GalTextureTarget.CubeMap;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static GalTextureTarget TexsToTextureTarget(int texType)
        {
            switch (texType)
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

        public static GalTextureTarget TldsToTextureTarget(int texType)
        {
            switch (texType)
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

        public static void Ld_A(ShaderIrBlock block, long opCode, int position)
        {
            ShaderIrNode[] opers = opCode.Abuf20();

            //Used by GS
            ShaderIrOperGpr vertex = opCode.Gpr39();

            int index = 0;

            foreach (ShaderIrNode operA in opers)
            {
                ShaderIrOperGpr operD = opCode.Gpr0();

                operD.Index += index++;

                block.AddNode(opCode.PredNode(new ShaderIrAsg(operD, operA)));
            }
        }

        public static void Ld_C(ShaderIrBlock block, long opCode, int position)
        {
            int cbufPos   = opCode.Read(22, 0x3fff);
            int cbufIndex = opCode.Read(36, 0x1f);
            int type      = opCode.Read(48, 7);

            if (type > 5)
            {
                throw new InvalidOperationException();
            }

            ShaderIrOperGpr temp = ShaderIrOperGpr.MakeTemporary();

            block.AddNode(new ShaderIrAsg(temp, opCode.Gpr8()));

            int count = type == 5 ? 2 : 1;

            for (int index = 0; index < count; index++)
            {
                ShaderIrOperCbuf operA = new ShaderIrOperCbuf(cbufIndex, cbufPos, temp);

                ShaderIrOperGpr operD = opCode.Gpr0();

                operA.Pos   += index;
                operD.Index += index;

                if (!operD.IsValidRegister)
                {
                    break;
                }

                ShaderIrNode node = operA;

                if (type < 4)
                {
                    //This is a 8 or 16 bits type.
                    bool signed = (type & 1) != 0;

                    int size = 8 << (type >> 1);

                    node = ExtendTo32(node, signed, size);
                }

                block.AddNode(opCode.PredNode(new ShaderIrAsg(operD, node)));
            }
        }

        public static void St_A(ShaderIrBlock block, long opCode, int position)
        {
            ShaderIrNode[] opers = opCode.Abuf20();

            int index = 0;

            foreach (ShaderIrNode operA in opers)
            {
                ShaderIrOperGpr operD = opCode.Gpr0();

                operD.Index += index++;

                block.AddNode(opCode.PredNode(new ShaderIrAsg(operA, operD)));
            }
        }

        public static void Texq(ShaderIrBlock block, long opCode, int position)
        {
            ShaderIrNode operD = opCode.Gpr0();
            ShaderIrNode operA = opCode.Gpr8();

            ShaderTexqInfo info = (ShaderTexqInfo)(opCode.Read(22, 0x1f));

            ShaderIrMetaTexq meta0 = new ShaderIrMetaTexq(info, 0);
            ShaderIrMetaTexq meta1 = new ShaderIrMetaTexq(info, 1);

            ShaderIrNode operC = opCode.Imm13_36();

            ShaderIrOp op0 = new ShaderIrOp(ShaderIrInst.Texq, operA, null, operC, meta0);
            ShaderIrOp op1 = new ShaderIrOp(ShaderIrInst.Texq, operA, null, operC, meta1);

            block.AddNode(opCode.PredNode(new ShaderIrAsg(operD, op0)));
            block.AddNode(opCode.PredNode(new ShaderIrAsg(operA, op1))); //Is this right?
        }

        public static void Tex(ShaderIrBlock block, long opCode, int position)
        {
            TextureInstructionSuffix suffix;

            int rawSuffix = opCode.Read(0x34, 0x38);

            switch (rawSuffix)
            {
                case 0:
                    suffix = TextureInstructionSuffix.None;
                    break;
                case 0x8:
                    suffix = TextureInstructionSuffix.Lz;
                    break;
                case 0x10:
                    suffix = TextureInstructionSuffix.Lb;
                    break;
                case 0x18:
                    suffix = TextureInstructionSuffix.Ll;
                    break;
                case 0x30:
                    suffix = TextureInstructionSuffix.Lba;
                    break;
                case 0x38:
                    suffix = TextureInstructionSuffix.Lla;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Suffix for TEX instruction {rawSuffix}");
            }

            bool isOffset = opCode.Read(0x36);

            if (isOffset)
                suffix |= TextureInstructionSuffix.AOffI;

            EmitTex(block, opCode, suffix, gprHandle: false);
        }

        public static void Tex_B(ShaderIrBlock block, long opCode, int position)
        {
            TextureInstructionSuffix suffix;

            int rawSuffix = opCode.Read(0x24, 0xe);

            switch (rawSuffix)
            {
                case 0:
                    suffix = TextureInstructionSuffix.None;
                    break;
                case 0x2:
                    suffix = TextureInstructionSuffix.Lz;
                    break;
                case 0x4:
                    suffix = TextureInstructionSuffix.Lb;
                    break;
                case 0x6:
                    suffix = TextureInstructionSuffix.Ll;
                    break;
                case 0xc:
                    suffix = TextureInstructionSuffix.Lba;
                    break;
                case 0xe:
                    suffix = TextureInstructionSuffix.Lla;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Suffix for TEX.B instruction {rawSuffix}");
            }

            bool isOffset = opCode.Read(0x23);

            if (isOffset)
                suffix |= TextureInstructionSuffix.AOffI;

            EmitTex(block, opCode, suffix, gprHandle: true);
        }

        private static void EmitTex(ShaderIrBlock block, long opCode, TextureInstructionSuffix textureInstructionSuffix, bool gprHandle)
        {
            bool isArray = opCode.HasArray();

            GalTextureTarget textureTarget = TexToTextureTarget(opCode.Read(28, 6), isArray);

            bool hasDepthCompare = opCode.Read(0x32);

            if (hasDepthCompare)
            {
                textureInstructionSuffix |= TextureInstructionSuffix.Dc;
            }

            ShaderIrOperGpr[] coords = new ShaderIrOperGpr[ImageUtils.GetCoordsCountTextureTarget(textureTarget)];

            int indexExtraCoord = 0;

            if (isArray)
            {
                indexExtraCoord++;

                coords[coords.Length - 1] = opCode.Gpr8();
            }


            for (int index = 0; index < coords.Length - indexExtraCoord; index++)
            {
                ShaderIrOperGpr coordReg = opCode.Gpr8();

                coordReg.Index += index;

                coordReg.Index += indexExtraCoord;

                if (!coordReg.IsValidRegister)
                {
                    coordReg.Index = ShaderIrOperGpr.ZrIndex;
                }

                coords[index] = coordReg;
            }

            int chMask = opCode.Read(31, 0xf);

            ShaderIrOperGpr levelOfDetail = null;
            ShaderIrOperGpr offset        = null;
            ShaderIrOperGpr depthCompare  = null;

            // TODO: determine first argument when TEX.B is used
            int operBIndex = gprHandle ? 1 : 0;

            if ((textureInstructionSuffix & TextureInstructionSuffix.Ll) != 0 ||
                (textureInstructionSuffix & TextureInstructionSuffix.Lb) != 0 ||
                (textureInstructionSuffix & TextureInstructionSuffix.Lba) != 0 ||
                (textureInstructionSuffix & TextureInstructionSuffix.Lla) != 0)
            {
                levelOfDetail        = opCode.Gpr20();
                levelOfDetail.Index += operBIndex;

                operBIndex++;
            }

            if ((textureInstructionSuffix & TextureInstructionSuffix.AOffI) != 0)
            {
                offset        = opCode.Gpr20();
                offset.Index += operBIndex;

                operBIndex++;
            }

            if ((textureInstructionSuffix & TextureInstructionSuffix.Dc) != 0)
            {
                depthCompare        = opCode.Gpr20();
                depthCompare.Index += operBIndex;

                operBIndex++;
            }

            // ???
            ShaderIrNode operC = gprHandle
                ? (ShaderIrNode)opCode.Gpr20()
                : (ShaderIrNode)opCode.Imm13_36();

            ShaderIrInst inst = gprHandle ? ShaderIrInst.Texb : ShaderIrInst.Texs;

            coords = CoordsRegistersToTempRegisters(block, coords);

            int regInc = 0;

            for (int ch = 0; ch < 4; ch++)
            {
                if (!IsChannelUsed(chMask, ch))
                {
                    continue;
                }

                ShaderIrOperGpr dst = opCode.Gpr0();

                dst.Index += regInc++;

                if (!dst.IsValidRegister || dst.IsConst)
                {
                    continue;
                }

                ShaderIrMetaTex meta = new ShaderIrMetaTex(ch, textureTarget, textureInstructionSuffix, coords)
                {
                    LevelOfDetail = levelOfDetail,
                    Offset        = offset,
                    DepthCompare  = depthCompare
                };

                ShaderIrOp op = new ShaderIrOp(inst, coords[0], coords.Length > 1 ? coords[1] : null, operC, meta);

                block.AddNode(opCode.PredNode(new ShaderIrAsg(dst, op)));
            }
        }

        public static void Texs(ShaderIrBlock block, long opCode, int position)
        {
            TextureInstructionSuffix suffix;

            int rawSuffix = opCode.Read(0x34, 0x1e);

            switch (rawSuffix)
            {
                case 0:
                case 0x4:
                case 0x10:
                case 0x16:
                    suffix = TextureInstructionSuffix.Lz;
                    break;
                case 0x6:
                case 0x1a:
                    suffix = TextureInstructionSuffix.Ll;
                    break;
                case 0x8:
                    suffix = TextureInstructionSuffix.Dc;
                    break;
                case 0x2:
                case 0xe:
                case 0x14:
                case 0x18:
                    suffix = TextureInstructionSuffix.None;
                    break;
                case 0xa:
                    suffix = TextureInstructionSuffix.Ll | TextureInstructionSuffix.Dc;
                    break;
                case 0xc:
                case 0x12:
                    suffix = TextureInstructionSuffix.Lz | TextureInstructionSuffix.Dc;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Suffix for TEXS instruction {rawSuffix}");
            }

            GalTextureTarget textureTarget = TexsToTextureTarget(opCode.Read(52, 0x1e));

            EmitTexs(block, opCode, ShaderIrInst.Texs, textureTarget, suffix);
        }

        public static void Tlds(ShaderIrBlock block, long opCode, int position)
        {
            TextureInstructionSuffix suffix;

            int rawSuffix = opCode.Read(0x34, 0x1e);

            switch (rawSuffix)
            {
                case 0:
                case 0x4:
                case 0x8:
                    suffix = TextureInstructionSuffix.Lz | TextureInstructionSuffix.AOffI;
                    break;
                case 0xc:
                    suffix = TextureInstructionSuffix.Lz | TextureInstructionSuffix.Mz;
                    break;
                case 0xe:
                case 0x10:
                    suffix = TextureInstructionSuffix.Lz;
                    break;
                case 0x2:
                case 0xa:
                    suffix = TextureInstructionSuffix.Ll;
                    break;
                case 0x18:
                    suffix = TextureInstructionSuffix.Ll | TextureInstructionSuffix.AOffI;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Suffix for TLDS instruction {rawSuffix}");
            }

            GalTextureTarget textureTarget = TldsToTextureTarget(opCode.Read(52, 0x1e));

            EmitTexs(block, opCode, ShaderIrInst.Txlf, textureTarget, suffix);
        }

        public static void Tld4(ShaderIrBlock block, long opCode, int position)
        {
            TextureInstructionSuffix suffix;

            int rawSuffix = opCode.Read(0x34, 0xc);

            switch (rawSuffix)
            {
                case 0:
                    suffix = TextureInstructionSuffix.None;
                    break;
                case 0x4:
                    suffix = TextureInstructionSuffix.AOffI;
                    break;
                case 0x8:
                    suffix = TextureInstructionSuffix.Ptp;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Suffix for TLD4 instruction {rawSuffix}");
            }

            bool isShadow = opCode.Read(0x32);

            bool isArray = opCode.HasArray();
            int  chMask  = opCode.Read(31, 0xf);

            GalTextureTarget textureTarget = TexToTextureTarget(opCode.Read(28, 6), isArray);

            if (isShadow)
            {
                suffix |= TextureInstructionSuffix.Dc;
            }

            EmitTld4(block, opCode, textureTarget, suffix, chMask, opCode.Read(0x38, 0x3), false);
        }

        public static void Tld4S(ShaderIrBlock block, long opCode, int position)
        {
            TextureInstructionSuffix suffix = TextureInstructionSuffix.None;

            bool isOffset = opCode.Read(0x33);
            bool isShadow = opCode.Read(0x32);

            if (isOffset)
            {
                suffix |= TextureInstructionSuffix.AOffI;
            }

            if (isShadow)
            {
                suffix |= TextureInstructionSuffix.Dc;
            }

            // TLD4S seems to only support 2D textures with RGBA mask?
            EmitTld4(block, opCode, GalTextureTarget.TwoD, suffix, RGBA, opCode.Read(0x34, 0x3), true);
        }

        private static void EmitTexs(ShaderIrBlock            block,
                                     long                     opCode,
                                     ShaderIrInst             inst,
                                     GalTextureTarget         textureTarget,
                                     TextureInstructionSuffix textureInstructionSuffix)
        {
            if (inst == ShaderIrInst.Txlf && textureTarget == GalTextureTarget.CubeArray)
            {
                throw new InvalidOperationException("TLDS instructions cannot use CUBE modifier!");
            }

            bool isArray = ImageUtils.IsArray(textureTarget);

            ShaderIrOperGpr[] coords = new ShaderIrOperGpr[ImageUtils.GetCoordsCountTextureTarget(textureTarget)];

            ShaderIrOperGpr operA = opCode.Gpr8();
            ShaderIrOperGpr operB = opCode.Gpr20();

            ShaderIrOperGpr suffixExtra = opCode.Gpr20();
            suffixExtra.Index += 1;

            int coordStartIndex = 0;

            if (isArray)
            {
                coordStartIndex++;
                coords[coords.Length - 1] = opCode.Gpr8();
            }

            switch (coords.Length - coordStartIndex)
            {
                case 1:
                    coords[0] = opCode.Gpr8();

                    break;
                case 2:
                    coords[0] = opCode.Gpr8();
                    coords[0].Index += coordStartIndex;

                    break;
                case 3:
                    coords[0] = opCode.Gpr8();
                    coords[0].Index += coordStartIndex;

                    coords[1] = opCode.Gpr8();
                    coords[1].Index += 1 + coordStartIndex;

                    break;
                default:
                    throw new NotSupportedException($"{coords.Length - coordStartIndex} coords textures aren't supported in TEXS");
            }

            int operBIndex = 0;

            ShaderIrOperGpr levelOfDetail = null;
            ShaderIrOperGpr offset        = null;
            ShaderIrOperGpr depthCompare  = null;

            // OperB is always the last value
            // Not applicable to 1d textures
            if (coords.Length - coordStartIndex != 1)
            {
                coords[coords.Length - coordStartIndex -  1] = operB;
                operBIndex++;
            }

            // Encoding of TEXS/TLDS is a bit special and change for 2d textures
            // NOTE: OperA seems to hold at best two args.
            // On 2D textures, if no suffix need an additional values, Y is stored in OperB, otherwise coords are in OperA and the additional values is in OperB.
            if (textureInstructionSuffix != TextureInstructionSuffix.None && textureInstructionSuffix != TextureInstructionSuffix.Lz && textureTarget == GalTextureTarget.TwoD)
            {
                coords[coords.Length - coordStartIndex - 1]        = opCode.Gpr8();
                coords[coords.Length - coordStartIndex - 1].Index += coords.Length - coordStartIndex - 1;
                operBIndex--;
            }

            // TODO: Find what MZ does and what changes about the encoding (Maybe Multisample?)
            if ((textureInstructionSuffix & TextureInstructionSuffix.Ll) != 0)
            {
                levelOfDetail        = opCode.Gpr20();
                levelOfDetail.Index += operBIndex;
                operBIndex++;
            }

            if ((textureInstructionSuffix & TextureInstructionSuffix.AOffI) != 0)
            {
                offset        = opCode.Gpr20();
                offset.Index += operBIndex;
                operBIndex++;
            }

            if ((textureInstructionSuffix & TextureInstructionSuffix.Dc) != 0)
            {
                depthCompare        = opCode.Gpr20();
                depthCompare.Index += operBIndex;
                operBIndex++;
            }

            int lutIndex;

            lutIndex  = !opCode.Gpr0().IsConst  ? 1 : 0;
            lutIndex |= !opCode.Gpr28().IsConst ? 2 : 0;

            if (lutIndex == 0)
            {
                //Both destination registers are RZ, do nothing.
                return;
            }

            bool fp16 = !opCode.Read(59);

            int dstIncrement = 0;

            ShaderIrOperGpr GetDst()
            {
                ShaderIrOperGpr dst;

                if (fp16)
                {
                    //FP16 mode, two components are packed on the two
                    //halfs of a 32-bits register, as two half-float values.
                    int halfPart = dstIncrement & 1;

                    switch (lutIndex)
                    {
                        case 1: dst = opCode.GprHalf0(halfPart);  break;
                        case 2: dst = opCode.GprHalf28(halfPart); break;
                        case 3: dst = (dstIncrement >> 1) != 0
                            ? opCode.GprHalf28(halfPart)
                            : opCode.GprHalf0(halfPart); break;

                        default: throw new InvalidOperationException();
                    }
                }
                else
                {
                    //32-bits mode, each component uses one register.
                    //Two components uses two consecutive registers.
                    switch (lutIndex)
                    {
                        case 1: dst = opCode.Gpr0();  break;
                        case 2: dst = opCode.Gpr28(); break;
                        case 3: dst = (dstIncrement >> 1) != 0
                            ? opCode.Gpr28()
                            : opCode.Gpr0(); break;

                        default: throw new InvalidOperationException();
                    }

                    dst.Index += dstIncrement & 1;
                }

                dstIncrement++;

                return dst;
            }

            int chMask = _maskLut[lutIndex, opCode.Read(50, 7)];

            if (chMask == 0)
            {
                //All channels are disabled, do nothing.
                return;
            }

            ShaderIrNode operC = opCode.Imm13_36();
            coords = CoordsRegistersToTempRegisters(block, coords);

            for (int ch = 0; ch < 4; ch++)
            {
                if (!IsChannelUsed(chMask, ch))
                {
                    continue;
                }

                ShaderIrMetaTex meta = new ShaderIrMetaTex(ch, textureTarget, textureInstructionSuffix, coords)
                {
                    LevelOfDetail = levelOfDetail,
                    Offset        = offset,
                    DepthCompare  = depthCompare
                };
                ShaderIrOp op = new ShaderIrOp(inst, operA, operB, operC, meta);

                ShaderIrOperGpr dst = GetDst();

                if (dst.IsValidRegister && !dst.IsConst)
                {
                    block.AddNode(opCode.PredNode(new ShaderIrAsg(dst, op)));
                }
            }
        }

        private static void EmitTld4(ShaderIrBlock block, long opCode, GalTextureTarget textureType, TextureInstructionSuffix textureInstructionSuffix, int chMask, int component, bool scalar)
        {
            ShaderIrOperGpr operA = opCode.Gpr8();
            ShaderIrOperGpr operB = opCode.Gpr20();
            ShaderIrOperImm operC = opCode.Imm13_36();

            ShaderIrOperGpr[] coords = new ShaderIrOperGpr[ImageUtils.GetCoordsCountTextureTarget(textureType)];

            ShaderIrOperGpr offset       = null;
            ShaderIrOperGpr depthCompare = null;

            bool isArray = ImageUtils.IsArray(textureType);

            int operBIndex = 0;

            if (scalar)
            {
                int coordStartIndex = 0;

                if (isArray)
                {
                    coordStartIndex++;
                    coords[coords.Length - 1] = operB;
                }

                switch (coords.Length - coordStartIndex)
                {
                    case 1:
                        coords[0] = opCode.Gpr8();

                        break;
                    case 2:
                        coords[0] = opCode.Gpr8();
                        coords[0].Index += coordStartIndex;

                        break;
                    case 3:
                        coords[0] = opCode.Gpr8();
                        coords[0].Index += coordStartIndex;

                        coords[1] = opCode.Gpr8();
                        coords[1].Index += 1 + coordStartIndex;

                        break;
                    default:
                        throw new NotSupportedException($"{coords.Length - coordStartIndex} coords textures aren't supported in TLD4S");
                }

                if (coords.Length - coordStartIndex != 1)
                {
                    coords[coords.Length - coordStartIndex - 1] = operB;
                    operBIndex++;
                }

                if (textureInstructionSuffix != TextureInstructionSuffix.None && textureType == GalTextureTarget.TwoD)
                {
                    coords[coords.Length - coordStartIndex - 1] = opCode.Gpr8();
                    coords[coords.Length - coordStartIndex - 1].Index += coords.Length - coordStartIndex - 1;
                    operBIndex--;
                }
            }
            else
            {
                int indexExtraCoord = 0;

                if (isArray)
                {
                    indexExtraCoord++;

                    coords[coords.Length - 1] = opCode.Gpr8();
                }

                for (int index = 0; index < coords.Length - indexExtraCoord; index++)
                {
                    coords[index] = opCode.Gpr8();

                    coords[index].Index += index;

                    coords[index].Index += indexExtraCoord;

                    if (coords[index].Index > ShaderIrOperGpr.ZrIndex)
                    {
                        coords[index].Index = ShaderIrOperGpr.ZrIndex;
                    }
                }
            }

            if ((textureInstructionSuffix & TextureInstructionSuffix.AOffI) != 0)
            {
                offset = opCode.Gpr20();
                offset.Index += operBIndex;
                operBIndex++;
            }

            if ((textureInstructionSuffix & TextureInstructionSuffix.Dc) != 0)
            {
                depthCompare = opCode.Gpr20();
                depthCompare.Index += operBIndex;
                operBIndex++;
            }

            coords = CoordsRegistersToTempRegisters(block, coords);

            int regInc = 0;

            for (int ch = 0; ch < 4; ch++)
            {
                if (!IsChannelUsed(chMask, ch))
                {
                    continue;
                }

                ShaderIrOperGpr dst = opCode.Gpr0();

                dst.Index += regInc++;

                if (!dst.IsValidRegister || dst.IsConst)
                {
                    continue;
                }

                ShaderIrMetaTex meta = new ShaderIrMetaTex(ch, textureType, textureInstructionSuffix, coords)
                {
                    Component = component,
                    Offset = offset,
                    DepthCompare = depthCompare
                };

                ShaderIrOp op = new ShaderIrOp(ShaderIrInst.Tld4, operA, operB, operC, meta);

                block.AddNode(opCode.PredNode(new ShaderIrAsg(dst, op)));
            }
        }

        private static bool IsChannelUsed(int chMask, int ch)
        {
            return (chMask & (1 << ch)) != 0;
        }

        private static ShaderIrOperGpr[] CoordsRegistersToTempRegisters(ShaderIrBlock block, params ShaderIrOperGpr[] registers)
        {
            ShaderIrOperGpr[] res = new ShaderIrOperGpr[registers.Length];

            for (int index = 0; index < res.Length; index++)
            {
                res[index] = ShaderIrOperGpr.MakeTemporary(index);
                block.AddNode(new ShaderIrAsg(res[index], registers[index]));
            }

            return res;
        }
    }
}