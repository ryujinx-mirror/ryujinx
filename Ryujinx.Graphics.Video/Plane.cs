using System;
using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.Graphics.Video
{
    public struct Plane : IEquatable<Plane>
    {
        public IntPtr Pointer { get; }
        public int Length { get; }

        public Plane(IntPtr pointer, int length)
        {
            Pointer = pointer;
            Length = length;
        }

        public override bool Equals(object obj)
        {
            return obj is Plane other && Equals(other);
        }

        public bool Equals([AllowNull] Plane other)
        {
            return Pointer == other.Pointer && Length == other.Length;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Pointer, Length);
        }

        public static bool operator ==(Plane left, Plane right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Plane left, Plane right)
        {
            return !(left == right);
        }
    }
}
