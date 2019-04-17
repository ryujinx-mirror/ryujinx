using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Ald(EmitterContext context)
        {
            OpCodeAttribute op = (OpCodeAttribute)context.CurrOp;

            Operand[] elems = new Operand[op.Count];

            for (int index = 0; index < op.Count; index++)
            {
                Operand src = Attribute(op.AttributeOffset + index * 4);

                context.Copy(elems[index] = Local(), src);
            }

            for (int index = 0; index < op.Count; index++)
            {
                Register rd = new Register(op.Rd.Index + index, RegisterType.Gpr);

                if (rd.IsRZ)
                {
                    break;
                }

                context.Copy(Register(rd), elems[index]);
            }
        }

        public static void Ast(EmitterContext context)
        {
            OpCodeAttribute op = (OpCodeAttribute)context.CurrOp;

            for (int index = 0; index < op.Count; index++)
            {
                if (op.Rd.Index + index > RegisterConsts.RegisterZeroIndex)
                {
                    break;
                }

                Register rd = new Register(op.Rd.Index + index, RegisterType.Gpr);

                Operand dest = Attribute(op.AttributeOffset + index * 4);

                context.Copy(dest, Register(rd));
            }
        }

        public static void Ipa(EmitterContext context)
        {
            OpCodeIpa op = (OpCodeIpa)context.CurrOp;

            Operand srcA = new Operand(OperandType.Attribute, op.AttributeOffset);

            Operand srcB = GetSrcB(context);

            context.Copy(GetDest(context), srcA);
        }

        public static void Ldc(EmitterContext context)
        {
            OpCodeLdc op = (OpCodeLdc)context.CurrOp;

            if (op.Size > IntegerSize.B64)
            {
                //TODO: Warning.
            }

            bool isSmallInt = op.Size < IntegerSize.B32;

            int count = op.Size == IntegerSize.B64 ? 2 : 1;

            Operand baseOffset = context.Copy(GetSrcA(context));

            for (int index = 0; index < count; index++)
            {
                Register rd = new Register(op.Rd.Index + index, RegisterType.Gpr);

                if (rd.IsRZ)
                {
                    break;
                }

                Operand offset = context.IAdd(baseOffset, Const((op.Offset + index) * 4));

                Operand value = context.LoadConstant(Const(op.Slot), offset);

                if (isSmallInt)
                {
                    Operand shift = context.BitwiseAnd(baseOffset, Const(3));

                    value = context.ShiftRightU32(value, shift);

                    switch (op.Size)
                    {
                        case IntegerSize.U8:  value = ZeroExtendTo32(context, value, 8);  break;
                        case IntegerSize.U16: value = ZeroExtendTo32(context, value, 16); break;
                        case IntegerSize.S8:  value = SignExtendTo32(context, value, 8);  break;
                        case IntegerSize.S16: value = SignExtendTo32(context, value, 16); break;
                    }
                }

                context.Copy(Register(rd), value);
            }
        }

        public static void Out(EmitterContext context)
        {
            OpCode op = context.CurrOp;

            bool emit = op.RawOpCode.Extract(39);
            bool cut  = op.RawOpCode.Extract(40);

            if (!(emit || cut))
            {
                //TODO: Warning.
            }

            if (emit)
            {
                context.EmitVertex();
            }

            if (cut)
            {
                context.EndPrimitive();
            }
        }
    }
}