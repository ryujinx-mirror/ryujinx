using ChocolArm64.Decoders;
using ChocolArm64.IntermediateRepresentation;
using ChocolArm64.Memory;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

namespace ChocolArm64.Instructions
{
    static class InstEmitMemoryHelper
    {
        private static int _tempIntAddress = ILEmitterCtx.GetIntTempIndex();
        private static int _tempIntValue   = ILEmitterCtx.GetIntTempIndex();
        private static int _tempIntPtAddr  = ILEmitterCtx.GetIntTempIndex();
        private static int _tempVecValue   = ILEmitterCtx.GetVecTempIndex();

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
            //Save the address into a temp.
            context.EmitStint(_tempIntAddress);

            bool isSimd = IsSimd(context);

            if (size < 0 || size > (isSimd ? 4 : 3))
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (isSimd)
            {
                if (context.Tier == TranslationTier.Tier0 || !Sse2.IsSupported || size < 2)
                {
                    EmitReadVectorFallback(context, size);
                }
                else
                {
                    EmitReadVector(context, size);
                }
            }
            else
            {
                if (context.Tier == TranslationTier.Tier0)
                {
                    EmitReadIntFallback(context, size);
                }
                else
                {
                    EmitReadInt(context, size);
                }
            }

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
            bool isSimd = IsSimd(context);

            //Save the value into a temp.
            if (isSimd)
            {
                context.EmitStvec(_tempVecValue);
            }
            else
            {
                context.EmitStint(_tempIntValue);
            }

            //Save the address into a temp.
            context.EmitStint(_tempIntAddress);

            if (size < 0 || size > (isSimd ? 4 : 3))
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (isSimd)
            {
                if (context.Tier == TranslationTier.Tier0 || !Sse2.IsSupported || size < 2)
                {
                    EmitWriteVectorFallback(context, size);
                }
                else
                {
                    EmitWriteVector(context, size);
                }
            }
            else
            {
                if (context.Tier == TranslationTier.Tier0)
                {
                    EmitWriteIntFallback(context, size);
                }
                else
                {
                    EmitWriteInt(context, size);
                }
            }
        }

        private static bool IsSimd(ILEmitterCtx context)
        {
            return context.CurrOp is IOpCodeSimd64 &&
                 !(context.CurrOp is OpCodeSimdMemMs64 ||
                   context.CurrOp is OpCodeSimdMemSs64);
        }

        private static void EmitReadInt(ILEmitterCtx context, int size)
        {
            EmitAddressCheck(context, size);

            ILLabel lblFastPath = new ILLabel();
            ILLabel lblSlowPath = new ILLabel();
            ILLabel lblEnd      = new ILLabel();

            context.Emit(OpCodes.Brfalse_S, lblFastPath);

            context.MarkLabel(lblSlowPath);

            EmitReadIntFallback(context, size);

            context.Emit(OpCodes.Br, lblEnd);

            context.MarkLabel(lblFastPath);

            EmitPtPointerLoad(context, lblSlowPath);

            switch (size)
            {
                case 0: context.Emit(OpCodes.Ldind_U1); break;
                case 1: context.Emit(OpCodes.Ldind_U2); break;
                case 2: context.Emit(OpCodes.Ldind_U4); break;
                case 3: context.Emit(OpCodes.Ldind_I8); break;
            }

            context.MarkLabel(lblEnd);
        }

        private static void EmitReadVector(ILEmitterCtx context, int size)
        {
            EmitAddressCheck(context, size);

            ILLabel lblFastPath = new ILLabel();
            ILLabel lblSlowPath = new ILLabel();
            ILLabel lblEnd      = new ILLabel();

            context.Emit(OpCodes.Brfalse_S, lblFastPath);

            context.MarkLabel(lblSlowPath);

            EmitReadVectorFallback(context, size);

            context.Emit(OpCodes.Br, lblEnd);

            context.MarkLabel(lblFastPath);

            EmitPtPointerLoad(context, lblSlowPath);

            switch (size)
            {
                case 2: context.EmitCall(typeof(Sse), nameof(Sse.LoadScalarVector128));  break;

                case 3:
                {
                    Type[] types = new Type[] { typeof(double*) };

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.LoadScalarVector128), types));

