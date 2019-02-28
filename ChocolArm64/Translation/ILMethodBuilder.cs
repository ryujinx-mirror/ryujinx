using ChocolArm64.State;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;

namespace ChocolArm64.Translation
{
    class ILMethodBuilder
    {
        private const int RegsCount = 32;
        private const int RegsMask  = RegsCount - 1;

        public RegisterUsage RegUsage { get; private set; }

        public ILGenerator Generator { get; private set; }

        private Dictionary<Register, int> _locals;

        private ILBlock[] _ilBlocks;

        private string _subName;

        public bool IsAarch64 { get; }

        public bool IsSubComplete { get; }

        private int _localsCount;

        public ILMethodBuilder(
            ILBlock[] ilBlocks,
            string    subName,
            bool      isAarch64,
            bool      isSubComplete = false)
        {
            _ilBlocks     = ilBlocks;
            _subName      = subName;
            IsAarch64     = isAarch64;
            IsSubComplete = isSubComplete;
        }

        public TranslatedSub GetSubroutine(TranslationTier tier, bool isWorthOptimizing)
        {
            RegUsage = new RegisterUsage();

            RegUsage.BuildUses(_ilBlocks[0]);

            DynamicMethod method = new DynamicMethod(_subName, typeof(long), TranslatedSub.FixedArgTypes);

            long intNiRegsMask = RegUsage.GetIntNotInputs(_ilBlocks[0]);
            long vecNiRegsMask = RegUsage.GetVecNotInputs(_ilBlocks[0]);

            TranslatedSub subroutine = new TranslatedSub(
                method,
                intNiRegsMask,
                vecNiRegsMask,
                tier,
                isWorthOptimizing);

            _locals = new Dictionary<Register, int>();

            _localsCount = 0;

            Generator = method.GetILGenerator();

            foreach (ILBlock ilBlock in _ilBlocks)
            {
                ilBlock.Emit(this);
            }

            subroutine.PrepareMethod();

            return subroutine;
        }

        public int GetLocalIndex(Register reg)
        {
            if (!_locals.TryGetValue(reg, out int index))
            {
                Generator.DeclareLocal(GetFieldType(reg.Type));

                index = _localsCount++;

                _locals.Add(reg, index);
            }

            return index;
        }

        private static Type GetFieldType(RegisterType regType)
        {
            switch (regType)
            {
                case RegisterType.Flag:   return typeof(bool);
                case RegisterType.Int:    return typeof(ulong);
                case RegisterType.Vector: return typeof(Vector128<float>);
            }

            throw new ArgumentException(nameof(regType));
        }

        public static Register GetRegFromBit(int bit, RegisterType baseType)
        {
            if (bit < RegsCount)
            {
                return new Register(bit, baseType);
            }
            else if (baseType == RegisterType.Int)
            {
                return new Register(bit & RegsMask, RegisterType.Flag);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(bit));
            }
        }

        public static bool IsRegIndex(int index)
        {
            return (uint)index < RegsCount;
        }
    }
}