using System;

namespace ChocolArm64
{
    using System.Reflection.Emit;

    static class ILGeneratorEx
    {
        public static void EmitLdc_I4(this ILGenerator Generator,int Value)
        {
            switch (Value)
            {
                case  0: Generator.Emit(OpCodes.Ldc_I4_0);      break;
                case  1: Generator.Emit(OpCodes.Ldc_I4_1);      break;
                case  2: Generator.Emit(OpCodes.Ldc_I4_2);      break;
                case  3: Generator.Emit(OpCodes.Ldc_I4_3);      break;
                case  4: Generator.Emit(OpCodes.Ldc_I4_4);      break;
                case  5: Generator.Emit(OpCodes.Ldc_I4_5);      break;
                case  6: Generator.Emit(OpCodes.Ldc_I4_6);      break;
                case  7: Generator.Emit(OpCodes.Ldc_I4_7);      break;
                case  8: Generator.Emit(OpCodes.Ldc_I4_8);      break;
                case -1: Generator.Emit(OpCodes.Ldc_I4_M1);     break;
                default: Generator.Emit(OpCodes.Ldc_I4, Value); break;
            }
        }

        public static void EmitLdarg(this ILGenerator Generator, int Index)
        {
            switch (Index)
            {
                case 0:  Generator.Emit(OpCodes.Ldarg_0); break;
                case 1:  Generator.Emit(OpCodes.Ldarg_1); break;
                case 2:  Generator.Emit(OpCodes.Ldarg_2); break;
                case 3:  Generator.Emit(OpCodes.Ldarg_3); break;

                default:
                    if ((uint)Index <= byte.MaxValue)
                    {
                        Generator.Emit(OpCodes.Ldarg_S, (byte)Index);
                    }
                    else if ((uint)Index < ushort.MaxValue)
                    {
                        Generator.Emit(OpCodes.Ldarg, (short)Index);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(Index));
                    }
                    break;
            }
        }

        public static void EmitStarg(this ILGenerator Generator, int Index)
        {
            if ((uint)Index <= byte.MaxValue)
            {
                Generator.Emit(OpCodes.Starg_S, (byte)Index);
            }
            else if ((uint)Index < ushort.MaxValue)
            {
                Generator.Emit(OpCodes.Starg, (short)Index);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Index));
            } 
        }

        public static void EmitLdloc(this ILGenerator Generator, int Index)
        {
            switch (Index)
            {
                case 0:  Generator.Emit(OpCodes.Ldloc_0); break;
                case 1:  Generator.Emit(OpCodes.Ldloc_1); break;
                case 2:  Generator.Emit(OpCodes.Ldloc_2); break;
                case 3:  Generator.Emit(OpCodes.Ldloc_3); break;

                default:
                    if ((uint)Index <= byte.MaxValue)
                    {
                        Generator.Emit(OpCodes.Ldloc_S, (byte)Index);
                    }
                    else if ((uint)Index < ushort.MaxValue)
                    {
                        Generator.Emit(OpCodes.Ldloc, (short)Index);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(Index));
                    }
                    break;
            }            
        }

        public static void EmitStloc(this ILGenerator Generator, int Index)
        {
            switch (Index)
            {
                case 0:  Generator.Emit(OpCodes.Stloc_0); break;
                case 1:  Generator.Emit(OpCodes.Stloc_1); break;
                case 2:  Generator.Emit(OpCodes.Stloc_2); break;
                case 3:  Generator.Emit(OpCodes.Stloc_3); break;

                default:
                    if ((uint)Index <= byte.MaxValue)
                    {
                        Generator.Emit(OpCodes.Stloc_S, (byte)Index);
                    }
                    else if ((uint)Index < ushort.MaxValue)
                    {
                        Generator.Emit(OpCodes.Stloc, (short)Index);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(Index));
                    }
                    break;
            }
        }

        public static void EmitLdargSeq(this ILGenerator Generator, int Count)
        {
            for (int Index = 0; Index < Count; Index++)
            {
                Generator.EmitLdarg(Index);
            }         
        }
    }
}