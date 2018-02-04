using ChocolArm64.State;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    static class AILConv
    {
        public static void EmitConv(AILEmitter Context, Type SrcType, Type TgtType)
        {
            if (SrcType == TgtType)
            {
                //If both types are equal we don't need to cast anything.
                return;
            }

            if (SrcType.IsPrimitive)
            {
                if (TgtType == typeof(byte))
                {
                    Context.Generator.Emit(OpCodes.Conv_U1);
                }
                else if (TgtType == typeof(ushort))
                {
                    Context.Generator.Emit(OpCodes.Conv_U2);
                }
                else if (TgtType == typeof(uint))
                {
                    Context.Generator.Emit(OpCodes.Conv_U4);
                }
                else if (TgtType == typeof(ulong))
                {
                    Context.Generator.Emit(OpCodes.Conv_U8);
                }
                else if (TgtType == typeof(float))
                {
                    Context.Generator.Emit(OpCodes.Conv_R4);
                }
                else if (TgtType == typeof(double))
                {
                    Context.Generator.Emit(OpCodes.Conv_R8);
                }
                else if (TgtType == typeof(AVec))
                {
                    EmitMakeVec(Context, SrcType);
                }
                else
                {
                    throw new ArgumentException(nameof(TgtType));
                }
            }
            else if (SrcType == typeof(AVec))
            {
                if (TgtType == typeof(float))
                {
                    EmitScalarLdfld(Context, nameof(AVec.S0));
                }
                else if (TgtType == typeof(double))
                {
                    EmitScalarLdfld(Context, nameof(AVec.D0));
                }
                else if (TgtType == typeof(byte))
                {
                    EmitScalarLdfld(Context, nameof(AVec.B0));
                }
                else if (TgtType == typeof(ushort))
                {
                    EmitScalarLdfld(Context, nameof(AVec.H0));
                }
                else if (TgtType == typeof(uint))
                {
                    EmitScalarLdfld(Context, nameof(AVec.W0));
                }
                else if (TgtType == typeof(ulong))
                {
                    EmitScalarLdfld(Context, nameof(AVec.X0));
                }
                else
                {
                    throw new ArgumentException(nameof(TgtType));
                }
            }
            else
            {
                throw new ArgumentException(nameof(SrcType));
            }
        }

        private static void EmitScalarLdfld(AILEmitter Context,string FldName)
        {
            Context.Generator.Emit(OpCodes.Ldfld, typeof(AVec).GetField(FldName));
        }

        private static void EmitMakeVec(AILEmitter Context, Type SrcType)
        {
            string MthdName = nameof(MakeScalar);

            Type[] MthdTypes = new Type[] { SrcType };

            MethodInfo MthdInfo = typeof(AILConv).GetMethod(MthdName, MthdTypes);

            Context.Generator.Emit(OpCodes.Call, MthdInfo);
        }

        public static AVec MakeScalar(byte   Value) => new AVec { B0 = Value };
        public static AVec MakeScalar(ushort Value) => new AVec { H0 = Value };
        public static AVec MakeScalar(uint   Value) => new AVec { W0 = Value };
        public static AVec MakeScalar(float  Value) => new AVec { S0 = Value };
        public static AVec MakeScalar(ulong  Value) => new AVec { X0 = Value };
        public static AVec MakeScalar(double Value) => new AVec { D0 = Value };
    }
}