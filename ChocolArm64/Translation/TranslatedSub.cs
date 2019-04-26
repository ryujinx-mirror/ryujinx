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
        //This is the minimum amount of calls needed for the method
        //to be retranslated with higher quality code. It's only worth
        //doing that for hot code.
        private const int MinCallCountForOpt = 30;

        public ArmSubroutine Delegate { get; private set; }

        public static int StateArgIdx  { get; }
        public static int MemoryArgIdx { get; }

        public static Type[] FixedArgTypes { get; }

        public DynamicMethod Method { get; }

        public TranslationTier Tier { get; }

        private bool _rejit;

        private int _callCount;

        public TranslatedSub(DynamicMethod method, TranslationTier tier, bool rejit)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));;
            Tier   = tier;
            _rejit = rejit;
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

        public bool Rejit()
        {
           if (!_rejit)
            {
                return false;
            }

            if (_callCount++ < MinCallCountForOpt)
            {
                return false;
            }

            //Only return true once, so that it is added to the queue only once.
            _rejit = false;

            return true;
        }
    }
}