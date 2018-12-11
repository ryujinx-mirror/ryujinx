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
    class TranslatedSub
    {
        private delegate long Aa64Subroutine(CpuThreadState register, MemoryManager memory);

        private const int MinCallCountForReJit = 250;

        private Aa64Subroutine _execDelegate;

        public static int StateArgIdx  { get; private set; }
        public static int MemoryArgIdx { get; private set; }

        public static Type[] FixedArgTypes { get; private set; }

        public DynamicMethod Method { get; private set; }

        public ReadOnlyCollection<Register> SubArgs { get; private set; }

        private HashSet<long> _callers;

        private TranslatedSubType _type;

        private int _callCount;

        private bool _needsReJit;

        public TranslatedSub(DynamicMethod method, List<Register> subArgs)
        {
            Method  = method                ?? throw new ArgumentNullException(nameof(method));;
            SubArgs = subArgs?.AsReadOnly() ?? throw new ArgumentNullException(nameof(subArgs));

            _callers = new HashSet<long>();

            PrepareDelegate();
        }

        static TranslatedSub()
        {
            MethodInfo mthdInfo = typeof(Aa64Subroutine).GetMethod("Invoke");

            ParameterInfo[] Params = mthdInfo.GetParameters();

            FixedArgTypes = new Type[Params.Length];

            for (int index = 0; index < Params.Length; index++)
            {
                Type paramType = Params[index].ParameterType;

                FixedArgTypes[index] = paramType;

                if (paramType == typeof(CpuThreadState))
                {
                    StateArgIdx = index;
                }
                else if (paramType == typeof(MemoryManager))
                {
                    MemoryArgIdx = index;
                }
            }
        }

        private void PrepareDelegate()
        {
            string name = $"{Method.Name}_Dispatch";

            DynamicMethod mthd = new DynamicMethod(name, typeof(long), FixedArgTypes);

            ILGenerator generator = mthd.GetILGenerator();

            generator.EmitLdargSeq(FixedArgTypes.Length);

            foreach (Register reg in SubArgs)
            {
                generator.EmitLdarg(StateArgIdx);

                generator.Emit(OpCodes.Ldfld, reg.GetField());
            }

            generator.Emit(OpCodes.Call, Method);
            generator.Emit(OpCodes.Ret);

            _execDelegate = (Aa64Subroutine)mthd.CreateDelegate(typeof(Aa64Subroutine));
        }

        public bool ShouldReJit()
        {
            if (_needsReJit && _callCount < MinCallCountForReJit)
            {
                _callCount++;

                return false;
            }

            return _needsReJit;
        }

        public long Execute(CpuThreadState threadState, MemoryManager memory)
        {
            return _execDelegate(threadState, memory);
        }

        public void AddCaller(long position)
        {
            lock (_callers)
            {
                _callers.Add(position);
            }
        }

        public long[] GetCallerPositions()
        {
            lock (_callers)
            {
                return _callers.ToArray();
            }
        }

        public void SetType(TranslatedSubType type)
        {
            _type = type;

            if (type == TranslatedSubType.SubTier0)
            {
                _needsReJit = true;
            }
        }

        public void MarkForReJit() => _needsReJit = true;
    }
}