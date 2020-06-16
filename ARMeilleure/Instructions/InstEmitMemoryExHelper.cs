using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System.Reflection;

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
            MethodInfo info = null;

            if (exclusive)
            {
                switch (size)
                {
                    case 0: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadByteExclusive));      break;
                    case 1: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadUInt16Exclusive));    break;
                    case 2: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadUInt32Exclusive));    break;
                    case 3: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadUInt64Exclusive));    break;
                    case 4: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadVector128Exclusive)); break;
                }
            }
            else
            {
                switch (size)
                {
                    case 0: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadByte));      break;
                    case 1: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadUInt16));    break;
                    case 2: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadUInt32));    break;
                    case 3: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadUInt64));    break;
                    case 4: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadVector128)); break;
                }
            }

            return context.Call(info, address);
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

            MethodInfo info = null;

            if (exclusive)
            {
                switch (size)
                {
                    case 0: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteByteExclusive));      break;
                    case 1: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteUInt16Exclusive));    break;
                    case 2: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteUInt32Exclusive));    break;
                    case 3: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteUInt64Exclusive));    break;
                    case 4: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteVector128Exclusive)); break;
                }

                return context.Call(info, address, value);
            }
            else
            {
                switch (size)
                {
                    case 0: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteByte));      break;
                    case 1: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteUInt16));    break;
                    case 2: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteUInt32));    break;
                    case 3: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteUInt64));    break;
                    case 4: info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteVector128)); break;
                }

                context.Call(info, address, value);

                return null;
            }
        }
    }
}
