using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    struct CreateId : IEquatable<CreateId>
    {
        public UInt128 Raw;

        public bool IsNull => Raw == UInt128.Zero;
        public bool IsValid => !IsNull && ((Raw >> 64) & 0xC0) == 0x80;

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

        public override bool Equals(object obj)
        {
            return obj is CreateId createId && Equals(createId);
        }

        public bool Equals(CreateId cmpObj)
        {
            // Nintendo additionally check that the CreatorId is valid before doing the actual comparison.
            return IsValid && Raw == cmpObj.Raw;
        }

        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }
    }
}
