using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64
{
    class ATranslatedSub
    {
        private delegate long AA64Subroutine(AThreadState Register, AMemory Memory);

        private const int MinCallCountForReJit = 250;

        private AA64Subroutine ExecDelegate;

        public static int StateArgIdx  { get; private set; }
        public static int MemoryArgIdx { get; private set; }

        public static Type[] FixedArgTypes { get; private set; }

        public DynamicMethod Method { get; private set; }

        public ReadOnlyCollection<ARegister> Params { get; private set; }

        private HashSet<long> Callers;

        private ATranslatedSubType Type;

        private int CallCount;

        private bool NeedsReJit;

        public ATranslatedSub(DynamicMethod Method, List<ARegister> Params)
        {
            if (Method == null)
            {
                throw new ArgumentNullException(nameof(Method));
            }

            if (Params == null)
            {
                throw new ArgumentNullException(nameof(Params));
            }

            this.Method = Method;
            this.Params = Params.AsReadOnly();

            Callers = new HashSet<long>();

            PrepareDelegate();
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

                if (ParamType == typeof(AThreadState))
                {
                    StateArgIdx = Index;
                }
                else if (ParamType == typeof(AMemory))
                {
                    MemoryArgIdx = Index;
                }
            }
        }

        private void PrepareDelegate()
        {
            string Name = $"{Method.Name}_Dispatch";

            DynamicMethod Mthd = new DynamicMethod(Name, typeof(long), FixedArgTypes);

            ILGenerator Generator = Mthd.GetILGenerator();

            Generator.EmitLdargSeq(FixedArgTypes.Length);

            foreach (ARegister Reg in Params)
            {
                Generator.EmitLdarg(StateArgIdx);

                Generator.Emit(OpCodes.Ldfld, Reg.GetField());
            }

            Generator.Emit(OpCodes.Call, Method);
            Generator.Emit(OpCodes.Ret);

            ExecDelegate = (AA64Subroutine)Mthd.CreateDelegate(typeof(AA64Subroutine));
        }

        public bool ShouldReJit()
        {
            if (NeedsReJit && CallCount < MinCallCountForReJit)
            {
                CallCount++;

                return false;
            }

            return NeedsReJit;
        }

        public long Execute(AThreadState ThreadState, AMemory Memory)
        {
            return ExecDelegate(ThreadState, Memory);
        }

        public void AddCaller(long Position)
        {
            lock (Callers)
            {
                Callers.Add(Position);
            }
        }

        public long[] GetCallerPositions()
        {
            lock (Callers)
            {
                return Callers.ToArray();
            }
        }

        public void SetType(ATranslatedSubType Type)
        {
            this.Type = Type;

            if (Type == ATranslatedSubType.SubTier0)
            {
                NeedsReJit = true;
            }
        }

        public void MarkForReJit() => NeedsReJit = true;
    }
}