using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.ObjectModel;
using UserProfile = Ryujinx.Ava.UI.Models.UserProfile;

namespace Ryujinx.Ava.UI.ViewModels
{
    public class UserProfileViewModel : BaseModel, IDisposable
    {
        public UserProfileViewModel()
        {
            Profiles = new ObservableCollection<BaseModel>();
            LostProfiles = new ObservableCollection<UserProfile>();
            IsEmpty = LostProfiles.IsNullOrEmpty();
        }

        public ObservableCollection<BaseModel> Profiles { get; set; }

        public ObservableCollection<UserProfile> LostProfiles { get; set; }

        public bool IsEmpty { get; set; }

        public void Dispose() { }
    }
}