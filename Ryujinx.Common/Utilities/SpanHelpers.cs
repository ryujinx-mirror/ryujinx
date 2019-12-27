using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Utilities
{
    public static class SpanHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> CreateSpan<T>(ref T reference, int length)
        {
            return MemoryMarshal.CreateSpan(ref reference, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(ref T reference) where T : unmanaged
        {
            return CreateSpan(ref reference, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TSpan> AsSpan<TStruct, TSpan>(ref TStruct reference)
            where TStruct : unmanaged where TSpan : unmanaged
        {
            return CreateSpan(ref Unsafe.As<TStruct, TSpan>(ref reference),
                Unsafe.SizeOf<TStruct>() / Unsafe.SizeOf<TSpan>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsByteSpan<T>(ref T reference) where T : unmanaged
        {
            return CreateSpan(ref Unsafe.As<T, byte>(ref reference), Unsafe.SizeOf<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T reference, int length)
        {
            return MemoryMarshal.CreateReadOnlySpan(ref reference, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(ref T reference) where T : unmanaged
        {
            return CreateReadOnlySpan(ref reference, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TSpan> AsReadOnlySpan<TStruct, TSpan>(ref TStruct reference)
            where TStruct : unmanaged where TSpan : unmanaged
        {
            return CreateReadOnlySpan(ref Unsafe.As<TStruct, TSpan>(ref reference),
                Unsafe.SizeOf<TStruct>() / Unsafe.SizeOf<TSpan>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> AsReadOnlyByteSpan<T>(ref T reference) where T : unmanaged
        {
            return CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref reference), Unsafe.SizeOf<T>());
        }
    }
}
