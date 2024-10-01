using Ryujinx.Common.Memory;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 2, Size = SizeConst)]
    struct Nickname : IEquatable<Nickname>
    {
        public const int CharCount = 10;
        private const int SizeConst = (CharCount + 1) * 2;

        private Array22<byte> _storage;

        public static Nickname Default => FromString("no name");
        public static Nickname Question => FromString("???");

        public Span<byte> Raw => _storage.AsSpan();

        private ReadOnlySpan<ushort> Characters => MemoryMarshal.Cast<byte, ushort>(Raw);

        private int GetEndCharacterIndex()
        {
            for (int i = 0; i < Characters.Length; i++)
            {
                if (Characters[i] == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < Characters.Length - 1; i++)
            {
                if (Characters[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsValid()
        {
            // Create a new unicode encoding instance with error checking enabled
            UnicodeEncoding unicodeEncoding = new(false, false, true);

            try
            {
                unicodeEncoding.GetString(Raw);

                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public bool IsValidForFontRegion(FontRegion fontRegion)
        {
            // TODO: We need to extract the character tables used here, for now just assume that if it's valid Unicode, it will be valid for any font.
            return IsValid();
        }

        public override string ToString()
        {
            return Encoding.Unicode.GetString(Raw);
        }

        public static Nickname FromBytes(ReadOnlySpan<byte> data)
        {
            if (data.Length > SizeConst)
            {
                data = data[..SizeConst];
            }

            Nickname result = new();

            data.CopyTo(result.Raw);

            return result;
        }

        public static Nickname FromString(string nickname)
        {
            return FromBytes(Encoding.Unicode.GetBytes(nickname));
        }

        public static bool operator ==(Nickname x, Nickname y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Nickname x, Nickname y)
        {
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            return obj is Nickname nickname && Equals(nickname);
        }

        public bool Equals(Nickname cmpObj)
        {
            return Raw.SequenceEqual(cmpObj.Raw);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Raw.ToArray());
        }
    }
}
