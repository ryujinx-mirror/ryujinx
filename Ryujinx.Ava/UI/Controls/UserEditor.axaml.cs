using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.Models;
using UserProfile = Ryujinx.Ava.UI.Models.UserProfile;

namespace Ryujinx.Ava.UI.Controls
{
    public partial class UserEditor : UserControl
    {
        private NavigationDialogHost _parent;
        private UserProfile _profile;
        private bool _isNewUser;

        public TempProfile TempProfile { get; set; }
        public uint MaxProfileNameLength => 0x20;

        public UserEditor()
        {
            InitializeComponent();
            AddHandler(Frame.NavigatedToEvent, (s, e) =>
            {
                NavigatedTo(e);
            }, RoutingStrategies.Direct);
        }

        private void NavigatedTo(NavigationEventArgs arg)
        {
            if (Program.PreviewerDetached)
            {
                switch (arg.NavigationMode)
                {
                    case NavigationMode.New:
                        var args = ((NavigationDialogHost parent, UserProfile profile, bool isNewUser))arg.Parameter;
                        _isNewUser = args.isNewUser;
                        _profile = args.profile;
                        TempProfile = new TempProfile(_profile);

                        _parent = args.parent;
                        break;
                }

                DataContext = TempProfile;

                AddPictureButton.IsVisible = _isNewUser;
                IdLabel.IsVisible = _profile != null;
                IdText.IsVisible = _profile != null;
                ChangePictureButton.IsVisible = !_isNewUser;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _parent?.GoBack();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DataValidationErrors.ClearErrors(NameBox);
            bool isInvalid = false;

            if (string.IsNullOrWhiteSpace(TempProfile.Name))
            {
                DataValidationErrors.SetError(NameBox, new DataValidationException(LocaleManager.Instance[LocaleKeys.UserProfileEmptyNameError]));

                isInvalid = true;
            }

            if (TempProfile.Image == null)
            {
                await ContentDialogHelper.CreateWarningDialog(LocaleManager.Instance[LocaleKeys.UserProfileNoImageError], "");

                isInvalid = true;
            }

            if(isInvalid)
            {
                return;
            }

            if (_profile != null && !_isNewUser)
            {
                _profile.Name = TempProfile.Name;
                _profile.Image = TempProfile.Image;
                _profile.UpdateState();
                _parent.AccountManager.SetUserName(_profile.UserId, _profile.Name);
                _parent.AccountManager.SetUserImage(_profile.UserId, _profile.Image);
            }
            else if (_isNewUser)
            {
                _parent.AccountManager.AddUser(TempProfile.Name, TempProfile.Image, TempProfile.UserId);
            }
            else
            {
                return;
            }

            _parent?.GoBack();
        }

        public void SelectProfileImage()
        {
            _parent.Navigate(typeof(ProfileImageSelectionDialog), (_parent, TempProfile));
        }

        private void ChangePictureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_profile != null || _isNewUser)
            {
                SelectProfileImage();
            }
        }
    }
}