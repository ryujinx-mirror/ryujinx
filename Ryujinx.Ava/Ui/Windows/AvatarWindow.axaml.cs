using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.ViewModels;
using Ryujinx.HLE.FileSystem;
using System;

namespace Ryujinx.Ava.Ui.Windows
{
    public class AvatarWindow : StyleableWindow
    {
        public AvatarWindow(ContentManager contentManager)
        {
            ContentManager = contentManager;
            ViewModel = new AvatarProfileViewModel(() => ViewModel.ReloadImages());

            DataContext = ViewModel;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["AvatarWindowTitle"];
        }

        public AvatarWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            if (Program.PreviewerDetached)
            {
                Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["AvatarWindowTitle"];
            }
        }

        public ContentManager ContentManager { get; }

        public byte[] SelectedImage { get; set; }

        internal AvatarProfileViewModel ViewModel { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            ViewModel.Dispose();
            base.OnClosed(e);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ChooseButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedIndex > -1)
            {
                SelectedImage = ViewModel.SelectedImage;

                Close();
            }
        }
    }
}