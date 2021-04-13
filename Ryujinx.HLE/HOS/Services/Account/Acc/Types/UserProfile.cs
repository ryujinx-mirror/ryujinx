using System;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    public class UserProfile
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public UserId UserId { get; }

        public string Name { get; }

        public byte[] Image { get; }

        public long LastModifiedTimestamp { get; private set; }

        public AccountState AccountState { get; set; }
        public AccountState OnlinePlayState { get; set; }

        public UserProfile(UserId userId, string name, byte[] image)
        {
            UserId = userId;
            Name   = name;

            Image = image;

            LastModifiedTimestamp = 0;

            AccountState    = AccountState.Closed;
            OnlinePlayState = AccountState.Closed;

            UpdateTimestamp();
        }

        private void UpdateTimestamp()
        {
            LastModifiedTimestamp = (long)(DateTime.Now - Epoch).TotalSeconds;
        }
    }
}