using System;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    public class UserProfile
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public UserId UserId { get; }

        public long LastModifiedTimestamp { get; set; }

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;

                UpdateLastModifiedTimestamp();
            }
        }

        private byte[] _image;

        public byte[] Image
        {
            get => _image;
            set
            {
                _image = value;

                UpdateLastModifiedTimestamp();
            }
        }

        private AccountState _accountState;

        public AccountState AccountState
        {
            get => _accountState;
            set
            {
                _accountState = value;

                UpdateLastModifiedTimestamp();
            }
        }

        public AccountState _onlinePlayState;

        public AccountState OnlinePlayState
        {
            get => _onlinePlayState;
            set
            {
                _onlinePlayState = value;

                UpdateLastModifiedTimestamp();
            }
        }

        public UserProfile(UserId userId, string name, byte[] image, long lastModifiedTimestamp = 0)
        {
            UserId = userId;
            Name   = name;
            Image  = image;

            AccountState    = AccountState.Closed;
            OnlinePlayState = AccountState.Closed;

            if (lastModifiedTimestamp != 0)
            {
                LastModifiedTimestamp = lastModifiedTimestamp;
            }
            else
            {
                UpdateLastModifiedTimestamp();
            }
        }

        private void UpdateLastModifiedTimestamp()
        {
            LastModifiedTimestamp = (long)(DateTime.Now - Epoch).TotalSeconds;
        }
    }
}