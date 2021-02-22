using ARMeilleure.Instructions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ARMeilleure.Translation
{
    static class Delegates
    {
        public static bool TryGetDelegateFuncPtrByIndex(int index, out IntPtr funcPtr)
        {
            if (index >= 0 && index < _delegates.Count)
            {
                funcPtr = _delegates.Values[index].FuncPtr; // O(1).

                return true;
            }
            else
            {
                funcPtr = default;

                return false;
            }
        }

        public static IntPtr GetDelegateFuncPtrByIndex(int index)
        {
            if (index < 0 || index >= _delegates.Count)
            {
                throw new ArgumentOutOfRangeException($"({nameof(index)} = {index})");
            }

            return _delegates.Values[index].FuncPtr; // O(1).
        }

        public static IntPtr GetDelegateFuncPtr(MethodInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            string key = GetKey(info);

            if (!_delegates.TryGetValue(key, out DelegateInfo dlgInfo)) // O(log(n)).
            {
                throw new KeyNotFoundException($"({nameof(key)} = {key})");
            }

            return dlgInfo.FuncPtr;
        }

        public static int GetDelegateIndex(MethodInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            string key = GetKey(info);

            int index = _delegates.IndexOfKey(key); // O(log(n)).

            if (index == -1)
            {
                throw new KeyNotFoundException($"({nameof(key)} = {key})");
            }

            return index;
        }

        private static void SetDelegateInfo(MethodInfo info)
        {
            string key = GetKey(info);

            Delegate dlg = DelegateHelper.GetDelegate(info);

            _delegates.Add(key, new DelegateInfo(dlg)); // ArgumentException (key).
        }

        private static string GetKey(MethodInfo info)
        {
            return $"{info.DeclaringType.Name}.{info.Name}";
        }

        private static readonly SortedList<string, DelegateInfo> _delegates;

        static Delegates()
        {
            _delegates = new SortedList<string, DelegateInfo>();

            SetDelegateInfo(typeof(Math).GetMethod(nameof(Math.Abs),      new Type[] { typeof(double) }));
            SetDelegateInfo(typeof(Math).GetMethod(nameof(Math.Ceiling),  new Type[] { typeof(double) }));
            SetDelegateInfo(typeof(Math).GetMethod(nameof(Math.Floor),    new Type[] { typeof(double) }));
            SetDelegateInfo(typeof(Math).GetMethod(nameof(Math.Round),    new Type[] { typeof(double), typeof(MidpointRounding) }));
            SetDelegateInfo(typeof(Math).GetMethod(nameof(Math.Truncate), new Type[] { typeof(double) }));

            SetDelegateInfo(typeof(MathF).GetMethod(nameof(MathF.Abs),      new Type[] { typeof(float) }));
            SetDelegateInfo(typeof(MathF).GetMethod(nameof(MathF.Ceiling),  new Type[] { typeof(float) }));
            SetDelegateInfo(typeof(MathF).GetMethod(nameof(MathF.Floor),    new Type[] { typeof(float) }));
            SetDelegateInfo(typeof(MathF).GetMethod(nameof(MathF.Round),    new Type[] { typeof(float), typeof(MidpointRounding) }));
            SetDelegateInfo(typeof(MathF).GetMethod(nameof(MathF.Truncate), new Type[] { typeof(float) }));

            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.Break)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.CheckSynchronization)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetCntfrqEl0)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetCntpctEl0)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetCntvctEl0)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetCtrEl0)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetDczidEl0)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetFpcr)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetFpcrFz)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetFpscr))); // A32 only.
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetFpsr)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetFunctionAddress)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetFunctionAddressWithoutRejit)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetIndirectFunctionAddress)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetTpidr)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetTpidr32))); // A32 only.
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetTpidrEl0)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetTpidrEl032))); // A32 only.
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadByte)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadUInt16)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadUInt32)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadUInt64)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.ReadVector128)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.SetFpcr)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.SetFpscr))); // A32 only.
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.SetFpsr)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.SetFpsrQc))); // A32 only.
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.SetTpidrEl0)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.SetTpidrEl032))); // A32 only.
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.SignalMemoryTracking)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.SupervisorCall)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.ThrowInvalidMemoryAccess)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.Undefined)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteByte)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteUInt16)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteUInt32)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteUInt64)));
            SetDelegateInfo(typeof(NativeInterface).GetMethod(nameof(NativeInterface.WriteVector128)));

            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.BinarySignedSatQAcc)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.BinarySignedSatQAdd)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.BinarySignedSatQSub)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.BinaryUnsignedSatQAcc)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.BinaryUnsignedSatQAdd)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.BinaryUnsignedSatQSub)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.CountLeadingSigns)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.CountLeadingZeros)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Crc32b)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Crc32cb)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Crc32ch)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Crc32cw)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Crc32cx)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Crc32h)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Crc32w)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Crc32x)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Decrypt)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.DoubleToInt32))); // A32 only.
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.DoubleToUInt32))); // A32 only.
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Encrypt)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.FixedRotate)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.FloatToInt32))); // A32 only.
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.FloatToUInt32))); // A32 only.
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashChoose)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashLower)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashMajority)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashParity)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashUpper)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.InverseMixColumns)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.MixColumns)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.PolynomialMult64_128)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Round)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.RoundF)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF32ToS32)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF32ToS64)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF32ToU32)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF32ToU64)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF64ToS32)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF64ToS64)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF64ToU32)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF64ToU64)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha1SchedulePart1)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha1SchedulePart2)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha256SchedulePart1)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha256SchedulePart2)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SignedShlReg)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SignedShlRegSatQ)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SignedShrImm64)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SignedSrcSignedDstSatQ)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SignedSrcUnsignedDstSatQ)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Tbl1)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Tbl2)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Tbl3)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Tbl4)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Tbx1)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Tbx2)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Tbx3)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Tbx4)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.UnarySignedSatQAbsOrNeg)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.UnsignedShlReg)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.UnsignedShlRegSatQ)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.UnsignedShrImm64)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.UnsignedSrcSignedDstSatQ)));
            SetDelegateInfo(typeof(SoftFallback).GetMethod(nameof(SoftFallback.UnsignedSrcUnsignedDstSatQ)));

            SetDelegateInfo(typeof(SoftFloat16_32).GetMethod(nameof(SoftFloat16_32.FPConvert)));

            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPAdd)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPAddFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompare)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompareEQ)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompareEQFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompareGE)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompareGEFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompareGT)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompareGTFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompareLE)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompareLEFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompareLT)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPCompareLTFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPDiv)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMax)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMaxFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMaxNum)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMaxNumFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMin)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMinFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMinNum)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMinNumFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMul)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMulFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMulAdd)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMulAddFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMulSub)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMulSubFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPMulX)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPNegMulAdd)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPNegMulSub)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPRecipEstimate)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPRecipEstimateFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPRecipStep))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPRecipStepFused)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPRecpX)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPRSqrtEstimate)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPRSqrtEstimateFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPRSqrtStep))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPRSqrtStepFused)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPSqrt)));
            SetDelegateInfo(typeof(SoftFloat32).GetMethod(nameof(SoftFloat32.FPSub)));

            SetDelegateInfo(typeof(SoftFloat32_16).GetMethod(nameof(SoftFloat32_16.FPConvert)));

            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPAdd)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPAddFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompare)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompareEQ)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompareEQFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompareGE)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompareGEFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompareGT)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompareGTFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompareLE)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompareLEFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompareLT)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPCompareLTFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPDiv)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMax)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMaxFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMaxNum)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMaxNumFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMin)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMinFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMinNum)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMinNumFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMul)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMulFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMulAdd)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMulAddFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMulSub)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMulSubFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPMulX)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPNegMulAdd)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPNegMulSub)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPRecipEstimate)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPRecipEstimateFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPRecipStep))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPRecipStepFused)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPRecpX)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPRSqrtEstimate)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPRSqrtEstimateFpscr))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPRSqrtStep))); // A32 only.
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPRSqrtStepFused)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPSqrt)));
            SetDelegateInfo(typeof(SoftFloat64).GetMethod(nameof(SoftFloat64.FPSub)));
        }
    }
}
