using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static class InstEmitHelper
    {
        public static Operand GetZF()
        {
            return Register(0, RegisterType.Flag);
        }

        public static Operand GetNF()
        {
            return Register(1, RegisterType.Flag);
        }

        public static Operand GetCF()
        {
            return Register(2, RegisterType.Flag);
        }

        public static Operand GetVF()
        {
            return Register(3, RegisterType.Flag);
        }

        public static Operand GetDest(EmitterContext context)
        {
            return Register(((IOpCodeRd)context.CurrOp).Rd);
        }

        public static Operand GetDest2(EmitterContext context)
        {
            Register rd = ((IOpCodeRd)context.CurrOp).Rd;

            return Register(rd.Index | 1, rd.Type);
        }

        public static Operand GetSrcA(EmitterContext context, bool isFP64 = false)
        {
            IOpCodeRa op = (IOpCodeRa)context.CurrOp;

            if (isFP64)
            {
                return context.PackDouble2x32(Register(op.Ra.Index, op.Ra.Type), Register(op.Ra.Index | 1, op.Ra.Type));
            }
            else
            {
                return Register(op.Ra);
            }
        }

        public static Operand GetSrcB(EmitterContext context, FPType floatType)
        {
            if (floatType == FPType.FP32)
            {
                return GetSrcB(context);
            }
            else if (floatType == FPType.FP16)
            {
                int h = context.CurrOp.RawOpCode.Extract(41, 1);

                return GetHalfUnpacked(context, GetSrcB(context), FPHalfSwizzle.FP16)[h];
            }
            else if (floatType == FPType.FP64)
            {
                return GetSrcB(context, true);
            }

            throw new ArgumentException($"Invalid floating point type \"{floatType}\".");
        }

        public static Operand GetSrcB(EmitterContext context, bool isFP64 = false)
        {
            if (isFP64)
            {
                switch (context.CurrOp)
                {
                    case IOpCodeCbuf op:
                        return context.PackDouble2x32(Cbuf(op.Slot, op.Offset), Cbuf(op.Slot, op.Offset + 1));

                    case IOpCodeImmF op:
                        return context.FP32ConvertToFP64(ConstF(op.Immediate));

                    case IOpCodeReg op:
                        return context.PackDouble2x32(Register(op.Rb.Index, op.Rb.Type), Register(op.Rb.Index | 1, op.Rb.Type));

                    case IOpCodeRegCbuf op:
                        return context.PackDouble2x32(Register(op.Rc.Index, op.Rc.Type), Register(op.Rc.Index | 1, op.Rc.Type));
                }
            }
            else
            {
                switch (context.CurrOp)
                {
                    case IOpCodeCbuf op:
                        return Cbuf(op.Slot, op.Offset);

                    case IOpCodeImm op:
                        return Const(op.Immediate);

                    case IOpCodeImmF op:
                        return ConstF(op.Immediate);

                    case IOpCodeReg op:
                        return Register(op.Rb);

                    case IOpCodeRegCbuf op:
                        return Register(op.Rc);
                }
            }

            throw new InvalidOperationException($"Unexpected opcode type \"{context.CurrOp.GetType().Name}\".");
        }

        public static Operand GetSrcC(EmitterContext context, bool isFP64 = false)
        {
            if (isFP64)
            {
                switch (context.CurrOp)
                {
                    case IOpCodeRegCbuf op:
                        return context.PackDouble2x32(Cbuf(op.Slot, op.Offset), Cbuf(op.Slot, op.Offset + 1));

                    case IOpCodeRc op:
                        return context.PackDouble2x32(Register(op.Rc.Index, op.Rc.Type), Register(op.Rc.Index | 1, op.Rc.Type));
                }
            }
            else
            {
                switch (context.CurrOp)
                {
                    case IOpCodeRegCbuf op:
                        return Cbuf(op.Slot, op.Offset);

                    case IOpCodeRc op:
                        return Register(op.Rc);
                }
            }

            throw new InvalidOperationException($"Unexpected opcode type \"{context.CurrOp.GetType().Name}\".");
        }

        public static Operand[] GetHalfSrcA(EmitterContext context, bool isAdd = false)
        {
            OpCode op = context.CurrOp;

            bool absoluteA = false, negateA = false;

            if (op is OpCodeAluImm32 && isAdd)
            {
                negateA = op.RawOpCode.Extract(56);
            }
            else if (isAdd || op is IOpCodeCbuf || op is IOpCodeImm)
            {
                negateA   = op.RawOpCode.Extract(43);
                absoluteA = op.RawOpCode.Extract(44);
            }
            else if (op is IOpCodeReg)
            {
                absoluteA = op.RawOpCode.Extract(44);
            }

            FPHalfSwizzle swizzle = (FPHalfSwizzle)op.RawOpCode.Extract(47, 2);

            Operand[] operands = GetHalfUnpacked(context, GetSrcA(context), swizzle);

            return FPAbsNeg(context, operands, absoluteA, negateA);
        }

        public static Operand[] GetHalfSrcB(EmitterContext context, bool isMul = false)
        {
            OpCode op = context.CurrOp;

            FPHalfSwizzle swizzle = FPHalfSwizzle.FP16;

            bool absoluteB = false, negateB = false;

            if (op is IOpCodeReg)
            {
                swizzle = (FPHalfSwizzle)op.RawOpCode.Extract(28, 2);

                absoluteB = op.RawOpCode.Extract(30);
                negateB   = op.RawOpCode.Extract(31);
            }
            else if (op is IOpCodeCbuf)
            {
                swizzle = FPHalfSwizzle.FP32;

                absoluteB = op.RawOpCode.Extract(54);

                if (!isMul)
                {
                    negateB = op.RawOpCode.Extract(56);
                }
            }

            Operand[] operands = GetHalfUnpacked(context, GetSrcB(context), swizzle);

            return FPAbsNeg(context, operands, absoluteB, negateB);
        }

        public static Operand[] FPAbsNeg(EmitterContext context, Operand[] operands, bool abs, bool neg)
        {
            for (int index = 0; index < operands.Length; index++)
            {
                operands[index] = context.FPAbsNeg(operands[index], abs, neg);
            }

            return operands;
        }

        public static Operand[] GetHalfUnpacked(EmitterContext context, Operand src, FPHalfSwizzle swizzle)
        {
            switch (swizzle)
            {
                case FPHalfSwizzle.FP16:
                    return new Operand[]
                    {
                        context.UnpackHalf2x16Low (src),
                        context.UnpackHalf2x16High(src)
                    };

                case FPHalfSwizzle.FP32: return new Operand[] { src, src };

                case FPHalfSwizzle.DupH0:
                    return new Operand[]
                    {
                        context.UnpackHalf2x16Low(src),
                        context.UnpackHalf2x16Low(src)
                    };

                case FPHalfSwizzle.DupH1:
                    return new Operand[]
                    {
                        context.UnpackHalf2x16High(src),
                        context.UnpackHalf2x16High(src)
                    };
            }

            throw new ArgumentException($"Invalid swizzle \"{swizzle}\".");
        }

        public static Operand GetHalfPacked(EmitterContext context, Operand[] results)
        {
            OpCode op = context.CurrOp;

            FPHalfSwizzle swizzle = FPHalfSwizzle.FP16;

            if (!(op is OpCodeAluImm32))
            {
                swizzle = (FPHalfSwizzle)context.CurrOp.RawOpCode.Extract(49, 2);
            }

            switch (swizzle)
            {
                case FPHalfSwizzle.FP16: return context.PackHalf2x16(results[0], results[1]);

                case FPHalfSwizzle.FP32: return results[0];

                case FPHalfSwizzle.DupH0:
                {
                    Operand h1 = GetHalfDest(context, isHigh: true);

                    return context.PackHalf2x16(results[0], h1);
                }

                case FPHalfSwizzle.DupH1:
                {
                    Operand h0 = GetHalfDest(context, isHigh: false);

                    return context.PackHalf2x16(h0, results[1]);
                }
            }

            throw new ArgumentException($"Invalid swizzle \"{swizzle}\".");
        }

        public static Operand GetHalfDest(EmitterContext context, bool isHigh)
        {
            if (isHigh)
            {
                return context.UnpackHalf2x16High(GetDest(context));
            }
            else
            {
                return context.UnpackHalf2x16Low(GetDest(context));
            }
        }

        public static Operand GetPredicate39(EmitterContext context)
        {
            IOpCodePredicate39 op = (IOpCodePredicate39)context.CurrOp;

            Operand local = Register(op.Predicate39);

            if (op.InvertP)
            {
                local = context.BitwiseNot(local);
            }

            return local;
        }

        public static Operand SignExtendTo32(EmitterContext context, Operand src, int srcBits)
        {
            return context.BitfieldExtractS32(src, Const(0), Const(srcBits));
        }

        public static Operand ZeroExtendTo32(EmitterContext context, Operand src, int srcBits)
        {
            int mask = (int)(0xffffffffu >> (32 - srcBits));

            return context.BitwiseAnd(src, Const(mask));
        }
    }
}