using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64
{
    class ATranslatedSub
    {
        private delegate long AA64Subroutine(ARegisters Register, AMemory Memory);

        private AA64Subroutine ExecDelegate;

        private bool HasDelegate;

        public static Type[] FixedArgTypes { get; private set; }

        public static int RegistersArgIdx { get; private set; }
        public static int MemoryArgIdx    { get; private set; }

        public DynamicMethod Method { get; private set; }

        public HashSet<long> SubCalls { get; private set; }

        public List<ARegister> Params { get; private set; }

        public bool NeedsReJit { get; private set; }

        public ATranslatedSub()
        {
            SubCalls = new HashSet<long>();
        }

        public ATranslatedSub(DynamicMethod Method, List<ARegister> Params) : this()
        {
            if (Params == null)
            {
                throw new ArgumentNullException(nameof(Params));
            }

            this.Method = Method;
            this.Params = Params;
        }

        static ATranslatedSub()
        {
            MethodInfo MthdInfo = typeof(AA64Subroutine).GetMethod("Invoke");

            ParameterInfo[] Params = MthdInfo.GetParameters();

            FixedArgTypes = new Type[Params.Length];

            for (int Index = 0; Index < Params.Length; Index++)
            {
                Type ParamType = Params[Index].ParameterType;

                FixedArgTypes[Index] = ParamType;

                if (ParamType == typeof(ARegisters))
                {
                    RegistersArgIdx = Index;
                }
                else if (ParamType == typeof(AMemory))
                {
                    MemoryArgIdx = Index;
                }
            }
        }

        public long Execute(ARegisters Registers, AMemory Memory)
        {
            if (!HasDelegate)
            {
                string Name = $"{Method.Name}_Dispatch";

                DynamicMethod Mthd = new DynamicMethod(Name, typeof(long), FixedArgTypes);

                ILGenerator Generator = Mthd.GetILGenerator();

                Generator.EmitLdargSeq(FixedArgTypes.Length);

                foreach (ARegister Reg in Params)
                {
                    Generator.EmitLdarg(RegistersArgIdx);

                    Generator.Emit(OpCodes.Ldfld, Reg.GetField());
                }

                Generator.Emit(OpCodes.Call, Method);
                Generator.Emit(OpCodes.Ret);

                ExecDelegate = (AA64Subroutine)Mthd.CreateDelegate(typeof(AA64Subroutine));

                HasDelegate = true;
            }

            return ExecDelegate(Registers, Memory);
        }

        public void MarkForReJit() => NeedsReJit = true;
    }
}