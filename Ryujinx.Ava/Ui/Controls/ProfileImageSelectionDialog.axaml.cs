using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.HLE.FileSystem;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using Image = SixLabors.ImageSharp.Image;

namespace Ryujinx.Ava.Ui.Controls
{
    public class ProfileImageSelectionDialog : StyleableWindow
    {
        private readonly ContentManager _contentManager;

        public bool FirmwareFound => _contentManager.GetCurrentFirmwareVersion() != null;

        public byte[] BufferImageProfile { get; set; }

        public ProfileImageSelectionDialog(ContentManager contentManager)
        {
            _contentManager = contentManager;
            DataContext = this;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public ProfileImageSelectionDialog()
        {
            DataContext = this;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void Import_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.Filters.Add(new FileDialogFilter
            {
                Name = LocaleManager.Instance["AllSupportedFormats"],
                Extensions = { "jpg", "jpeg", "png", "bmp" }
            });
            dialog.Filters.Add(new FileDialogFilter { Name = "JPEG", Extensions = { "jpg", "jpeg" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "PNG", Extensions = { "png" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "BMP", Extensions = { "bmp" } });

            dialog.AllowMultiple = false;

            string[] image = await dialog.ShowAsync(this);

            if (image != null)
            {
                if (image.Length > 0)
                {
                    string imageFile = image[0];

                    ProcessProfileImage(File.ReadAllBytes(imageFile));
                }

                Close();
            }
        }

        private async void SelectFirmwareImage_OnClick(object sender, RoutedEventArgs e)
        {
            if (FirmwareFound)
            {
                AvatarWindow window = new(_contentManager);

                await window.ShowDialog(this);

                BufferImageProfile = window.SelectedImage;

                Close();
            }
        }

        private void ProcessProfileImage(byte[] buffer)
        {
            using (Image image = Image.Load(buffer))
            {
                image.Mutate(x => x.Resize(256, 256));

                using (MemoryStream streamJpg = new())
                {
                    image.SaveAsJpeg(streamJpg);

                    BufferImageProfile = streamJpg.ToArray();
                }
            }
        }
    }
}