using ARMeilleure.State;
using System;

namespace ARMeilleure.Instructions
{
    delegate double _F64_F64(double a1);
    delegate double _F64_F64_F64(double a1, double a2);
    delegate double _F64_F64_F64_F64(double a1, double a2, double a3);
    delegate double _F64_F64_MidpointRounding(double a1, MidpointRounding a2);

    delegate float _F32_F32(float a1);
    delegate float _F32_F32_F32(float a1, float a2);
    delegate float _F32_F32_F32_F32(float a1, float a2, float a3);
    delegate float _F32_F32_MidpointRounding(float a1, MidpointRounding a2);
    delegate float _F32_U16(ushort a1);

    delegate int _S32_F32(float a1);
    delegate int _S32_F32_F32_Bool(float a1, float a2, bool a3);
    delegate int _S32_F64(double a1);
    delegate int _S32_F64_F64_Bool(double a1, double a2, bool a3);
    delegate int _S32_U64_U16(ulong a1, ushort a2);
    delegate int _S32_U64_U32(ulong a1, uint a2);
    delegate int _S32_U64_U64(ulong a1, ulong a2);
    delegate int _S32_U64_U8(ulong a1, byte a2);
    delegate int _S32_U64_V128(ulong a1, V128 a2);

    delegate long _S64_F32(float a1);
    delegate long _S64_F64(double a1);
    delegate long _S64_S64(long a1);
    delegate long _S64_S64_S32(long a1, int a2);
    delegate long _S64_S64_S64(long a1, long a2);
    delegate long _S64_S64_S64_Bool_S32(long a1, long a2, bool a3, int a4);
    delegate long _S64_S64_S64_S32(long a1, long a2, int a3);
    delegate long _S64_U64_S32(ulong a1, int a2);
    delegate long _S64_U64_S64(ulong a1, long a2);

    delegate ushort _U16_F32(float a1);
    delegate ushort _U16_U64(ulong a1);

    delegate uint _U32_F32(float a1);
    delegate uint _U32_F64(double a1);
    delegate uint _U32_U32(uint a1);
    delegate uint _U32_U32_U16(uint a1, ushort a2);
    delegate uint _U32_U32_U32(uint a1, uint a2);
    delegate uint _U32_U32_U64(uint a1, ulong a2);
    delegate uint _U32_U32_U8(uint a1, byte a2);
    delegate uint _U32_U64(ulong a1);

    delegate ulong _U64();
    delegate ulong _U64_F32(float a1);
    delegate ulong _U64_F64(double a1);
    delegate ulong _U64_S64_S32(long a1, int a2);
    delegate ulong _U64_S64_U64(long a1, ulong a2);
    delegate ulong _U64_U64(ulong a1);
    delegate ulong _U64_U64_S32(ulong a1, int a2);
    delegate ulong _U64_U64_S64_S32(ulong a1, long a2, int a3);
    delegate ulong _U64_U64_U64(ulong a1, ulong a2);
    delegate ulong _U64_U64_U64_Bool_S32(ulong a1, ulong a2, bool a3, int a4);

    delegate byte _U8_U64(ulong a1);

    delegate V128 _V128_U64(ulong a1);
    delegate V128 _V128_V128(V128 a1);
    delegate V128 _V128_V128_U32_V128(V128 a1, uint a2, V128 a3);
    delegate V128 _V128_V128_V128(V128 a1, V128 a2);
    delegate V128 _V128_V128_V128_V128(V128 a1, V128 a2, V128 a3);
    delegate V128 _V128_V128_V128_V128_V128(V128 a1, V128 a2, V128 a3, V128 a4);
    delegate V128 _V128_V128_V128_V128_V128_V128(V128 a1, V128 a2, V128 a3, V128 a4, V128 a5);

    delegate void _Void();
    delegate void _Void_U64(ulong a1);
    delegate void _Void_U64_S32(ulong a1, int a2);
    delegate void _Void_U64_U16(ulong a1, ushort a2);
    delegate void _Void_U64_U32(ulong a1, uint a2);
    delegate void _Void_U64_U64(ulong a1, ulong a2);
    delegate void _Void_U64_U8(ulong a1, byte a2);
    delegate void _Void_U64_V128(ulong a1, V128 a2);
}