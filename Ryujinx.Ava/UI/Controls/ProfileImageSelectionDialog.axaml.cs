using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Models;
using Ryujinx.Ava.UI.Windows;
using Ryujinx.HLE.FileSystem;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using Image = SixLabors.ImageSharp.Image;

namespace Ryujinx.Ava.UI.Controls
{
    public partial class ProfileImageSelectionDialog : UserControl
    {
        private ContentManager _contentManager;
        private NavigationDialogHost _parent;
        private TempProfile _profile;

        public bool FirmwareFound => _contentManager.GetCurrentFirmwareVersion() != null;

        public ProfileImageSelectionDialog()
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
                        break;
                    case NavigationMode.Back:
                        _parent.GoBack();
                        break;
                }

                DataContext = this;
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
                }

                _parent.GoBack();
            }
        }

        private void SelectFirmwareImage_OnClick(object sender, RoutedEventArgs e)
        {
            if (FirmwareFound)
            {
                _parent.Navigate(typeof(AvatarWindow), (_parent, _profile));
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