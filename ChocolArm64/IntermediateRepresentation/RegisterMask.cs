using System;

namespace ChocolArm64.IntermediateRepresentation
{
    struct RegisterMask : IEquatable<RegisterMask>
    {
        public long IntMask { get; set; }
        public long VecMask { get; set; }

        public RegisterMask(long intMask, long vecMask)
        {
            IntMask = intMask;
            VecMask = vecMask;
        }

        public static RegisterMask operator &(RegisterMask x, RegisterMask y)
        {
            return new RegisterMask(x.IntMask & y.IntMask, x.VecMask & y.VecMask);
        }

        public static RegisterMask operator |(RegisterMask x, RegisterMask y)
        {
            return new RegisterMask(x.IntMask | y.IntMask, x.VecMask | y.VecMask);
        }

        public static RegisterMask operator ~(RegisterMask x)
        {
            return new RegisterMask(~x.IntMask, ~x.VecMask);
        }

        public static bool operator ==(RegisterMask x, RegisterMask y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(RegisterMask x, RegisterMask y)
        {
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            return obj is RegisterMask regMask && Equals(regMask);
        }

        public bool Equals(RegisterMask other)
        {
            return IntMask == other.IntMask && VecMask == other.VecMask;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IntMask, VecMask);
        }
    }
}
