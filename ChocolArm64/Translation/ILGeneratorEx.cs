using System;

namespace ChocolArm64
{
    using System.Reflection.Emit;

    static class ILGeneratorEx
    {
        public static void EmitLdc_I4(this ILGenerator generator, int value)
        {
            switch (value)
            {
                case  0: generator.Emit(OpCodes.Ldc_I4_0);      break;
                case  1: generator.Emit(OpCodes.Ldc_I4_1);      break;
                case  2: generator.Emit(OpCodes.Ldc_I4_2);      break;
                case  3: generator.Emit(OpCodes.Ldc_I4_3);      break;
                case  4: generator.Emit(OpCodes.Ldc_I4_4);      break;
                case  5: generator.Emit(OpCodes.Ldc_I4_5);      break;
                case  6: generator.Emit(OpCodes.Ldc_I4_6);      break;
                case  7: generator.Emit(OpCodes.Ldc_I4_7);      break;
                case  8: generator.Emit(OpCodes.Ldc_I4_8);      break;
                case -1: generator.Emit(OpCodes.Ldc_I4_M1);     break;
                default: generator.Emit(OpCodes.Ldc_I4, value); break;
            }
        }

        public static void EmitLdarg(this ILGenerator generator, int index)
        {
            switch (index)
            {
                case 0:  generator.Emit(OpCodes.Ldarg_0); break;
                case 1:  generator.Emit(OpCodes.Ldarg_1); break;
                case 2:  generator.Emit(OpCodes.Ldarg_2); break;
                case 3:  generator.Emit(OpCodes.Ldarg_3); break;

                default:
                    if ((uint)index <= byte.MaxValue)
                    {
                        generator.Emit(OpCodes.Ldarg_S, (byte)index);
                    }
                    else if ((uint)index < ushort.MaxValue)
                    {
                        generator.Emit(OpCodes.Ldarg, (short)index);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }
                    break;
            }
        }

        public static void EmitStarg(this ILGenerator generator, int index)
        {
            if ((uint)index <= byte.MaxValue)
            {
                generator.Emit(OpCodes.Starg_S, (byte)index);
            }
            else if ((uint)index < ushort.MaxValue)
            {
                generator.Emit(OpCodes.Starg, (short)index);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public static void EmitLdloc(this ILGenerator generator, int index)
        {
            switch (index)
            {
                case 0:  generator.Emit(OpCodes.Ldloc_0); break;
                case 1:  generator.Emit(OpCodes.Ldloc_1); break;
                case 2:  generator.Emit(OpCodes.Ldloc_2); break;
                case 3:  generator.Emit(OpCodes.Ldloc_3); break;

                default:
                    if ((uint)index <= byte.MaxValue)
                    {
                        generator.Emit(OpCodes.Ldloc_S, (byte)index);
                    }
                    else if ((uint)index < ushort.MaxValue)
                    {
                        generator.Emit(OpCodes.Ldloc, (short)index);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }
                    break;
            }
        }

        public static void EmitStloc(this ILGenerator generator, int index)
        {
            switch (index)
            {
                case 0:  generator.Emit(OpCodes.Stloc_0); break;
                case 1:  generator.Emit(OpCodes.Stloc_1); break;
                case 2:  generator.Emit(OpCodes.Stloc_2); break;
                case 3:  generator.Emit(OpCodes.Stloc_3); break;

                default:
                    if ((uint)index <= byte.MaxValue)
                    {
                        generator.Emit(OpCodes.Stloc_S, (byte)index);
                    }
                    else if ((uint)index < ushort.MaxValue)
                    {
                        generator.Emit(OpCodes.Stloc, (short)index);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }
                    break;
            }
        }

        public static void EmitLdargSeq(this ILGenerator generator, int count)
        {
            for (int index = 0; index < count; index++)
            {
                generator.EmitLdarg(index);
            }
        }
    }
}
