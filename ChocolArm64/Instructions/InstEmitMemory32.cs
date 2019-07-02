using ChocolArm64.Decoders;
using ChocolArm64.IntermediateRepresentation;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

using static ChocolArm64.Instructions.InstEmit32Helper;
using static ChocolArm64.Instructions.InstEmitMemoryHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit32
    {
        private const int ByteSizeLog2  = 0;
        private const int HWordSizeLog2 = 1;
        private const int WordSizeLog2  = 2;
        private const int DWordSizeLog2 = 3;

        [Flags]
        enum AccessType
        {
            Store  = 0,
            Signed = 1,
            Load   = 2,

            LoadZx = Load,
            LoadSx = Load | Signed,
        }

        public static void Ldm(ILEmitterCtx context)
        {
            OpCode32MemMult op = (OpCode32MemMult)context.CurrOp;

            EmitLoadFromRegister(context, op.Rn);

            bool writesToPc = (op.RegisterMask & (1 << RegisterAlias.Aarch32Pc)) != 0;

            bool writeBack = op.PostOffset != 0 && (op.Rn != RegisterAlias.Aarch32Pc || !writesToPc);

            if (writeBack)
            {
                context.Emit(OpCodes.Dup);
            }

            context.EmitLdc_I4(op.Offset);

            context.Emit(OpCodes.Add);

            context.EmitSttmp();

            if (writeBack)
            {
                context.EmitLdc_I4(op.PostOffset);

                context.Emit(OpCodes.Add);

                EmitStoreToRegister(context, op.Rn);
            }

            int mask   = op.RegisterMask;
            int offset = 0;

            for (int register = 0; mask != 0; mask >>= 1, register++)
            {
                if ((mask & 1) != 0)
                {
                    context.EmitLdtmp();
                    context.EmitLdc_I4(offset);

                    context.Emit(OpCodes.Add);

                    EmitReadZxCall(context, WordSizeLog2);

                    EmitStoreToRegister(context, register);

                    offset += 4;
                }
            }
        }

        public static void Ldr(ILEmitterCtx context)
        {
            EmitLoadOrStore(context, WordSizeLog2, AccessType.LoadZx);
        }

        public static void Ldrb(ILEmitterCtx context)
        {
            EmitLoadOrStore(context, ByteSizeLog2, AccessType.LoadZx);
        }

        public static void Ldrd(ILEmitterCtx context)
        {
            EmitLoadOrStore(context, DWordSizeLog2, AccessType.LoadZx);
        }

        public static void Ldrh(ILEmitterCtx context)
        {
            EmitLoadOrStore(context, HWordSizeLog2, AccessType.LoadZx);
        }

        public static void Ldrsb(ILEmitterCtx context)
        {
            EmitLoadOrStore(context, ByteSizeLog2, AccessType.LoadSx);
        }

        public static void Ldrsh(ILEmitterCtx context)
        {
            EmitLoadOrStore(context, HWordSizeLog2, AccessType.LoadSx);
        }

        public static void Stm(ILEmitterCtx context)
        {
            OpCode32MemMult op = (OpCode32MemMult)context.CurrOp;

            EmitLoadFromRegister(context, op.Rn);

            context.EmitLdc_I4(op.Offset);

            context.Emit(OpCodes.Add);

            context.EmitSttmp();

            int mask   = op.RegisterMask;
            int offset = 0;

            for (int register = 0; mask != 0; mask >>= 1, register++)
            {
                if ((mask & 1) != 0)
                {
                    context.EmitLdtmp();
                    context.EmitLdc_I4(offset);

                    context.Emit(OpCodes.Add);

                    EmitLoadFromRegister(context, register);

                    EmitWriteCall(context, WordSizeLog2);

                    // Note: If Rn is also specified on the register list,
                    // and Rn is the first register on this list, then the
                    // value that is written to memory is the unmodified value,
                    // before the write back. If it is on the list, but it's
                    // not the first one, then the value written to memory
                    // varies between CPUs.
                    if (offset == 0 && op.PostOffset != 0)
                    {
                        // Emit write back after the first write.
                        EmitLoadFromRegister(context, op.Rn);

                        context.EmitLdc_I4(op.PostOffset);

                        context.Emit(OpCodes.Add);

                        EmitStoreToRegister(context, op.Rn);
                    }

                    offset += 4;
                }
            }
        }

        public static void Str(ILEmitterCtx context)
        {
            EmitLoadOrStore(context, WordSizeLog2, AccessType.Store);
        }

        public static void Strb(ILEmitterCtx context)
        {
            EmitLoadOrStore(context, ByteSizeLog2, AccessType.Store);
        }

        public static void Strd(ILEmitterCtx context)
        {
            EmitLoadOrStore(context, DWordSizeLog2, AccessType.Store);
        }

        public static void Strh(ILEmitterCtx context)
        {
            EmitLoadOrStore(context, HWordSizeLog2, AccessType.Store);
        }

        private static void EmitLoadOrStore(ILEmitterCtx context, int size, AccessType accType)
        {
            OpCode32Mem op = (OpCode32Mem)context.CurrOp;

            if (op.Index || op.WBack)
            {
                EmitLoadFromRegister(context, op.Rn);

                context.EmitLdc_I4(op.Imm);

                context.Emit(op.Add ? OpCodes.Add : OpCodes.Sub);

                context.EmitSttmp();
            }

            if (op.Index)
            {
                context.EmitLdtmp();
            }
            else
            {
                EmitLoadFromRegister(context, op.Rn);
            }

            if ((accType & AccessType.Load) != 0)
            {
                if ((accType & AccessType.Signed) != 0)
                {
                    EmitReadSx32Call(context, size);
                }
                else
                {
                    EmitReadZxCall(context, size);
                }

                if (op.WBack)
                {
                    context.EmitLdtmp();

                    EmitStoreToRegister(context, op.Rn);
                }

                if (size == DWordSizeLog2)
                {
                    context.Emit(OpCodes.Dup);

                    context.EmitLdflg((int)PState.EBit);

                    ILLabel lblBigEndian = new ILLabel();
                    ILLabel lblEnd       = new ILLabel();

                    context.Emit(OpCodes.Brtrue_S, lblBigEndian);

                    // Little endian mode.
                    context.Emit(OpCodes.Conv_U4);

                    EmitStoreToRegister(context, op.Rt);

                    context.EmitLsr(32);

                    context.Emit(OpCodes.Conv_U4);

                    EmitStoreToRegister(context, op.Rt | 1);

                    context.Emit(OpCodes.Br_S, lblEnd);

                    // Big endian mode.
                    context.MarkLabel(lblBigEndian);

                    context.EmitLsr(32);

                    context.Emit(OpCodes.Conv_U4);

                    EmitStoreToRegister(context, op.Rt);

                    context.Emit(OpCodes.Conv_U4);

                    EmitStoreToRegister(context, op.Rt | 1);

                    context.MarkLabel(lblEnd);
                }
                else
                {
                    EmitStoreToRegister(context, op.Rt);
                }
            }
            else
            {
                if (op.WBack)
                {
                    context.EmitLdtmp();

                    EmitStoreToRegister(context, op.Rn);
                }

                EmitLoadFromRegister(context, op.Rt);

                if (size == DWordSizeLog2)
                {
                    context.Emit(OpCodes.Conv_U8);

                    context.EmitLdflg((int)PState.EBit);

                    ILLabel lblBigEndian = new ILLabel();
                    ILLabel lblEnd       = new ILLabel();

                    context.Emit(OpCodes.Brtrue_S, lblBigEndian);

                    // Little endian mode.
                    EmitLoadFromRegister(context, op.Rt | 1);

                    context.Emit(OpCodes.Conv_U8);

                    context.EmitLsl(32);

                    context.Emit(OpCodes.Or);

                    context.Emit(OpCodes.Br_S, lblEnd);

                    // Big endian mode.
                    context.MarkLabel(lblBigEndian);

                    context.EmitLsl(32);

                    EmitLoadFromRegister(context, op.Rt | 1);

                    context.Emit(OpCodes.Conv_U8);

                    context.Emit(OpCodes.Or);

                    context.MarkLabel(lblEnd);
                }

                EmitWriteCall(context, size);
            }
        }
    }
}