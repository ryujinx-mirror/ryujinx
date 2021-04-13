using Ryujinx.Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    public class AccountManager
    {
        private ConcurrentDictionary<string, UserProfile> _profiles;

        public UserProfile LastOpenedUser { get; private set; }

        public AccountManager()
        {
            _profiles = new ConcurrentDictionary<string, UserProfile>();

            UserId defaultUserId    = new UserId("00000000000000010000000000000000");
            byte[] defaultUserImage = EmbeddedResources.Read("Ryujinx.HLE/HOS/Services/Account/Acc/DefaultUserImage.jpg");

            AddUser(defaultUserId, "Player", defaultUserImage);
            
            OpenUser(defaultUserId);
        }

        public void AddUser(UserId userId, string name, byte[] image)
        {
            UserProfile profile = new UserProfile(userId, name, image);

            _profiles.AddOrUpdate(userId.ToString(), profile, (key, old) => profile);
        }

        public void OpenUser(UserId userId)
        {
            if (_profiles.TryGetValue(userId.ToString(), out UserProfile profile))
            {
                (LastOpenedUser = profile).AccountState = AccountState.Open;
            }
        }

        public void CloseUser(UserId userId)
        {
            if (_profiles.TryGetValue(userId.ToString(), out UserProfile profile))
            {
                profile.AccountState = AccountState.Closed;
            }
        }

        public int GetUserCount()
        {
            return _profiles.Count;
        }

        internal bool TryGetUser(UserId userId, out UserProfile profile)
        {
            return _profiles.TryGetValue(userId.ToString(), out profile);
        }

        internal IEnumerable<UserProfile> GetAllUsers()
        {
            return _profiles.Values;
        }

        internal IEnumerable<UserProfile> GetOpenedUsers()
        {
            return _profiles.Values.Where(x => x.AccountState == AccountState.Open);
        }

        internal UserProfile GetFirst()
        {
            return _profiles.First().Value;
        }
    }
}