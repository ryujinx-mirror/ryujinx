using ARMeilleure.Instructions;
using ARMeilleure.State;
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
            ArgumentNullException.ThrowIfNull(info);

            string key = GetKey(info);

            if (!_delegates.TryGetValue(key, out DelegateInfo dlgInfo)) // O(log(n)).
            {
                throw new KeyNotFoundException($"({nameof(key)} = {key})");
            }

            return dlgInfo.FuncPtr;
        }

        public static int GetDelegateIndex(MethodInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            string key = GetKey(info);

            int index = _delegates.IndexOfKey(key); // O(log(n)).

            if (index == -1)
            {
                throw new KeyNotFoundException($"({nameof(key)} = {key})");
            }

            return index;
        }

        private static void SetDelegateInfo(Delegate dlg)
        {
            string key = GetKey(dlg.Method);

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

            SetDelegateInfo(new MathAbs(Math.Abs));
            SetDelegateInfo(new MathCeiling(Math.Ceiling));
            SetDelegateInfo(new MathFloor(Math.Floor));
            SetDelegateInfo(new MathRound(Math.Round));
            SetDelegateInfo(new MathTruncate(Math.Truncate));

            SetDelegateInfo(new MathFAbs(MathF.Abs));
            SetDelegateInfo(new MathFCeiling(MathF.Ceiling));
            SetDelegateInfo(new MathFFloor(MathF.Floor));
            SetDelegateInfo(new MathFRound(MathF.Round));
            SetDelegateInfo(new MathFTruncate(MathF.Truncate));

            SetDelegateInfo(new NativeInterfaceBreak(NativeInterface.Break));
            SetDelegateInfo(new NativeInterfaceCheckSynchronization(NativeInterface.CheckSynchronization));
            SetDelegateInfo(new NativeInterfaceEnqueueForRejit(NativeInterface.EnqueueForRejit));
            SetDelegateInfo(new NativeInterfaceGetCntfrqEl0(NativeInterface.GetCntfrqEl0));
            SetDelegateInfo(new NativeInterfaceGetCntpctEl0(NativeInterface.GetCntpctEl0));
            SetDelegateInfo(new NativeInterfaceGetCntvctEl0(NativeInterface.GetCntvctEl0));
            SetDelegateInfo(new NativeInterfaceGetCtrEl0(NativeInterface.GetCtrEl0));
            SetDelegateInfo(new NativeInterfaceGetDczidEl0(NativeInterface.GetDczidEl0));
            SetDelegateInfo(new NativeInterfaceGetFunctionAddress(NativeInterface.GetFunctionAddress));
            SetDelegateInfo(new NativeInterfaceInvalidateCacheLine(NativeInterface.InvalidateCacheLine));
            SetDelegateInfo(new NativeInterfaceReadByte(NativeInterface.ReadByte));
            SetDelegateInfo(new NativeInterfaceReadUInt16(NativeInterface.ReadUInt16));
            SetDelegateInfo(new NativeInterfaceReadUInt32(NativeInterface.ReadUInt32));
            SetDelegateInfo(new NativeInterfaceReadUInt64(NativeInterface.ReadUInt64));
            SetDelegateInfo(new NativeInterfaceReadVector128(NativeInterface.ReadVector128));
            SetDelegateInfo(new NativeInterfaceSignalMemoryTracking(NativeInterface.SignalMemoryTracking));
            SetDelegateInfo(new NativeInterfaceSupervisorCall(NativeInterface.SupervisorCall));
            SetDelegateInfo(new NativeInterfaceThrowInvalidMemoryAccess(NativeInterface.ThrowInvalidMemoryAccess));
            SetDelegateInfo(new NativeInterfaceUndefined(NativeInterface.Undefined));
            SetDelegateInfo(new NativeInterfaceWriteByte(NativeInterface.WriteByte));
            SetDelegateInfo(new NativeInterfaceWriteUInt16(NativeInterface.WriteUInt16));
            SetDelegateInfo(new NativeInterfaceWriteUInt32(NativeInterface.WriteUInt32));
            SetDelegateInfo(new NativeInterfaceWriteUInt64(NativeInterface.WriteUInt64));
            SetDelegateInfo(new NativeInterfaceWriteVector128(NativeInterface.WriteVector128));

            SetDelegateInfo(new SoftFallbackCountLeadingSigns(SoftFallback.CountLeadingSigns));
            SetDelegateInfo(new SoftFallbackCountLeadingZeros(SoftFallback.CountLeadingZeros));
            SetDelegateInfo(new SoftFallbackCrc32b(SoftFallback.Crc32b));
            SetDelegateInfo(new SoftFallbackCrc32cb(SoftFallback.Crc32cb));
            SetDelegateInfo(new SoftFallbackCrc32ch(SoftFallback.Crc32ch));
            SetDelegateInfo(new SoftFallbackCrc32cw(SoftFallback.Crc32cw));
            SetDelegateInfo(new SoftFallbackCrc32cx(SoftFallback.Crc32cx));
            SetDelegateInfo(new SoftFallbackCrc32h(SoftFallback.Crc32h));
            SetDelegateInfo(new SoftFallbackCrc32w(SoftFallback.Crc32w));
            SetDelegateInfo(new SoftFallbackCrc32x(SoftFallback.Crc32x));
            SetDelegateInfo(new SoftFallbackDecrypt(SoftFallback.Decrypt));
            SetDelegateInfo(new SoftFallbackEncrypt(SoftFallback.Encrypt));
            SetDelegateInfo(new SoftFallbackFixedRotate(SoftFallback.FixedRotate));
            SetDelegateInfo(new SoftFallbackHashChoose(SoftFallback.HashChoose));
            SetDelegateInfo(new SoftFallbackHashLower(SoftFallback.HashLower));
            SetDelegateInfo(new SoftFallbackHashMajority(SoftFallback.HashMajority));
            SetDelegateInfo(new SoftFallbackHashParity(SoftFallback.HashParity));
            SetDelegateInfo(new SoftFallbackHashUpper(SoftFallback.HashUpper));
            SetDelegateInfo(new SoftFallbackInverseMixColumns(SoftFallback.InverseMixColumns));
            SetDelegateInfo(new SoftFallbackMixColumns(SoftFallback.MixColumns));
            SetDelegateInfo(new SoftFallbackPolynomialMult64_128(SoftFallback.PolynomialMult64_128));
            SetDelegateInfo(new SoftFallbackSatF32ToS32(SoftFallback.SatF32ToS32));
            SetDelegateInfo(new SoftFallbackSatF32ToS64(SoftFallback.SatF32ToS64));
            SetDelegateInfo(new SoftFallbackSatF32ToU32(SoftFallback.SatF32ToU32));
            SetDelegateInfo(new SoftFallbackSatF32ToU64(SoftFallback.SatF32ToU64));
            SetDelegateInfo(new SoftFallbackSatF64ToS32(SoftFallback.SatF64ToS32));
            SetDelegateInfo(new SoftFallbackSatF64ToS64(SoftFallback.SatF64ToS64));
            SetDelegateInfo(new SoftFallbackSatF64ToU32(SoftFallback.SatF64ToU32));
            SetDelegateInfo(new SoftFallbackSatF64ToU64(SoftFallback.SatF64ToU64));
            SetDelegateInfo(new SoftFallbackSha1SchedulePart1(SoftFallback.Sha1SchedulePart1));
            SetDelegateInfo(new SoftFallbackSha1SchedulePart2(SoftFallback.Sha1SchedulePart2));
            SetDelegateInfo(new SoftFallbackSha256SchedulePart1(SoftFallback.Sha256SchedulePart1));
            SetDelegateInfo(new SoftFallbackSha256SchedulePart2(SoftFallback.Sha256SchedulePart2));
            SetDelegateInfo(new SoftFallbackSignedShrImm64(SoftFallback.SignedShrImm64));
            SetDelegateInfo(new SoftFallbackTbl1(SoftFallback.Tbl1));
            SetDelegateInfo(new SoftFallbackTbl2(SoftFallback.Tbl2));
            SetDelegateInfo(new SoftFallbackTbl3(SoftFallback.Tbl3));
            SetDelegateInfo(new SoftFallbackTbl4(SoftFallback.Tbl4));
            SetDelegateInfo(new SoftFallbackTbx1(SoftFallback.Tbx1));
            SetDelegateInfo(new SoftFallbackTbx2(SoftFallback.Tbx2));
            SetDelegateInfo(new SoftFallbackTbx3(SoftFallback.Tbx3));
            SetDelegateInfo(new SoftFallbackTbx4(SoftFallback.Tbx4));
            SetDelegateInfo(new SoftFallbackUnsignedShrImm64(SoftFallback.UnsignedShrImm64));

            SetDelegateInfo(new SoftFloat16_32FPConvert(SoftFloat16_32.FPConvert));
            SetDelegateInfo(new SoftFloat16_64FPConvert(SoftFloat16_64.FPConvert));

            SetDelegateInfo(new SoftFloat32FPAdd(SoftFloat32.FPAdd));
            SetDelegateInfo(new SoftFloat32FPAddFpscr(SoftFloat32.FPAddFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPCompare(SoftFloat32.FPCompare));
            SetDelegateInfo(new SoftFloat32FPCompareEQ(SoftFloat32.FPCompareEQ));
            SetDelegateInfo(new SoftFloat32FPCompareEQFpscr(SoftFloat32.FPCompareEQFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPCompareGE(SoftFloat32.FPCompareGE));
            SetDelegateInfo(new SoftFloat32FPCompareGEFpscr(SoftFloat32.FPCompareGEFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPCompareGT(SoftFloat32.FPCompareGT));
            SetDelegateInfo(new SoftFloat32FPCompareGTFpscr(SoftFloat32.FPCompareGTFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPCompareLE(SoftFloat32.FPCompareLE));
            SetDelegateInfo(new SoftFloat32FPCompareLEFpscr(SoftFloat32.FPCompareLEFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPCompareLT(SoftFloat32.FPCompareLT));
            SetDelegateInfo(new SoftFloat32FPCompareLTFpscr(SoftFloat32.FPCompareLTFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPDiv(SoftFloat32.FPDiv));
            SetDelegateInfo(new SoftFloat32FPMax(SoftFloat32.FPMax));
            SetDelegateInfo(new SoftFloat32FPMaxFpscr(SoftFloat32.FPMaxFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPMaxNum(SoftFloat32.FPMaxNum));
            SetDelegateInfo(new SoftFloat32FPMaxNumFpscr(SoftFloat32.FPMaxNumFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPMin(SoftFloat32.FPMin));
            SetDelegateInfo(new SoftFloat32FPMinFpscr(SoftFloat32.FPMinFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPMinNum(SoftFloat32.FPMinNum));
            SetDelegateInfo(new SoftFloat32FPMinNumFpscr(SoftFloat32.FPMinNumFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPMul(SoftFloat32.FPMul));
            SetDelegateInfo(new SoftFloat32FPMulFpscr(SoftFloat32.FPMulFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPMulAdd(SoftFloat32.FPMulAdd));
            SetDelegateInfo(new SoftFloat32FPMulAddFpscr(SoftFloat32.FPMulAddFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPMulSub(SoftFloat32.FPMulSub));
            SetDelegateInfo(new SoftFloat32FPMulSubFpscr(SoftFloat32.FPMulSubFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPMulX(SoftFloat32.FPMulX));
            SetDelegateInfo(new SoftFloat32FPNegMulAdd(SoftFloat32.FPNegMulAdd));
            SetDelegateInfo(new SoftFloat32FPNegMulSub(SoftFloat32.FPNegMulSub));
            SetDelegateInfo(new SoftFloat32FPRecipEstimate(SoftFloat32.FPRecipEstimate));
            SetDelegateInfo(new SoftFloat32FPRecipEstimateFpscr(SoftFloat32.FPRecipEstimateFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPRecipStep(SoftFloat32.FPRecipStep)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPRecipStepFused(SoftFloat32.FPRecipStepFused));
            SetDelegateInfo(new SoftFloat32FPRecpX(SoftFloat32.FPRecpX));
            SetDelegateInfo(new SoftFloat32FPRSqrtEstimate(SoftFloat32.FPRSqrtEstimate));
            SetDelegateInfo(new SoftFloat32FPRSqrtEstimateFpscr(SoftFloat32.FPRSqrtEstimateFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPRSqrtStep(SoftFloat32.FPRSqrtStep)); // A32 only.
            SetDelegateInfo(new SoftFloat32FPRSqrtStepFused(SoftFloat32.FPRSqrtStepFused));
            SetDelegateInfo(new SoftFloat32FPSqrt(SoftFloat32.FPSqrt));
            SetDelegateInfo(new SoftFloat32FPSub(SoftFloat32.FPSub));

            SetDelegateInfo(new SoftFloat32_16FPConvert(SoftFloat32_16.FPConvert));

            SetDelegateInfo(new SoftFloat64FPAdd(SoftFloat64.FPAdd));
            SetDelegateInfo(new SoftFloat64FPAddFpscr(SoftFloat64.FPAddFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPCompare(SoftFloat64.FPCompare));
            SetDelegateInfo(new SoftFloat64FPCompareEQ(SoftFloat64.FPCompareEQ));
            SetDelegateInfo(new SoftFloat64FPCompareEQFpscr(SoftFloat64.FPCompareEQFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPCompareGE(SoftFloat64.FPCompareGE));
            SetDelegateInfo(new SoftFloat64FPCompareGEFpscr(SoftFloat64.FPCompareGEFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPCompareGT(SoftFloat64.FPCompareGT));
            SetDelegateInfo(new SoftFloat64FPCompareGTFpscr(SoftFloat64.FPCompareGTFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPCompareLE(SoftFloat64.FPCompareLE));
            SetDelegateInfo(new SoftFloat64FPCompareLEFpscr(SoftFloat64.FPCompareLEFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPCompareLT(SoftFloat64.FPCompareLT));
            SetDelegateInfo(new SoftFloat64FPCompareLTFpscr(SoftFloat64.FPCompareLTFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPDiv(SoftFloat64.FPDiv));
            SetDelegateInfo(new SoftFloat64FPMax(SoftFloat64.FPMax));
            SetDelegateInfo(new SoftFloat64FPMaxFpscr(SoftFloat64.FPMaxFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPMaxNum(SoftFloat64.FPMaxNum));
            SetDelegateInfo(new SoftFloat64FPMaxNumFpscr(SoftFloat64.FPMaxNumFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPMin(SoftFloat64.FPMin));
            SetDelegateInfo(new SoftFloat64FPMinFpscr(SoftFloat64.FPMinFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPMinNum(SoftFloat64.FPMinNum));
            SetDelegateInfo(new SoftFloat64FPMinNumFpscr(SoftFloat64.FPMinNumFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPMul(SoftFloat64.FPMul));
            SetDelegateInfo(new SoftFloat64FPMulFpscr(SoftFloat64.FPMulFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPMulAdd(SoftFloat64.FPMulAdd));
            SetDelegateInfo(new SoftFloat64FPMulAddFpscr(SoftFloat64.FPMulAddFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPMulSub(SoftFloat64.FPMulSub));
            SetDelegateInfo(new SoftFloat64FPMulSubFpscr(SoftFloat64.FPMulSubFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPMulX(SoftFloat64.FPMulX));
            SetDelegateInfo(new SoftFloat64FPNegMulAdd(SoftFloat64.FPNegMulAdd));
            SetDelegateInfo(new SoftFloat64FPNegMulSub(SoftFloat64.FPNegMulSub));
            SetDelegateInfo(new SoftFloat64FPRecipEstimate(SoftFloat64.FPRecipEstimate));
            SetDelegateInfo(new SoftFloat64FPRecipEstimateFpscr(SoftFloat64.FPRecipEstimateFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPRecipStep(SoftFloat64.FPRecipStep)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPRecipStepFused(SoftFloat64.FPRecipStepFused));
            SetDelegateInfo(new SoftFloat64FPRecpX(SoftFloat64.FPRecpX));
            SetDelegateInfo(new SoftFloat64FPRSqrtEstimate(SoftFloat64.FPRSqrtEstimate));
            SetDelegateInfo(new SoftFloat64FPRSqrtEstimateFpscr(SoftFloat64.FPRSqrtEstimateFpscr)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPRSqrtStep(SoftFloat64.FPRSqrtStep)); // A32 only.
            SetDelegateInfo(new SoftFloat64FPRSqrtStepFused(SoftFloat64.FPRSqrtStepFused));
            SetDelegateInfo(new SoftFloat64FPSqrt(SoftFloat64.FPSqrt));
            SetDelegateInfo(new SoftFloat64FPSub(SoftFloat64.FPSub));

            SetDelegateInfo(new SoftFloat64_16FPConvert(SoftFloat64_16.FPConvert));
        }

        private delegate double MathAbs(double value);
        private delegate double MathCeiling(double a);
        private delegate double MathFloor(double d);
        private delegate double MathRound(double value, MidpointRounding mode);
        private delegate double MathTruncate(double d);

        private delegate float MathFAbs(float x);
        private delegate float MathFCeiling(float x);
        private delegate float MathFFloor(float x);
        private delegate float MathFRound(float x, MidpointRounding mode);
        private delegate float MathFTruncate(float x);

        private delegate void NativeInterfaceBreak(ulong address, int imm);
        private delegate bool NativeInterfaceCheckSynchronization();
        private delegate void NativeInterfaceEnqueueForRejit(ulong address);
        private delegate ulong NativeInterfaceGetCntfrqEl0();
        private delegate ulong NativeInterfaceGetCntpctEl0();
        private delegate ulong NativeInterfaceGetCntvctEl0();
        private delegate ulong NativeInterfaceGetCtrEl0();
        private delegate ulong NativeInterfaceGetDczidEl0();
        private delegate ulong NativeInterfaceGetFunctionAddress(ulong address);
        private delegate void NativeInterfaceInvalidateCacheLine(ulong address);
        private delegate byte NativeInterfaceReadByte(ulong address);
        private delegate ushort NativeInterfaceReadUInt16(ulong address);
        private delegate uint NativeInterfaceReadUInt32(ulong address);
        private delegate ulong NativeInterfaceReadUInt64(ulong address);
        private delegate V128 NativeInterfaceReadVector128(ulong address);
        private delegate void NativeInterfaceSignalMemoryTracking(ulong address, ulong size, bool write);
        private delegate void NativeInterfaceSupervisorCall(ulong address, int imm);
        private delegate void NativeInterfaceThrowInvalidMemoryAccess(ulong address);
        private delegate void NativeInterfaceUndefined(ulong address, int opCode);
        private delegate void NativeInterfaceWriteByte(ulong address, byte value);
        private delegate void NativeInterfaceWriteUInt16(ulong address, ushort value);
        private delegate void NativeInterfaceWriteUInt32(ulong address, uint value);
        private delegate void NativeInterfaceWriteUInt64(ulong address, ulong value);
        private delegate void NativeInterfaceWriteVector128(ulong address, V128 value);

        private delegate ulong SoftFallbackCountLeadingSigns(ulong value, int size);
        private delegate ulong SoftFallbackCountLeadingZeros(ulong value, int size);
        private delegate uint SoftFallbackCrc32b(uint crc, byte value);
        private delegate uint SoftFallbackCrc32cb(uint crc, byte value);
        private delegate uint SoftFallbackCrc32ch(uint crc, ushort value);
        private delegate uint SoftFallbackCrc32cw(uint crc, uint value);
        private delegate uint SoftFallbackCrc32cx(uint crc, ulong value);
        private delegate uint SoftFallbackCrc32h(uint crc, ushort value);
        private delegate uint SoftFallbackCrc32w(uint crc, uint value);
        private delegate uint SoftFallbackCrc32x(uint crc, ulong value);
        private delegate V128 SoftFallbackDecrypt(V128 value, V128 roundKey);
        private delegate V128 SoftFallbackEncrypt(V128 value, V128 roundKey);
        private delegate uint SoftFallbackFixedRotate(uint hash_e);
        private delegate V128 SoftFallbackHashChoose(V128 hash_abcd, uint hash_e, V128 wk);
        private delegate V128 SoftFallbackHashLower(V128 hash_abcd, V128 hash_efgh, V128 wk);
        private delegate V128 SoftFallbackHashMajority(V128 hash_abcd, uint hash_e, V128 wk);
        private delegate V128 SoftFallbackHashParity(V128 hash_abcd, uint hash_e, V128 wk);
        private delegate V128 SoftFallbackHashUpper(V128 hash_abcd, V128 hash_efgh, V128 wk);
        private delegate V128 SoftFallbackInverseMixColumns(V128 value);
        private delegate V128 SoftFallbackMixColumns(V128 value);
        private delegate V128 SoftFallbackPolynomialMult64_128(ulong op1, ulong op2);
        private delegate int SoftFallbackSatF32ToS32(float value);
        private delegate long SoftFallbackSatF32ToS64(float value);
        private delegate uint SoftFallbackSatF32ToU32(float value);
        private delegate ulong SoftFallbackSatF32ToU64(float value);
        private delegate int SoftFallbackSatF64ToS32(double value);
        private delegate long SoftFallbackSatF64ToS64(double value);
        private delegate uint SoftFallbackSatF64ToU32(double value);
        private delegate ulong SoftFallbackSatF64ToU64(double value);
        private delegate V128 SoftFallbackSha1SchedulePart1(V128 w0_3, V128 w4_7, V128 w8_11);
        private delegate V128 SoftFallbackSha1SchedulePart2(V128 tw0_3, V128 w12_15);
        private delegate V128 SoftFallbackSha256SchedulePart1(V128 w0_3, V128 w4_7);
        private delegate V128 SoftFallbackSha256SchedulePart2(V128 w0_3, V128 w8_11, V128 w12_15);
        private delegate long SoftFallbackSignedShrImm64(long value, long roundConst, int shift);
        private delegate V128 SoftFallbackTbl1(V128 vector, int bytes, V128 tb0);
        private delegate V128 SoftFallbackTbl2(V128 vector, int bytes, V128 tb0, V128 tb1);
        private delegate V128 SoftFallbackTbl3(V128 vector, int bytes, V128 tb0, V128 tb1, V128 tb2);
        private delegate V128 SoftFallbackTbl4(V128 vector, int bytes, V128 tb0, V128 tb1, V128 tb2, V128 tb3);
        private delegate V128 SoftFallbackTbx1(V128 dest, V128 vector, int bytes, V128 tb0);
        private delegate V128 SoftFallbackTbx2(V128 dest, V128 vector, int bytes, V128 tb0, V128 tb1);
        private delegate V128 SoftFallbackTbx3(V128 dest, V128 vector, int bytes, V128 tb0, V128 tb1, V128 tb2);
        private delegate V128 SoftFallbackTbx4(V128 dest, V128 vector, int bytes, V128 tb0, V128 tb1, V128 tb2, V128 tb3);
        private delegate ulong SoftFallbackUnsignedShrImm64(ulong value, long roundConst, int shift);

        private delegate float SoftFloat16_32FPConvert(ushort valueBits);

        private delegate double SoftFloat16_64FPConvert(ushort valueBits);

        private delegate float SoftFloat32FPAdd(float value1, float value2);
        private delegate float SoftFloat32FPAddFpscr(float value1, float value2, bool standardFpscr);
        private delegate int SoftFloat32FPCompare(float value1, float value2, bool signalNaNs);
        private delegate float SoftFloat32FPCompareEQ(float value1, float value2);
        private delegate float SoftFloat32FPCompareEQFpscr(float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPCompareGE(float value1, float value2);
        private delegate float SoftFloat32FPCompareGEFpscr(float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPCompareGT(float value1, float value2);
        private delegate float SoftFloat32FPCompareGTFpscr(float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPCompareLE(float value1, float value2);
        private delegate float SoftFloat32FPCompareLEFpscr(float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPCompareLT(float value1, float value2);
        private delegate float SoftFloat32FPCompareLTFpscr(float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPDiv(float value1, float value2);
        private delegate float SoftFloat32FPMax(float value1, float value2);
        private delegate float SoftFloat32FPMaxFpscr(float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPMaxNum(float value1, float value2);
        private delegate float SoftFloat32FPMaxNumFpscr(float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPMin(float value1, float value2);
        private delegate float SoftFloat32FPMinFpscr(float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPMinNum(float value1, float value2);
        private delegate float SoftFloat32FPMinNumFpscr(float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPMul(float value1, float value2);
        private delegate float SoftFloat32FPMulFpscr(float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPMulAdd(float valueA, float value1, float value2);
        private delegate float SoftFloat32FPMulAddFpscr(float valueA, float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPMulSub(float valueA, float value1, float value2);
        private delegate float SoftFloat32FPMulSubFpscr(float valueA, float value1, float value2, bool standardFpscr);
        private delegate float SoftFloat32FPMulX(float value1, float value2);
        private delegate float SoftFloat32FPNegMulAdd(float valueA, float value1, float value2);
        private delegate float SoftFloat32FPNegMulSub(float valueA, float value1, float value2);
        private delegate float SoftFloat32FPRecipEstimate(float value);
        private delegate float SoftFloat32FPRecipEstimateFpscr(float value, bool standardFpscr);
        private delegate float SoftFloat32FPRecipStep(float value1, float value2);
        private delegate float SoftFloat32FPRecipStepFused(float value1, float value2);
        private delegate float SoftFloat32FPRecpX(float value);
        private delegate float SoftFloat32FPRSqrtEstimate(float value);
        private delegate float SoftFloat32FPRSqrtEstimateFpscr(float value, bool standardFpscr);
        private delegate float SoftFloat32FPRSqrtStep(float value1, float value2);
        private delegate float SoftFloat32FPRSqrtStepFused(float value1, float value2);
        private delegate float SoftFloat32FPSqrt(float value);
        private delegate float SoftFloat32FPSub(float value1, float value2);

        private delegate ushort SoftFloat32_16FPConvert(float value);

        private delegate double SoftFloat64FPAdd(double value1, double value2);
        private delegate double SoftFloat64FPAddFpscr(double value1, double value2, bool standardFpscr);
        private delegate int SoftFloat64FPCompare(double value1, double value2, bool signalNaNs);
        private delegate double SoftFloat64FPCompareEQ(double value1, double value2);
        private delegate double SoftFloat64FPCompareEQFpscr(double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPCompareGE(double value1, double value2);
        private delegate double SoftFloat64FPCompareGEFpscr(double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPCompareGT(double value1, double value2);
        private delegate double SoftFloat64FPCompareGTFpscr(double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPCompareLE(double value1, double value2);
        private delegate double SoftFloat64FPCompareLEFpscr(double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPCompareLT(double value1, double value2);
        private delegate double SoftFloat64FPCompareLTFpscr(double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPDiv(double value1, double value2);
        private delegate double SoftFloat64FPMax(double value1, double value2);
        private delegate double SoftFloat64FPMaxFpscr(double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPMaxNum(double value1, double value2);
        private delegate double SoftFloat64FPMaxNumFpscr(double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPMin(double value1, double value2);
        private delegate double SoftFloat64FPMinFpscr(double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPMinNum(double value1, double value2);
        private delegate double SoftFloat64FPMinNumFpscr(double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPMul(double value1, double value2);
        private delegate double SoftFloat64FPMulFpscr(double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPMulAdd(double valueA, double value1, double value2);
        private delegate double SoftFloat64FPMulAddFpscr(double valueA, double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPMulSub(double valueA, double value1, double value2);
        private delegate double SoftFloat64FPMulSubFpscr(double valueA, double value1, double value2, bool standardFpscr);
        private delegate double SoftFloat64FPMulX(double value1, double value2);
        private delegate double SoftFloat64FPNegMulAdd(double valueA, double value1, double value2);
        private delegate double SoftFloat64FPNegMulSub(double valueA, double value1, double value2);
        private delegate double SoftFloat64FPRecipEstimate(double value);
        private delegate double SoftFloat64FPRecipEstimateFpscr(double value, bool standardFpscr);
        private delegate double SoftFloat64FPRecipStep(double value1, double value2);
        private delegate double SoftFloat64FPRecipStepFused(double value1, double value2);
        private delegate double SoftFloat64FPRecpX(double value);
        private delegate double SoftFloat64FPRSqrtEstimate(double value);
        private delegate double SoftFloat64FPRSqrtEstimateFpscr(double value, bool standardFpscr);
        private delegate double SoftFloat64FPRSqrtStep(double value1, double value2);
        private delegate double SoftFloat64FPRSqrtStepFused(double value1, double value2);
        private delegate double SoftFloat64FPSqrt(double value);
        private delegate double SoftFloat64FPSub(double value1, double value2);

        private delegate ushort SoftFloat64_16FPConvert(double value);
    }
}
