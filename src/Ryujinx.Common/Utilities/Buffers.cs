using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Utilities
{
    [DebuggerDisplay("{ToString()}")]
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct Buffer16
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ulong _dummy0;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ulong _dummy1;

        public byte this[int i]
        {
            get => Bytes[i];
            set => Bytes[i] = value;
        }

        public Span<byte> Bytes => SpanHelpers.AsByteSpan(ref this);

        // Prevent a defensive copy by changing the read-only in reference to a reference with Unsafe.AsRef()
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(in Buffer16 value)
        {
            return SpanHelpers.AsByteSpan(ref Unsafe.AsRef(in value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(in Buffer16 value)
        {
            return SpanHelpers.AsReadOnlyByteSpan(ref Unsafe.AsRef(in value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T As<T>() where T : unmanaged
        {
            if (Unsafe.SizeOf<T>() > (uint)Unsafe.SizeOf<Buffer16>())
            {
                throw new ArgumentException();
            }

            return ref MemoryMarshal.GetReference(AsSpan<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan<T>() where T : unmanaged
        {
            return SpanHelpers.AsSpan<Buffer16, T>(ref this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged
        {
            return SpanHelpers.AsReadOnlySpan<Buffer16, T>(ref Unsafe.AsRef(in this));
        }
    }
}
