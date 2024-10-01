using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Account.Acc.Types
{
    internal struct ProfilesJson
    {
        public List<UserProfileJson> Profiles { get; set; }
        public string LastOpened { get; set; }
    }
}
