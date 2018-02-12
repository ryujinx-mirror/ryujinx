using ChocolArm64.Instruction;
using System;

namespace ChocolArm64.Decoder
{
    class AOpCodeAluImm : AOpCodeAlu, IAOpCodeAluImm
    {
        public long Imm { get; private set; }

        public AOpCodeAluImm(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            if (DataOp == ADataOp.Arithmetic)
            {
                Imm = (OpCode >> 10) & 0xfff;

                int Shift = (OpCode >> 22) & 3;

                Imm <<= Shift * 12;
            }
            else if (DataOp == ADataOp.Logical)
            {
                var BM = ADecoderHelper.DecodeBitMask(OpCode, true);

                if (BM.IsUndefined)
                {
                    Emitter = AInstEmit.Und;

                    return;
                }

                Imm = BM.WMask;
            }
            else
            {
                throw new ArgumentException(nameof(OpCode));
            }
        }
    }
}