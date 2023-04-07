using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Controls;
using Ryujinx.Ava.UI.Models;
using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.HLE.FileSystem;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using Image = SixLabors.ImageSharp.Image;

namespace Ryujinx.Ava.UI.Views.User
{
    public partial class UserProfileImageSelectorView : UserControl
    {
        private ContentManager _contentManager;
        private NavigationDialogHost _parent;
        private TempProfile _profile;

        internal UserProfileImageSelectorViewModel ViewModel { get; private set; }

        public UserProfileImageSelectorView()
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
                        (_parent, _profile) = ((NavigationDialogHost, TempProfile))arg.Parameter;
                        _contentManager = _parent.ContentManager;

                        ((ContentDialog)_parent.Parent).Title = $"{LocaleManager.Instance[LocaleKeys.UserProfileWindowTitle]} - {LocaleManager.Instance[LocaleKeys.ProfileImageSelectionHeader]}";

                        if (Program.PreviewerDetached)
                        {
                            DataContext = ViewModel = new UserProfileImageSelectorViewModel();
                            ViewModel.FirmwareFound = _contentManager.GetCurrentFirmwareVersion() != null;
                        }

                        break;
                    case NavigationMode.Back:
                        if (_profile.Image != null)
                        {
                            _parent.GoBack();
                        }
                        break;
                }
            }
        }

        private async void Import_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.Filters.Add(new FileDialogFilter
            {
                Name = LocaleManager.Instance[LocaleKeys.AllSupportedFormats],
                Extensions = { "jpg", "jpeg", "png", "bmp" }
            });
            dialog.Filters.Add(new FileDialogFilter { Name = "JPEG", Extensions = { "jpg", "jpeg" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "PNG", Extensions = { "png" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "BMP", Extensions = { "bmp" } });

            dialog.AllowMultiple = false;

            string[] image = await dialog.ShowAsync(((TopLevel)_parent.GetVisualRoot()) as Window);

            if (image != null)
            {
                if (image.Length > 0)
                {
                    string imageFile = image[0];

                    _profile.Image = ProcessProfileImage(File.ReadAllBytes(imageFile));

                    if (_profile.Image != null)
                    {
                        _parent.GoBack();
                    }
                }
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            _parent.GoBack();
        }

        private void SelectFirmwareImage_OnClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.FirmwareFound)
            {
                _parent.Navigate(typeof(UserFirmwareAvatarSelectorView), (_parent, _profile));
            }
        }

        private static byte[] ProcessProfileImage(byte[] buffer)
        {
            using (Image image = Image.Load(buffer))
            {
                image.Mutate(x => x.Resize(256, 256));

                using (MemoryStream streamJpg = new())
                {
                    image.SaveAsJpeg(streamJpg);

                    return streamJpg.ToArray();
                }
            }
        }
    }
}