using ChocolArm64.Decoders;
using ChocolArm64.Memory;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static class InstEmitMemoryHelper
    {
        private enum Extension
        {
            Zx,
            Sx32,
            Sx64
        }

        public static void EmitReadZxCall(ILEmitterCtx context, int size)
        {
            EmitReadCall(context, Extension.Zx, size);
        }

        public static void EmitReadSx32Call(ILEmitterCtx context, int size)
        {
            EmitReadCall(context, Extension.Sx32, size);
        }

        public static void EmitReadSx64Call(ILEmitterCtx context, int size)
        {
            EmitReadCall(context, Extension.Sx64, size);
        }

        private static void EmitReadCall(ILEmitterCtx context, Extension ext, int size)
        {
            bool isSimd = GetIsSimd(context);

            string name = null;

            if (size < 0 || size > (isSimd ? 4 : 3))
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (isSimd)
            {
                switch (size)
                {
                    case 0: name = nameof(MemoryManager.ReadVector8);   break;
                    case 1: name = nameof(MemoryManager.ReadVector16);  break;
                    case 2: name = nameof(MemoryManager.ReadVector32);  break;
                    case 3: name = nameof(MemoryManager.ReadVector64);  break;
                    case 4: name = nameof(MemoryManager.ReadVector128); break;
                }
            }
            else
            {
                switch (size)
                {
                    case 0: name = nameof(MemoryManager.ReadByte);   break;
                    case 1: name = nameof(MemoryManager.ReadUInt16); break;
                    case 2: name = nameof(MemoryManager.ReadUInt32); break;
                    case 3: name = nameof(MemoryManager.ReadUInt64); break;
                }
            }

            context.EmitCall(typeof(MemoryManager), name);

            if (!isSimd)
            {
                if (ext == Extension.Sx32 ||
                    ext == Extension.Sx64)
                {
                    switch (size)
                    {
                        case 0: context.Emit(OpCodes.Conv_I1); break;
                        case 1: context.Emit(OpCodes.Conv_I2); break;
                        case 2: context.Emit(OpCodes.Conv_I4); break;
                    }
                }

                if (size < 3)
                {
                    context.Emit(ext == Extension.Sx64
                        ? OpCodes.Conv_I8
                        : OpCodes.Conv_U8);
                }
            }
        }

        public static void EmitWriteCall(ILEmitterCtx context, int size)
        {
            bool isSimd = GetIsSimd(context);

            string name = null;

            if (size < 0 || size > (isSimd ? 4 : 3))
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (size < 3 && !isSimd)
            {
                context.Emit(OpCodes.Conv_I4);
            }

            if (isSimd)
            {
                switch (size)
                {
                    case 0: name = nameof(MemoryManager.WriteVector8);   break;
                    case 1: name = nameof(MemoryManager.WriteVector16);  break;
                    case 2: name = nameof(MemoryManager.WriteVector32);  break;
                    case 3: name = nameof(MemoryManager.WriteVector64);  break;
                    case 4: name = nameof(MemoryManager.WriteVector128); break;
                }
            }
            else
            {
                switch (size)
                {
                    case 0: name = nameof(MemoryManager.WriteByte);   break;
                    case 1: name = nameof(MemoryManager.WriteUInt16); break;
                    case 2: name = nameof(MemoryManager.WriteUInt32); break;
                    case 3: name = nameof(MemoryManager.WriteUInt64); break;
                }
            }

            context.EmitCall(typeof(MemoryManager), name);
        }

        private static bool GetIsSimd(ILEmitterCtx context)
        {
            return context.CurrOp is IOpCodeSimd64 &&
                 !(context.CurrOp is OpCodeSimdMemMs64 ||
                   context.CurrOp is OpCodeSimdMemSs64);
        }
    }
}