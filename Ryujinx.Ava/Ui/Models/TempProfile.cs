using Ryujinx.Ava.Ui.ViewModels;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using System;

namespace Ryujinx.Ava.Ui.Models
{
    public class TempProfile : BaseModel
    {
        private readonly UserProfile _profile;
        private byte[] _image = null;
        private string _name = String.Empty;
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

        public TempProfile(UserProfile profile)
        {
            _profile = profile;

            if (_profile != null)
            {
                Image = profile.Image;
                Name = profile.Name;
                UserId = profile.UserId;
            }
        }

        public TempProfile(){}
    }
}