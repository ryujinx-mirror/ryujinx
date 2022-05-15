using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Controls;
using System;
using System.IO;
using System.Reflection;

namespace Ryujinx.Ava.Ui.Windows
{
    public class StyleableWindow : Window
    {
        public ContentDialog ContentDialog { get; private set; }
        public IBitmap IconImage { get; set; }

        public StyleableWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            TransparencyLevelHint = WindowTransparencyLevel.None;

            using Stream stream = Assembly.GetAssembly(typeof(Ryujinx.Ui.Common.Configuration.ConfigurationState)).GetManifestResourceStream("Ryujinx.Ui.Common.Resources.Logo_Ryujinx.png");

            Icon = new WindowIcon(stream);
            stream.Position = 0;
            IconImage = new Bitmap(stream);
        }

        public void LoadDialog()
        {
            ContentDialog = this.FindControl<ContentDialog>("ContentDialog");
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            ContentDialog = this.FindControl<ContentDialog>("ContentDialog");
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome | ExtendClientAreaChromeHints.OSXThickTitleBar;
        }
    }
}