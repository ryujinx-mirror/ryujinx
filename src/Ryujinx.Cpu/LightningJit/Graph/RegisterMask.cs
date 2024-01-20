using System;

namespace Ryujinx.Cpu.LightningJit.Graph
{
    readonly struct RegisterMask : IEquatable<RegisterMask>
    {
        public readonly uint GprMask;
        public readonly uint FpSimdMask;
        public readonly uint PStateMask;

        public static RegisterMask Zero => new(0u, 0u, 0u);

        public RegisterMask(uint gprMask, uint fpSimdMask, uint pStateMask)
        {
            GprMask = gprMask;
            FpSimdMask = fpSimdMask;
            PStateMask = pStateMask;
        }

        public static RegisterMask operator &(RegisterMask x, RegisterMask y)
        {
            return new(x.GprMask & y.GprMask, x.FpSimdMask & y.FpSimdMask, x.PStateMask & y.PStateMask);
        }

        public static RegisterMask operator |(RegisterMask x, RegisterMask y)
        {
            return new(x.GprMask | y.GprMask, x.FpSimdMask | y.FpSimdMask, x.PStateMask | y.PStateMask);
        }

        public static RegisterMask operator ~(RegisterMask x)
        {
            return new(~x.GprMask, ~x.FpSimdMask, ~x.PStateMask);
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
            return GprMask == other.GprMask && FpSimdMask == other.FpSimdMask && PStateMask == other.PStateMask;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GprMask, FpSimdMask, PStateMask);
        }
    }
}
