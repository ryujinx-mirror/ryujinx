using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;

namespace ARMeilleure.Instructions
{
    static class InstEmitMemoryExHelper
    {
        public static Operand EmitLoadExclusive(
            ArmEmitterContext context,
            Operand address,
            bool exclusive,
            int size)
        {
            Delegate fallbackMethodDlg = null;

            if (exclusive)
            {
                switch (size)
                {
                    case 0: fallbackMethodDlg = new _U8_U64(NativeInterface.ReadByteExclusive); break;
                    case 1: fallbackMethodDlg = new _U16_U64(NativeInterface.ReadUInt16Exclusive); break;
                    case 2: fallbackMethodDlg = new _U32_U64(NativeInterface.ReadUInt32Exclusive); break;
                    case 3: fallbackMethodDlg = new _U64_U64(NativeInterface.ReadUInt64Exclusive); break;
                    case 4: fallbackMethodDlg = new _V128_U64(NativeInterface.ReadVector128Exclusive); break;
                }
            }
            else
            {
                switch (size)
                {
                    case 0: fallbackMethodDlg = new _U8_U64(NativeInterface.ReadByte); break;
                    case 1: fallbackMethodDlg = new _U16_U64(NativeInterface.ReadUInt16); break;
                    case 2: fallbackMethodDlg = new _U32_U64(NativeInterface.ReadUInt32); break;
                    case 3: fallbackMethodDlg = new _U64_U64(NativeInterface.ReadUInt64); break;
                    case 4: fallbackMethodDlg = new _V128_U64(NativeInterface.ReadVector128); break;
                }
            }

            return context.Call(fallbackMethodDlg, address);
        }

        public static Operand EmitStoreExclusive(
            ArmEmitterContext context,
            Operand address,
            Operand value,
            bool exclusive,
            int size)
        {
            if (size < 3)
            {
                value = context.ConvertI64ToI32(value);
            }

            Delegate fallbackMethodDlg = null;

            if (exclusive)
            {
                switch (size)
                {
                    case 0: fallbackMethodDlg = new _S32_U64_U8(NativeInterface.WriteByteExclusive); break;
                    case 1: fallbackMethodDlg = new _S32_U64_U16(NativeInterface.WriteUInt16Exclusive); break;
                    case 2: fallbackMethodDlg = new _S32_U64_U32(NativeInterface.WriteUInt32Exclusive); break;
                    case 3: fallbackMethodDlg = new _S32_U64_U64(NativeInterface.WriteUInt64Exclusive); break;
                    case 4: fallbackMethodDlg = new _S32_U64_V128(NativeInterface.WriteVector128Exclusive); break;
                }

                return context.Call(fallbackMethodDlg, address, value);
            }
            else
            {
                switch (size)
                {
                    case 0: fallbackMethodDlg = new _Void_U64_U8(NativeInterface.WriteByte); break;
                    case 1: fallbackMethodDlg = new _Void_U64_U16(NativeInterface.WriteUInt16); break;
                    case 2: fallbackMethodDlg = new _Void_U64_U32(NativeInterface.WriteUInt32); break;
                    case 3: fallbackMethodDlg = new _Void_U64_U64(NativeInterface.WriteUInt64); break;
                    case 4: fallbackMethodDlg = new _Void_U64_V128(NativeInterface.WriteVector128); break;
                }

                context.Call(fallbackMethodDlg, address, value);

                return null;
            }
        }
    }
}
