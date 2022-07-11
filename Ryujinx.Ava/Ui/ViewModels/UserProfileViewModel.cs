using Avalonia.Threading;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UserProfile = Ryujinx.Ava.Ui.Models.UserProfile;

namespace Ryujinx.Ava.Ui.ViewModels
{
    public class UserProfileViewModel : BaseModel, IDisposable
    {
        private const uint MaxProfileNameLength = 0x20;

        private readonly UserProfileWindow _owner;

        private UserProfile _selectedProfile;
        private string _tempUserName;

        public UserProfileViewModel()
        {
            Profiles = new ObservableCollection<UserProfile>();
        }

        public UserProfileViewModel(UserProfileWindow owner) : this()
        {
            _owner = owner;

            LoadProfiles();
        }

        public ObservableCollection<UserProfile> Profiles { get; set; }

        public UserProfile SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                _selectedProfile = value;

                OnPropertyChanged(nameof(SelectedProfile));
                OnPropertyChanged(nameof(IsSelectedProfileDeletable));
            }
        }

        public bool IsSelectedProfileDeletable =>
            _selectedProfile != null && _selectedProfile.UserId != AccountManager.DefaultUserId;

        public void Dispose()
        {
        }

        public void LoadProfiles()
        {
            Profiles.Clear();

            var profiles = _owner.AccountManager.GetAllUsers()
                .OrderByDescending(x => x.AccountState == AccountState.Open);

            foreach (var profile in profiles)
            {
                Profiles.Add(new UserProfile(profile));
            }

            SelectedProfile = Profiles.FirstOrDefault(x => x.UserId == _owner.AccountManager.LastOpenedUser.UserId);

            if (SelectedProfile == null)
            {
                SelectedProfile = Profiles.First();

                if (SelectedProfile != null)
                {
                    _owner.AccountManager.OpenUser(_selectedProfile.UserId);
                }
            }
        }

        public async void ChooseProfileImage()
        {
            await SelectProfileImage();
        }

        public async Task SelectProfileImage(bool isNewUser = false)
        {
            ProfileImageSelectionDialog selectionDialog = new(_owner.ContentManager);

            await selectionDialog.ShowDialog(_owner);

            if (selectionDialog.BufferImageProfile != null)
            {
                if (isNewUser)
                {
                    if (!string.IsNullOrWhiteSpace(_tempUserName))
                    {
                        _owner.AccountManager.AddUser(_tempUserName, selectionDialog.BufferImageProfile);
                    }
                }
                else if (SelectedProfile != null)
                {
                    _owner.AccountManager.SetUserImage(SelectedProfile.UserId, selectionDialog.BufferImageProfile);
                    SelectedProfile.Image = selectionDialog.BufferImageProfile;

                    SelectedProfile = null;
                }

                LoadProfiles();
            }
        }

        public async void AddUser()
        {
            var dlgTitle = LocaleManager.Instance["InputDialogAddNewProfileTitle"];
            var dlgMainText = LocaleManager.Instance["InputDialogAddNewProfileHeader"];
            var dlgSubText = string.Format(LocaleManager.Instance["InputDialogAddNewProfileSubtext"],
                MaxProfileNameLength);

            _tempUserName =
                await ContentDialogHelper.CreateInputDialog(dlgTitle, dlgMainText, dlgSubText, _owner,
                    MaxProfileNameLength);

            if (!string.IsNullOrWhiteSpace(_tempUserName))
            {
                await SelectProfileImage(true);
            }

            _tempUserName = String.Empty;
        }

        public async void DeleteUser()
        {
            if (_selectedProfile != null)
            {
                var lastUserId = _owner.AccountManager.LastOpenedUser.UserId;

                if (_selectedProfile.UserId == lastUserId)
                {
                    // If we are deleting the currently open profile, then we must open something else before deleting.
                    var profile = Profiles.FirstOrDefault(x => x.UserId != lastUserId);

                    if (profile == null)
                    {
                        Dispatcher.UIThread.Post(async () =>
                        {
                            await ContentDialogHelper.CreateErrorDialog(_owner,
                                LocaleManager.Instance["DialogUserProfileDeletionWarningMessage"]);
                        });

                        return;
                    }

                    _owner.AccountManager.OpenUser(profile.UserId);
                }

                var result =
                    await ContentDialogHelper.CreateConfirmationDialog(_owner,
                        LocaleManager.Instance["DialogUserProfileDeletionConfirmMessage"], "",
                        LocaleManager.Instance["InputDialogYes"], LocaleManager.Instance["InputDialogNo"], "");

                if (result == UserResult.Yes)
                {
                    _owner.AccountManager.DeleteUser(_selectedProfile.UserId);
                }
            }

            LoadProfiles();
        }
    }
}