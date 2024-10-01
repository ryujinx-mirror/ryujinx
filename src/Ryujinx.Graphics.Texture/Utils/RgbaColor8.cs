using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Graphics.Texture.Utils
{
    struct RgbaColor8 : IEquatable<RgbaColor8>
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public RgbaColor8(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static RgbaColor8 FromUInt32(uint color)
        {
            return Unsafe.As<uint, RgbaColor8>(ref color);
        }

        public static bool operator ==(RgbaColor8 x, RgbaColor8 y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(RgbaColor8 x, RgbaColor8 y)
        {
            return !x.Equals(y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RgbaColor32 GetColor32()
        {
            if (Sse41.IsSupported)
            {
                Vector128<byte> color = Vector128.CreateScalarUnsafe(Unsafe.As<RgbaColor8, uint>(ref this)).AsByte();
                return new RgbaColor32(Sse41.ConvertToVector128Int32(color));
            }
            else
            {
                return new RgbaColor32(R, G, B, A);
            }
        }

        public uint ToUInt32()
        {
            return Unsafe.As<RgbaColor8, uint>(ref this);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        public readonly override bool Equals(object obj)
        {
            return obj is RgbaColor8 other && Equals(other);
        }

        public readonly bool Equals(RgbaColor8 other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A;
        }

        public readonly byte GetComponent(int index)
        {
            return index switch
            {
                0 => R,
                1 => G,
                2 => B,
                3 => A,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
    }
}
