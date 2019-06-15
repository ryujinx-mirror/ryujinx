using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Utilities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE.HOS.Services.Acc
{
    public class AccountUtils
    {
        private ConcurrentDictionary<string, UserProfile> _profiles;

        internal UserProfile LastOpenedUser { get; private set; }

        public AccountUtils()
        {
            _profiles = new ConcurrentDictionary<string, UserProfile>();
        }

        public void AddUser(UInt128 userId, string name)
        {
            UserProfile profile = new UserProfile(userId, name);

            _profiles.AddOrUpdate(userId.ToString(), profile, (key, old) => profile);
        }

        public void OpenUser(UInt128 userId)
        {
            if (_profiles.TryGetValue(userId.ToString(), out UserProfile profile))
            {
                (LastOpenedUser = profile).AccountState = AccountState.Open;
            }
        }

        public void CloseUser(UInt128 userId)
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

        internal bool TryGetUser(UInt128 userId, out UserProfile profile)
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