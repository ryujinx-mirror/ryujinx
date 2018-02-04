using ChocolArm64.Memory;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static class AInstEmitMemoryHelper
    {
        private enum Extension
        {
            Zx,
            Sx32,
            Sx64
        }

        public static void EmitReadZxCall(AILEmitterCtx Context, int Size)
        {
            EmitReadCall(Context, Extension.Zx, Size);
        }

        public static void EmitReadSx32Call(AILEmitterCtx Context, int Size)
        {
            EmitReadCall(Context, Extension.Sx32, Size);
        }

        public static void EmitReadSx64Call(AILEmitterCtx Context, int Size)
        {
            EmitReadCall(Context, Extension.Sx64, Size);
        }

        private static void EmitReadCall(AILEmitterCtx Context, Extension Ext, int Size)
        {
            if (Size < 0 || Size > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            string Name = null;

            switch (Size)
            {
                case 0: Name = nameof(AMemory.ReadByte);      break;
                case 1: Name = nameof(AMemory.ReadUInt16);    break;
                case 2: Name = nameof(AMemory.ReadUInt32);    break;
                case 3: Name = nameof(AMemory.ReadUInt64);    break;
                case 4: Name = nameof(AMemory.ReadVector128); break;
            }

            Context.EmitCall(typeof(AMemory), Name);

            if (Ext == Extension.Sx32 ||
                Ext == Extension.Sx64)
            {
                switch (Size)
                {
                    case 0: Context.Emit(OpCodes.Conv_I1); break;
                    case 1: Context.Emit(OpCodes.Conv_I2); break;
                    case 2: Context.Emit(OpCodes.Conv_I4); break;
                }
            }
            
            if (Size < 3)
            {
                Context.Emit(Ext == Extension.Sx64
                    ? OpCodes.Conv_I8
                    : OpCodes.Conv_U8);
            }
        }

        public static void EmitWriteCall(AILEmitterCtx Context, int Size)
        {
            if (Size < 0 || Size > 4)
            {              
                throw new ArgumentOutOfRangeException(nameof(Size));
            }            

            if (Size < 3)
            {
                Context.Emit(OpCodes.Conv_I4);
            }

            string Name = null;

            switch (Size)
            {
                case 0: Name = nameof(AMemory.WriteByte);      break;
                case 1: Name = nameof(AMemory.WriteUInt16);    break;
                case 2: Name = nameof(AMemory.WriteUInt32);    break;
                case 3: Name = nameof(AMemory.WriteUInt64);    break;
                case 4: Name = nameof(AMemory.WriteVector128); break;
            }

            Context.EmitCall(typeof(AMemory), Name);
        }
    }
}