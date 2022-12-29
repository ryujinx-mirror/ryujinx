using Ryujinx.Ava.UI.Controls;
using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Profile = Ryujinx.HLE.HOS.Services.Account.Acc.UserProfile;

namespace Ryujinx.Ava.UI.Models
{
    public class UserProfile : BaseModel
    {
        private readonly Profile _profile;
        private readonly NavigationDialogHost _owner;
        private byte[] _image;
        private string _name;
        private UserId _userId;

        public byte[] Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        public UserId UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public UserProfile(Profile profile, NavigationDialogHost owner)
        {
            _profile = profile;
            _owner = owner;

            Image = profile.Image;
            Name = profile.Name;
            UserId = profile.UserId;
        }

        public bool IsOpened => _profile.AccountState == AccountState.Open;

        public void UpdateState()
        {
            OnPropertyChanged(nameof(IsOpened));
            OnPropertyChanged(nameof(Name));
        }

        public void Recover(UserProfile userProfile)
        {
            _owner.Navigate(typeof(UserEditor), (_owner, userProfile, true));
        }
    }
}