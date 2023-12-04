using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Ryujinx.Ui.Common.Configuration;
using System.IO;
using System.Reflection;

namespace Ryujinx.Ava.UI.Windows
{
    public class StyleableWindow : Window
    {
        public Bitmap IconImage { get; set; }

        public StyleableWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            TransparencyLevelHint = new[] { WindowTransparencyLevel.None };

            using Stream stream = Assembly.GetAssembly(typeof(ConfigurationState)).GetManifestResourceStream("Ryujinx.Ui.Common.Resources.Logo_Ryujinx.png");

            Icon = new WindowIcon(stream);
            stream.Position = 0;
            IconImage = new Bitmap(stream);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome | ExtendClientAreaChromeHints.OSXThickTitleBar;
        }
    }
}
