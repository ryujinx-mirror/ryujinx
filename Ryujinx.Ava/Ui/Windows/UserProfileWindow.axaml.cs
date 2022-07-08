using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.ViewModels;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using System.Threading.Tasks;
using UserProfile = Ryujinx.Ava.Ui.Models.UserProfile;

namespace Ryujinx.Ava.Ui.Windows
{
    public class UserProfileWindow : StyleableWindow
    {
        private TextBox _nameBox;

        public UserProfileWindow(AccountManager accountManager, ContentManager contentManager,
            VirtualFileSystem virtualFileSystem)
        {
            AccountManager = accountManager;
            ContentManager = contentManager;
            ViewModel = new UserProfileViewModel(this);

            DataContext = ViewModel;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            if (contentManager.GetCurrentFirmwareVersion() != null)
            {
                Task.Run(() =>
                {
                    AvatarProfileViewModel.PreloadAvatars(contentManager, virtualFileSystem);
                });
            }

            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["UserProfileWindowTitle"];
        }

        public UserProfileWindow()
        {
            ViewModel = new UserProfileViewModel();

            DataContext = ViewModel;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["UserProfileWindowTitle"];
        }

        public AccountManager AccountManager { get; }
        public ContentManager ContentManager { get; }

        public UserProfileViewModel ViewModel { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _nameBox = this.FindControl<TextBox>("NameBox");
        }

        private void ProfilesList_DoubleTapped(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                int selectedIndex = listBox.SelectedIndex;

                if (selectedIndex >= 0 && selectedIndex < ViewModel.Profiles.Count)
                {
                    ViewModel.SelectedProfile = ViewModel.Profiles[selectedIndex];

                    AccountManager.OpenUser(ViewModel.SelectedProfile.UserId);

                    ViewModel.LoadProfiles();

                    foreach (UserProfile profile in ViewModel.Profiles)
                    {
                        profile.UpdateState();
                    }
                }
            }
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetNameButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_nameBox.Text))
            {
                ViewModel.SelectedProfile.Name = _nameBox.Text;
                AccountManager.SetUserName(ViewModel.SelectedProfile.UserId, _nameBox.Text);
            }
        }
    }
}