                    break;
                }

                case 4: context.EmitCall(typeof(Sse), nameof(Sse.LoadAlignedVector128)); break;

                throw new InvalidOperationException($"Invalid vector load size of {1 << size} bytes.");
            }

            context.MarkLabel(lblEnd);
        }

        private static void EmitWriteInt(ILEmitterCtx context, int size)
        {
            EmitAddressCheck(context, size);

            ILLabel lblFastPath = new ILLabel();
            ILLabel lblSlowPath = new ILLabel();
            ILLabel lblEnd      = new ILLabel();

            context.Emit(OpCodes.Brfalse_S, lblFastPath);

            context.MarkLabel(lblSlowPath);

            EmitWriteIntFallback(context, size);

            context.Emit(OpCodes.Br, lblEnd);

            context.MarkLabel(lblFastPath);

            EmitPtPointerLoad(context, lblSlowPath);

            context.EmitLdint(_tempIntValue);

            if (size < 3)
            {
                context.Emit(OpCodes.Conv_U4);
            }

            switch (size)
            {
                case 0: context.Emit(OpCodes.Stind_I1); break;
                case 1: context.Emit(OpCodes.Stind_I2); break;
                case 2: context.Emit(OpCodes.Stind_I4); break;
                case 3: context.Emit(OpCodes.Stind_I8); break;
            }

            context.MarkLabel(lblEnd);
        }

        private static void EmitWriteVector(ILEmitterCtx context, int size)
        {
            EmitAddressCheck(context, size);

            ILLabel lblFastPath = new ILLabel();
            ILLabel lblSlowPath = new ILLabel();
            ILLabel lblEnd      = new ILLabel();

            context.Emit(OpCodes.Brfalse_S, lblFastPath);

            context.MarkLabel(lblSlowPath);

            EmitWriteVectorFallback(context, size);

            context.Emit(OpCodes.Br, lblEnd);

            context.MarkLabel(lblFastPath);

            EmitPtPointerLoad(context, lblSlowPath);

            context.EmitLdvec(_tempVecValue);

            switch (size)
            {
                case 2: context.EmitCall(typeof(Sse),  nameof(Sse.StoreScalar));  break;
                case 3: context.EmitCall(typeof(Sse2), nameof(Sse2.StoreScalar)); break;
                case 4: context.EmitCall(typeof(Sse),  nameof(Sse.StoreAligned)); break;

                default: throw new InvalidOperationException($"Invalid vector store size of {1 << size} bytes.");
            }

            context.MarkLabel(lblEnd);
        }

        private static void EmitAddressCheck(ILEmitterCtx context, int size)
        {
            long addressCheckMask = ~(context.Memory.AddressSpaceSize - 1);

            addressCheckMask |= (1u << size) - 1;

            context.EmitLdint(_tempIntAddress);

            context.EmitLdc_I(addressCheckMask);

            context.Emit(OpCodes.And);
        }

        private static void EmitPtPointerLoad(ILEmitterCtx context, ILLabel lblFallbackPath)
        {
            context.EmitLdc_I8(context.Memory.PageTable.ToInt64());

            context.Emit(OpCodes.Conv_I);

            int bit = MemoryManager.PageBits;

            do
            {
                context.EmitLdint(_tempIntAddress);

                if (context.CurrOp.RegisterSize == RegisterSize.Int32)
                {
                    context.Emit(OpCodes.Conv_U8);
                }

                context.EmitLsr(bit);

                bit += context.Memory.PtLevelBits;

                if (bit < context.Memory.AddressSpaceBits)
                {
                    context.EmitLdc_I8(context.Memory.PtLevelMask);

                    context.Emit(OpCodes.And);
                }

                context.EmitLdc_I8(IntPtr.Size);

                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Conv_I);
                context.Emit(OpCodes.Add);
                context.Emit(OpCodes.Ldind_I);
            }
            while (bit < context.Memory.AddressSpaceBits);

            if (!context.Memory.HasWriteWatchSupport)
            {
                context.Emit(OpCodes.Conv_U8);

                context.EmitStint(_tempIntPtAddr);
                context.EmitLdint(_tempIntPtAddr);

                context.EmitLdc_I8(MemoryManager.PteFlagsMask);

                context.Emit(OpCodes.And);

                context.Emit(OpCodes.Brtrue, lblFallbackPath);

                context.EmitLdint(_tempIntPtAddr);

                context.Emit(OpCodes.Conv_I);
            }

            context.EmitLdint(_tempIntAddress);

            context.EmitLdc_I(MemoryManager.PageMask);

            context.Emit(OpCodes.And);
            context.Emit(OpCodes.Conv_I);
            context.Emit(OpCodes.Add);
        }

        private static void EmitReadIntFallback(ILEmitterCtx context, int size)
        {
            context.EmitLdarg(TranslatedSub.MemoryArgIdx);
            context.EmitLdint(_tempIntAddress);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U8);
            }

            string fallbackMethodName = null;

            switch (size)
            {
                case 0: fallbackMethodName = nameof(MemoryManager.ReadByte);   break;
                case 1: fallbackMethodName = nameof(MemoryManager.ReadUInt16); break;
                case 2: fallbackMethodName = nameof(MemoryManager.ReadUInt32); break;
                case 3: fallbackMethodName = nameof(MemoryManager.ReadUInt64); break;
            }

            context.EmitCall(typeof(MemoryManager), fallbackMethodName);
        }

        private static void EmitReadVectorFallback(ILEmitterCtx context, int size)
        {
            context.EmitLdarg(TranslatedSub.MemoryArgIdx);
            context.EmitLdint(_tempIntAddress);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U8);
            }

            string fallbackMethodName = null;

            switch (size)
            {
                case 0: fallbackMethodName = nameof(MemoryManager.ReadVector8);   break;
                case 1: fallbackMethodName = nameof(MemoryManager.ReadVector16);  break;
                case 2: fallbackMethodName = nameof(MemoryManager.ReadVector32);  break;
                case 3: fallbackMethodName = nameof(MemoryManager.ReadVector64);  break;
                case 4: fallbackMethodName = nameof(MemoryManager.ReadVector128); break;
            }

            context.EmitCall(typeof(MemoryManager), fallbackMethodName);
        }

        private static void EmitWriteIntFallback(ILEmitterCtx context, int size)
        {
            context.EmitLdarg(TranslatedSub.MemoryArgIdx);
            context.EmitLdint(_tempIntAddress);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U8);
            }

            context.EmitLdint(_tempIntValue);

            if (size < 3)
            {
                context.Emit(OpCodes.Conv_U4);
            }

            string fallbackMethodName = null;

            switch (size)
            {
                case 0: fallbackMethodName = nameof(MemoryManager.WriteByte);   break;
                case 1: fallbackMethodName = nameof(MemoryManager.WriteUInt16); break;
                case 2: fallbackMethodName = nameof(MemoryManager.WriteUInt32); break;
                case 3: fallbackMethodName = nameof(MemoryManager.WriteUInt64); break;
            }

            context.EmitCall(typeof(MemoryManager), fallbackMethodName);
        }

        private static void EmitWriteVectorFallback(ILEmitterCtx context, int size)
        {
            context.EmitLdarg(TranslatedSub.MemoryArgIdx);
            context.EmitLdint(_tempIntAddress);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U8);
            }

            context.EmitLdvec(_tempVecValue);

            string fallbackMethodName = null;

            switch (size)
            {
                case 0: fallbackMethodName = nameof(MemoryManager.WriteVector8);   break;
                case 1: fallbackMethodName = nameof(MemoryManager.WriteVector16);  break;
                case 2: fallbackMethodName = nameof(MemoryManager.WriteVector32);  break;
                case 3: fallbackMethodName = nameof(MemoryManager.WriteVector64);  break;
                case 4: fallbackMethodName = nameof(MemoryManager.WriteVector128); break;
            }

            context.EmitCall(typeof(MemoryManager), fallbackMethodName);
        }
    }
}