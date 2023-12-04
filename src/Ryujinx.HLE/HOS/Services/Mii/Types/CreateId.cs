using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    readonly struct CreateId : IEquatable<CreateId>
    {
        public readonly UInt128 Raw;

        public readonly bool IsNull => Raw == UInt128.Zero;
        public readonly bool IsValid => !IsNull && ((Raw >> 64) & 0xC0) == 0x80;

        public CreateId(UInt128 raw)
        {
            Raw = raw;
        }

        public static bool operator ==(CreateId x, CreateId y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(CreateId x, CreateId y)
        {
            return !x.Equals(y);
        }

        public readonly override bool Equals(object obj)
        {
            return obj is CreateId createId && Equals(createId);
        }

        public readonly bool Equals(CreateId cmpObj)
        {
            // Nintendo additionally check that the CreatorId is valid before doing the actual comparison.
            return IsValid && Raw == cmpObj.Raw;
        }

        public readonly override int GetHashCode()
        {
            return Raw.GetHashCode();
        }
    }
}
