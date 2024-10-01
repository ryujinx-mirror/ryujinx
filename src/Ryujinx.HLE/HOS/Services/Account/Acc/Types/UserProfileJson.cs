namespace Ryujinx.HLE.HOS.Services.Account.Acc.Types
{
    internal struct UserProfileJson
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public AccountState AccountState { get; set; }
        public AccountState OnlinePlayState { get; set; }
        public long LastModifiedTimestamp { get; set; }
        public byte[] Image { get; set; }
    }
}
