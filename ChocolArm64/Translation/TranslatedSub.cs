using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    delegate long ArmSubroutine(CpuThreadState state, MemoryManager memory);

    class TranslatedSub
    {
        public ArmSubroutine Delegate { get; private set; }

        public static int StateArgIdx  { get; private set; }
        public static int MemoryArgIdx { get; private set; }

        public static Type[] FixedArgTypes { get; private set; }

        public DynamicMethod Method { get; private set; }

        public TranslationTier Tier { get; private set; }

        public TranslatedSub(DynamicMethod method, TranslationTier tier)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));;
            Tier   = tier;
        }

        static TranslatedSub()
        {
            MethodInfo mthdInfo = typeof(ArmSubroutine).GetMethod("Invoke");

            ParameterInfo[] Params = mthdInfo.GetParameters();

            FixedArgTypes = new Type[Params.Length];

            for (int index = 0; index < Params.Length; index++)
            {
                Type argType = Params[index].ParameterType;

                FixedArgTypes[index] = argType;

                if (argType == typeof(CpuThreadState))
                {
                    StateArgIdx = index;
                }
                else if (argType == typeof(MemoryManager))
                {
                    MemoryArgIdx = index;
                }
            }
        }

        public void PrepareMethod()
        {
            Delegate = (ArmSubroutine)Method.CreateDelegate(typeof(ArmSubroutine));
        }

        public long Execute(CpuThreadState threadState, MemoryManager memory)
        {
            return Delegate(threadState, memory);
        }
    }
}