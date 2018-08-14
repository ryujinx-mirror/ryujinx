using Ryujinx.HLE.OsHle.Utilities;
using System;
using System.Linq;

namespace Ryujinx.HLE.OsHle.Services.Acc
{
    struct ProfileBase
    {
        public UserId UserId;
        public long   Timestamp;
        public string Username;

        public ProfileBase(Profile User)
        {
            UserId    = new UserId(User.UserId);
            Username  = User.Username;
            Timestamp = ((DateTimeOffset)DateTime.Today).ToUnixTimeSeconds();
        }
    }

    struct UserId
    {
        private readonly ulong LowBytes;
        private readonly ulong HighBytes;

        public UserId(string UserIdHex)
        {
            if (UserIdHex == null || UserIdHex.Length != 32 || !UserIdHex.All("0123456789abcdefABCDEF".Contains))
            {
                throw new ArgumentException("UserId is not a valid Hex string", "UserIdHex");
            }

            byte[] HexBytes = StringUtils.HexToBytes(UserIdHex);

            LowBytes = BitConverter.ToUInt64(HexBytes, 8);

            Array.Resize(ref HexBytes, 8);

            HighBytes = BitConverter.ToUInt64(HexBytes, 0);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(HighBytes).Concat(BitConverter.GetBytes(LowBytes)).ToArray();
        }

        public override string ToString()
        {
            return BitConverter.ToString(ToBytes()).ToLower().Replace("-", string.Empty);
        }
    }
}
