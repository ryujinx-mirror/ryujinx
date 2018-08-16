using System;

namespace Ryujinx.HLE.HOS.SystemState
{
    class UserProfile
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public UserId Uuid { get; private set; }

        public string Name { get; private set; }

        public long LastModifiedTimestamp { get; private set; }

        public OpenCloseState AccountState    { get; set; }
        public OpenCloseState OnlinePlayState { get; set; }

        public UserProfile(UserId Uuid, string Name)
        {
            this.Uuid = Uuid;
            this.Name = Name;

            LastModifiedTimestamp = 0;

            AccountState    = OpenCloseState.Closed;
            OnlinePlayState = OpenCloseState.Closed;

            UpdateTimestamp();
        }

        private void UpdateTimestamp()
        {
            LastModifiedTimestamp = (long)(DateTime.Now - Epoch).TotalSeconds;
        }
    }
}
