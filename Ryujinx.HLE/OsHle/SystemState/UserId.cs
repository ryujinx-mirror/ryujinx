using Ryujinx.HLE.OsHle.Utilities;
using System;
using System.IO;
using System.Linq;

namespace Ryujinx.HLE.OsHle.SystemState
{
    public struct UserId
    {
        public string UserIdHex { get; private set; }

        public byte[] Bytes { get; private set; }

        public UserId(long Low, long High)
        {
            if ((Low | High) == 0)
            {
                throw new ArgumentException("Zero is not a valid user id!");
            }

            byte[] Bytes = new byte[16];

            int Index = Bytes.Length;

            void WriteBytes(long Value)
            {
                for (int Byte = 0; Byte < 8; Byte++)
                {
                    Bytes[--Index] = (byte)(Value >> Byte * 8);
                }
            }

            WriteBytes(Low);
            WriteBytes(High);

            UserIdHex = string.Empty;

            foreach (byte Byte in Bytes)
            {
                UserIdHex += Byte.ToString("X2");
            }

            this.Bytes = Bytes;
        }

        public UserId(string UserIdHex)
        {
            if (UserIdHex == null || UserIdHex.Length != 32 || !UserIdHex.All("0123456789abcdefABCDEF".Contains))
            {
                throw new ArgumentException("Invalid user id!", nameof(UserIdHex));
            }

            if (UserIdHex == "00000000000000000000000000000000")
            {
                throw new ArgumentException("Zero is not a valid user id!", nameof(UserIdHex));
            }

            this.UserIdHex = UserIdHex.ToUpper();

            Bytes = StringUtils.HexToBytes(UserIdHex);
        }

        internal void Write(BinaryWriter Writer)
        {
            for (int Index = Bytes.Length - 1; Index >= 0; Index--)
            {
                Writer.Write(Bytes[Index]);
            }
        }

        public override string ToString()
        {
            return UserIdHex;
        }
    }
}