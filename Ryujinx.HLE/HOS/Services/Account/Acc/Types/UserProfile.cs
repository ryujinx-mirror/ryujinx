using Ryujinx.HLE.Utilities;
using System;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    class UserProfile
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public UserId UserId { get; private set; }

        public string Name { get; private set; }

        public long LastModifiedTimestamp { get; private set; }

        public AccountState AccountState    { get; set; }
        public AccountState OnlinePlayState { get; set; }

        public UserProfile(UserId userId, string name)
        {
            UserId = userId;
            Name   = name;

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