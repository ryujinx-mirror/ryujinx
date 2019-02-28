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

        public long IntNiRegsMask { get; }
        public long VecNiRegsMask { get; }

        private bool _isWorthOptimizing;

        private int _callCount;

        public TranslatedSub(
            DynamicMethod   method,
            long            intNiRegsMask,
            long            vecNiRegsMask,
            TranslationTier tier,
            bool            isWorthOptimizing)
        {
            Method             = method ?? throw new ArgumentNullException(nameof(method));;
            IntNiRegsMask      = intNiRegsMask;
            VecNiRegsMask      = vecNiRegsMask;
            _isWorthOptimizing = isWorthOptimizing;
            Tier               = tier;
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

        public bool IsWorthOptimizing()
        {
           if (!_isWorthOptimizing)
            {
                return false;
            }

            if (_callCount++ < MinCallCountForOpt)
            {
                return false;
            }

            //Only return true once, so that it is
            //added to the queue only once.
            _isWorthOptimizing = false;

            return true;
        }
    }
}