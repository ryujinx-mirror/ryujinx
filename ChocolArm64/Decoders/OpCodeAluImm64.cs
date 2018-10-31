using ChocolArm64.Instructions;
using System;

namespace ChocolArm64.Decoders
{
    class OpCodeAluImm64 : OpCodeAlu64, IOpCodeAluImm64
    {
        public long Imm { get; private set; }

        public OpCodeAluImm64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            if (DataOp == DataOp.Arithmetic)
            {
                Imm = (opCode >> 10) & 0xfff;

                int shift = (opCode >> 22) & 3;

                Imm <<= shift * 12;
            }
            else if (DataOp == DataOp.Logical)
            {
                var bm = DecoderHelper.DecodeBitMask(opCode, true);

                if (bm.IsUndefined)
                {
                    Emitter = InstEmit.Und;

                    return;
                }

                Imm = bm.WMask;
            }
            else
            {
                throw new ArgumentException(nameof(opCode));
            }
        }
    }
}