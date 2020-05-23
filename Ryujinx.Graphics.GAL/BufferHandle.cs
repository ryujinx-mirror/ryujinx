using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.GAL
{
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct BufferHandle : IEquatable<BufferHandle>
    {
        private readonly ulong _value;

        public static BufferHandle Null => new BufferHandle(0);

        private BufferHandle(ulong value) => _value = value;

        public override bool Equals(object obj) => obj is BufferHandle handle && Equals(handle);
        public bool Equals([AllowNull] BufferHandle other) => other._value == _value;
        public override int GetHashCode() => _value.GetHashCode();
        public static bool operator ==(BufferHandle left, BufferHandle right) => left.Equals(right);
        public static bool operator !=(BufferHandle left, BufferHandle right) => !(left == right);
    }
}
