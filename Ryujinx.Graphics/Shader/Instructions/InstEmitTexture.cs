using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Tex(EmitterContext context)
        {
            Tex(context, TextureFlags.None);
        }

        public static void Tex_B(EmitterContext context)
        {
            Tex(context, TextureFlags.Bindless);
        }

        public static void Tld(EmitterContext context)
        {
            Tex(context, TextureFlags.IntCoords);
        }

        public static void Tld_B(EmitterContext context)
        {
            Tex(context, TextureFlags.IntCoords | TextureFlags.Bindless);
        }

        public static void Texs(EmitterContext context)
        {
            OpCodeTextureScalar op = (OpCodeTextureScalar)context.CurrOp;

            if (op.Rd0.IsRZ && op.Rd1.IsRZ)
            {
                return;
            }

            List<Operand> sourcesList = new List<Operand>();

            int raIndex = op.Ra.Index;
            int rbIndex = op.Rb.Index;

            Operand Ra()
            {
                if (raIndex > RegisterConsts.RegisterZeroIndex)
                {
                    return Const(0);
                }

                return context.Copy(Register(raIndex++, RegisterType.Gpr));
            }

            Operand Rb()
            {
                if (rbIndex > RegisterConsts.RegisterZeroIndex)
                {
                    return Const(0);
                }

                return context.Copy(Register(rbIndex++, RegisterType.Gpr));
            }

            void AddTextureOffset(int coordsCount, int stride, int size)
            {
                Operand packedOffs = Rb();

                for (int index = 0; index < coordsCount; index++)
                {
                    sourcesList.Add(context.BitfieldExtractS32(packedOffs, Const(index * stride), Const(size)));
                }
            }

            TextureType  type;
            TextureFlags flags;

            if (op is OpCodeTexs texsOp)
            {
                type  = GetTextureType (texsOp.Type);
                flags = GetTextureFlags(texsOp.Type);

                if ((type & TextureType.Array) != 0)
                {
                    Operand arrayIndex = Ra();

                    sourcesList.Add(Ra());
                    sourcesList.Add(Rb());

                    sourcesList.Add(arrayIndex);

                    if ((type & TextureType.Shadow) != 0)
                    {
                        sourcesList.Add(Rb());
                    }

                    if ((flags & TextureFlags.LodLevel) != 0)
                    {
                        sourcesList.Add(ConstF(0));
                    }
                }
                else
                {
                    switch (texsOp.Type)
                    {
                        case TextureScalarType.Texture1DLodZero:
                            sourcesList.Add(Ra());
                            break;

                        case TextureScalarType.Texture2D:
                            sourcesList.Add(Ra());
                            sourcesList.Add(Rb());
                            break;

                        case TextureScalarType.Texture2DLodZero:
                            sourcesList.Add(Ra());
                            sourcesList.Add(Rb());
                            sourcesList.Add(ConstF(0));
                            break;

                        case TextureScalarType.Texture2DLodLevel:
                        case TextureScalarType.Texture2DDepthCompare:
                        case TextureScalarType.Texture3D:
                        case TextureScalarType.TextureCube:
                            sourcesList.Add(Ra());
                            sourcesList.Add(Ra());
                            sourcesList.Add(Rb());
                            break;

                        case TextureScalarType.Texture2DLodZeroDepthCompare:
                        case TextureScalarType.Texture3DLodZero:
                            sourcesList.Add(Ra());
                            sourcesList.Add(Ra());
                            sourcesList.Add(Rb());
                            sourcesList.Add(ConstF(0));
                            break;

                        case TextureScalarType.Texture2DLodLevelDepthCompare:
                        case TextureScalarType.TextureCubeLodLevel:
                            sourcesList.Add(Ra());
                            sourcesList.Add(Ra());
                            sourcesList.Add(Rb());
                            sourcesList.Add(Rb());
                            break;
                    }
                }
            }
            else if (op is OpCodeTlds tldsOp)
            {
                type  = GetTextureType (tldsOp.Type);
                flags = GetTextureFlags(tldsOp.Type) | TextureFlags.IntCoords;

                switch (tldsOp.Type)
                {
                    case TexelLoadScalarType.Texture1DLodZero:
                        sourcesList.Add(Ra());
                        sourcesList.Add(Const(0));
                        break;

                    case TexelLoadScalarType.Texture1DLodLevel:
                        sourcesList.Add(Ra());
                        sourcesList.Add(Rb());
                        break;

                    case TexelLoadScalarType.Texture2DLodZero:
                        sourcesList.Add(Ra());
                        sourcesList.Add(Rb());
                        sourcesList.Add(Const(0));
                        break;

                    case TexelLoadScalarType.Texture2DLodZeroOffset:
                        sourcesList.Add(Ra());
                        sourcesList.Add(Ra());
                        sourcesList.Add(Const(0));
                        break;

                    case TexelLoadScalarType.Texture2DLodZeroMultisample:
                    case TexelLoadScalarType.Texture2DLodLevel:
                    case TexelLoadScalarType.Texture2DLodLevelOffset:
                        sourcesList.Add(Ra());
                        sourcesList.Add(Ra());
                        sourcesList.Add(Rb());
                        break;

                    case TexelLoadScalarType.Texture3DLodZero:
                        sourcesList.Add(Ra());
                        sourcesList.Add(Ra());
                        sourcesList.Add(Rb());
                        sourcesList.Add(Const(0));
                        break;

                    case TexelLoadScalarType.Texture2DArrayLodZero:
                        sourcesList.Add(Rb());
                        sourcesList.Add(Rb());
                        sourcesList.Add(Ra());
                        sourcesList.Add(Const(0));
                        break;
                }

                if ((flags & TextureFlags.Offset) != 0)
                {
                    AddTextureOffset(type.GetCoordsCount(), 4, 4);
                }
            }
            else if (op is OpCodeTld4s tld4sOp)
            {
                if (!(tld4sOp.HasDepthCompare || tld4sOp.HasOffset))
                {
                    sourcesList.Add(Ra());
                    sourcesList.Add(Rb());
                }
                else
                {
                    sourcesList.Add(Ra());
                    sourcesList.Add(Ra());
                }

                type  = TextureType.Texture2D;
                flags = TextureFlags.Gather;

                if (tld4sOp.HasDepthCompare)
                {
                    sourcesList.Add(Rb());

                    type |= TextureType.Shadow;
                }

                if (tld4sOp.HasOffset)
                {
                    AddTextureOffset(type.GetCoordsCount(), 8, 6);

                    flags |= TextureFlags.Offset;
                }

                sourcesList.Add(Const(tld4sOp.GatherCompIndex));
            }
            else
            {
                throw new InvalidOperationException($"Invalid opcode type \"{op.GetType().Name}\".");
            }

            Operand[] sources = sourcesList.ToArray();

            Operand[] rd0 = new Operand[2] { ConstF(0), ConstF(0) };
            Operand[] rd1 = new Operand[2] { ConstF(0), ConstF(0) };

            int destIncrement = 0;

            Operand GetDest()
            {
                int high = destIncrement >> 1;
                int low  = destIncrement &  1;

                destIncrement++;

                if (op.IsFp16)
                {
                    return high != 0
                        ? (rd1[low] = Local())
                        : (rd0[low] = Local());
                }
                else
                {
                    int rdIndex = high != 0 ? op.Rd1.Index : op.Rd0.Index;

                    if (rdIndex < RegisterConsts.RegisterZeroIndex)
                    {
                        rdIndex += low;
                    }

                    return Register(rdIndex, RegisterType.Gpr);
                }
            }

            int handle = op.Immediate;

            for (int compMask = op.ComponentMask, compIndex = 0; compMask != 0; compMask >>= 1, compIndex++)
            {
                if ((compMask & 1) != 0)
                {
                    Operand dest = GetDest();

                    TextureOperation operation = new TextureOperation(
                        Instruction.TextureSample,
                        type,
                        flags,
                        handle,
                        compIndex,
                        dest,
                        sources);

                    context.Add(operation);
                }
            }

            if (op.IsFp16)
            {
                context.Copy(Register(op.Rd0), context.PackHalf2x16(rd0[0], rd0[1]));
                context.Copy(Register(op.Rd1), context.PackHalf2x16(rd1[0], rd1[1]));
            }
        }

        public static void Tld4(EmitterContext context)
        {
            OpCodeTld4 op = (OpCodeTld4)context.CurrOp;

            if (op.Rd.IsRZ)
            {
                return;
            }

            int raIndex = op.Ra.Index;
            int rbIndex = op.Rb.Index;

            Operand Ra()
            {
                if (raIndex > RegisterConsts.RegisterZeroIndex)
                {
                    return Const(0);
                }

                return context.Copy(Register(raIndex++, RegisterType.Gpr));
            }

            Operand Rb()
            {
                if (rbIndex > RegisterConsts.RegisterZeroIndex)
                {
                    return Const(0);
                }

                return context.Copy(Register(rbIndex++, RegisterType.Gpr));
            }

            Operand arrayIndex = op.IsArray ? Ra() : null;

            List<Operand> sourcesList = new List<Operand>();

            TextureType type = GetTextureType(op.Dimensions);

            TextureFlags flags = TextureFlags.Gather;

            int coordsCount = type.GetCoordsCount();

            for (int index = 0; index < coordsCount; index++)
            {
                sourcesList.Add(Ra());
            }

            if (op.IsArray)
            {
                sourcesList.Add(arrayIndex);

                type |= TextureType.Array;
            }

            Operand[] packedOffs = new Operand[2];

            packedOffs[0] = op.Offset != TextureGatherOffset.None    ? Rb() : null;
            packedOffs[1] = op.Offset == TextureGatherOffset.Offsets ? Rb() : null;

            if (op.HasDepthCompare)
            {
                sourcesList.Add(Rb());

                type |= TextureType.Shadow;
            }

            if (op.Offset != TextureGatherOffset.None)
            {
                int offsetTexelsCount = op.Offset == TextureGatherOffset.Offsets ? 4 : 1;

                for (int index = 0; index < coordsCount * offsetTexelsCount; index++)
                {
                    Operand packed = packedOffs[(index >> 2) & 1];

                    sourcesList.Add(context.BitfieldExtractS32(packed, Const((index & 3) * 8), Const(6)));
                }

                flags |= op.Offset == TextureGatherOffset.Offsets
                    ? TextureFlags.Offsets
                    : TextureFlags.Offset;
            }

            sourcesList.Add(Const(op.GatherCompIndex));

            Operand[] sources = sourcesList.ToArray();

            int rdIndex = op.Rd.Index;

            Operand GetDest()
            {
                if (rdIndex > RegisterConsts.RegisterZeroIndex)
                {
                    return Const(0);
                }

                return Register(rdIndex++, RegisterType.Gpr);
            }

            int handle = op.Immediate;

            for (int compMask = op.ComponentMask, compIndex = 0; compMask != 0; compMask >>= 1, compIndex++)
            {
                if ((compMask & 1) != 0)
                {
                    Operand dest = GetDest();

                    TextureOperation operation = new TextureOperation(
                        Instruction.TextureSample,
                        type,
                        flags,
                        handle,
                        compIndex,
                        dest,
                        sources);

                    context.Add(operation);
                }
            }
        }

        public static void Txq(EmitterContext context)
        {
            Txq(context, bindless: false);
        }

        public static void Txq_B(EmitterContext context)
        {
            Txq(context, bindless: true);
        }

        private static void Txq(EmitterContext context, bool bindless)
        {
            OpCodeTex op = (OpCodeTex)context.CurrOp;

            if (op.Rd.IsRZ)
            {
                return;
            }

            TextureProperty property = (TextureProperty)op.RawOpCode.Extract(22, 6);

            // TODO: Validate and use property.
            Instruction inst = Instruction.TextureSize;

            TextureType type = TextureType.Texture2D;

            TextureFlags flags = bindless ? TextureFlags.Bindless : TextureFlags.None;

            int raIndex = op.Ra.Index;

            Operand Ra()
            {
                if (raIndex > RegisterConsts.RegisterZeroIndex)
                {
                    return Const(0);
                }

                return context.Copy(Register(raIndex++, RegisterType.Gpr));
            }

            List<Operand> sourcesList = new List<Operand>();

            if (bindless)
            {
                sourcesList.Add(Ra());
            }

            sourcesList.Add(Ra());

            Operand[] sources = sourcesList.ToArray();

            int rdIndex = op.Rd.Index;

            Operand GetDest()
            {
                if (rdIndex > RegisterConsts.RegisterZeroIndex)
                {
                    return Const(0);
                }

                return Register(rdIndex++, RegisterType.Gpr);
            }

            int handle = !bindless ? op.Immediate : 0;

            for (int compMask = op.ComponentMask, compIndex = 0; compMask != 0; compMask >>= 1, compIndex++)
            {
                if ((compMask & 1) != 0)
                {
                    Operand dest = GetDest();

                    TextureOperation operation = new TextureOperation(
                        inst,
                        type,
                        flags,
                        handle,
                        compIndex,
                        dest,
                        sources);

                    context.Add(operation);
                }
            }
        }

        private static void Tex(EmitterContext context, TextureFlags flags)
        {
            OpCodeTexture op = (OpCodeTexture)context.CurrOp;

            bool isBindless = (flags & TextureFlags.Bindless)  != 0;
            bool intCoords  = (flags & TextureFlags.IntCoords) != 0;

            if (op.Rd.IsRZ)
            {
                return;
            }

            int raIndex = op.Ra.Index;
            int rbIndex = op.Rb.Index;

            Operand Ra()
            {
                if (raIndex > RegisterConsts.RegisterZeroIndex)
                {
                    return Const(0);
                }

                return context.Copy(Register(raIndex++, RegisterType.Gpr));
            }

            Operand Rb()
            {
                if (rbIndex > RegisterConsts.RegisterZeroIndex)
                {
                    return Const(0);
                }

                return context.Copy(Register(rbIndex++, RegisterType.Gpr));
            }

            Operand arrayIndex = op.IsArray ? Ra() : null;

            List<Operand> sourcesList = new List<Operand>();

            if (isBindless)
            {
                sourcesList.Add(Rb());
            }

            TextureType type = GetTextureType(op.Dimensions);

            int coordsCount = type.GetCoordsCount();

            for (int index = 0; index < coordsCount; index++)
            {
                sourcesList.Add(Ra());
            }

            if (op.IsArray)
            {
                sourcesList.Add(arrayIndex);

                type |= TextureType.Array;
            }

            bool hasLod = op.LodMode > TextureLodMode.LodZero;

            Operand lodValue = hasLod ? Rb() : ConstF(0);

            Operand packedOffs = op.HasOffset ? Rb() : null;

            if (op.HasDepthCompare)
            {
                sourcesList.Add(Rb());

                type |= TextureType.Shadow;
            }

            if ((op.LodMode == TextureLodMode.LodZero  ||
                 op.LodMode == TextureLodMode.LodLevel ||
                 op.LodMode == TextureLodMode.LodLevelA) && !op.IsMultisample)
            {
                sourcesList.Add(lodValue);

                flags |= TextureFlags.LodLevel;
            }

            if (op.HasOffset)
            {
                for (int index = 0; index < coordsCount; index++)
                {
                    sourcesList.Add(context.BitfieldExtractS32(packedOffs, Const(index * 4), Const(4)));
                }

                flags |= TextureFlags.Offset;
            }

            if (op.LodMode == TextureLodMode.LodBias ||
                op.LodMode == TextureLodMode.LodBiasA)
            {
                sourcesList.Add(lodValue);

                flags |= TextureFlags.LodBias;
            }

            if (op.IsMultisample)
            {
                sourcesList.Add(Rb());

                type |= TextureType.Multisample;
            }

            Operand[] sources = sourcesList.ToArray();

            int rdIndex = op.Rd.Index;

            Operand GetDest()
            {
                if (rdIndex > RegisterConsts.RegisterZeroIndex)
                {
                    return Const(0);
                }

                return Register(rdIndex++, RegisterType.Gpr);
            }

            int handle = !isBindless ? op.Immediate : 0;

            for (int compMask = op.ComponentMask, compIndex = 0; compMask != 0; compMask >>= 1, compIndex++)
            {
                if ((compMask & 1) != 0)
                {
                    Operand dest = GetDest();

                    TextureOperation operation = new TextureOperation(
                        Instruction.TextureSample,
                        type,
                        flags,
                        handle,
                        compIndex,
                        dest,
                        sources);

                    context.Add(operation);
                }
            }
        }

        private static TextureType GetTextureType(TextureDimensions dimensions)
        {
            switch (dimensions)
            {
                case TextureDimensions.Texture1D:   return TextureType.Texture1D;
                case TextureDimensions.Texture2D:   return TextureType.Texture2D;
                case TextureDimensions.Texture3D:   return TextureType.Texture3D;
                case TextureDimensions.TextureCube: return TextureType.TextureCube;
            }

            throw new ArgumentException($"Invalid texture dimensions \"{dimensions}\".");
        }

        private static TextureType GetTextureType(TextureScalarType type)
        {
            switch (type)
            {
                case TextureScalarType.Texture1DLodZero:
                    return TextureType.Texture1D;

                case TextureScalarType.Texture2D:
                case TextureScalarType.Texture2DLodZero:
                case TextureScalarType.Texture2DLodLevel:
                    return TextureType.Texture2D;

                case TextureScalarType.Texture2DDepthCompare:
                case TextureScalarType.Texture2DLodLevelDepthCompare:
                case TextureScalarType.Texture2DLodZeroDepthCompare:
                    return TextureType.Texture2D | TextureType.Shadow;

                case TextureScalarType.Texture2DArray:
                case TextureScalarType.Texture2DArrayLodZero:
                    return TextureType.Texture2D | TextureType.Array;

                case TextureScalarType.Texture2DArrayLodZeroDepthCompare:
                    return TextureType.Texture2D | TextureType.Array | TextureType.Shadow;

                case TextureScalarType.Texture3D:
                case TextureScalarType.Texture3DLodZero:
                    return TextureType.Texture3D;

                case TextureScalarType.TextureCube:
                case TextureScalarType.TextureCubeLodLevel:
                    return TextureType.TextureCube;
            }

            throw new ArgumentException($"Invalid texture type \"{type}\".");
        }

        private static TextureType GetTextureType(TexelLoadScalarType type)
        {
            switch (type)
            {
                case TexelLoadScalarType.Texture1DLodZero:
                case TexelLoadScalarType.Texture1DLodLevel:
                    return TextureType.Texture1D;

                case TexelLoadScalarType.Texture2DLodZero:
                case TexelLoadScalarType.Texture2DLodZeroOffset:
                case TexelLoadScalarType.Texture2DLodLevel:
                case TexelLoadScalarType.Texture2DLodLevelOffset:
                    return TextureType.Texture2D;

                case TexelLoadScalarType.Texture2DLodZeroMultisample:
                    return TextureType.Texture2D | TextureType.Multisample;

                case TexelLoadScalarType.Texture3DLodZero:
                    return TextureType.Texture3D;

                case TexelLoadScalarType.Texture2DArrayLodZero:
                    return TextureType.Texture2D | TextureType.Array;
            }

            throw new ArgumentException($"Invalid texture type \"{type}\".");
        }

        private static TextureFlags GetTextureFlags(TextureScalarType type)
        {
            switch (type)
            {
                case TextureScalarType.Texture1DLodZero:
                case TextureScalarType.Texture2DLodZero:
                case TextureScalarType.Texture2DLodLevel:
                case TextureScalarType.Texture2DLodLevelDepthCompare:
                case TextureScalarType.Texture2DLodZeroDepthCompare:
                case TextureScalarType.Texture2DArrayLodZero:
                case TextureScalarType.Texture2DArrayLodZeroDepthCompare:
                case TextureScalarType.Texture3DLodZero:
                case TextureScalarType.TextureCubeLodLevel:
                    return TextureFlags.LodLevel;

                case TextureScalarType.Texture2D:
                case TextureScalarType.Texture2DDepthCompare:
                case TextureScalarType.Texture2DArray:
                case TextureScalarType.Texture3D:
                case TextureScalarType.TextureCube:
                    return TextureFlags.None;
            }

            throw new ArgumentException($"Invalid texture type \"{type}\".");
        }

        private static TextureFlags GetTextureFlags(TexelLoadScalarType type)
        {
            switch (type)
            {
                case TexelLoadScalarType.Texture1DLodZero:
                case TexelLoadScalarType.Texture1DLodLevel:
                case TexelLoadScalarType.Texture2DLodZero:
                case TexelLoadScalarType.Texture2DLodLevel:
                case TexelLoadScalarType.Texture2DLodZeroMultisample:
                case TexelLoadScalarType.Texture3DLodZero:
                case TexelLoadScalarType.Texture2DArrayLodZero:
                    return TextureFlags.LodLevel;

                case TexelLoadScalarType.Texture2DLodZeroOffset:
                case TexelLoadScalarType.Texture2DLodLevelOffset:
                    return TextureFlags.LodLevel | TextureFlags.Offset;
            }

            throw new ArgumentException($"Invalid texture type \"{type}\".");
        }
    }
}