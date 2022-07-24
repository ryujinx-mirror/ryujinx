using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Models;
using UserProfile = Ryujinx.Ava.Ui.Models.UserProfile;

namespace Ryujinx.Ava.Ui.Controls
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
                        if (!_isNewUser)
                        {
                            _profile = args.profile;
                            TempProfile = new TempProfile(_profile);
                        }
                        else
                        {
                            TempProfile = new TempProfile();
                        }

                        _parent = args.parent;
                        break;
                }

                DataContext = TempProfile;

                AddPictureButton.IsVisible = _isNewUser;
                IdLabel.IsVisible = !_isNewUser;
                ChangePictureButton.IsVisible = !_isNewUser;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _parent?.GoBack();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DataValidationErrors.ClearErrors(NameBox);
            bool isInvalid = false;

            if (string.IsNullOrWhiteSpace(TempProfile.Name))
            {
                DataValidationErrors.SetError(NameBox, new DataValidationException(LocaleManager.Instance["UserProfileEmptyNameError"]));

                isInvalid = true;
            }

            if (TempProfile.Image == null)
            {
                ContentDialogHelper.CreateWarningDialog(LocaleManager.Instance["UserProfileNoImageError"], "");

                isInvalid = true;
            }

            if(isInvalid)
            {
                return;
            }

            if (_profile != null)
            {
                _profile.Name = TempProfile.Name;
                _profile.Image = TempProfile.Image;
                _profile.UpdateState();
                _parent.AccountManager.SetUserName(_profile.UserId, _profile.Name);
                _parent.AccountManager.SetUserImage(_profile.UserId, _profile.Image);
            }
            else if (_isNewUser)
            {
                _parent.AccountManager.AddUser(TempProfile.Name, TempProfile.Image);
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