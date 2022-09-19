using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Ryujinx.Ava.Ui.Windows
{
    public partial class ContentDialogOverlayWindow : StyleableWindow
    {
        public ContentDialogOverlayWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            ExtendClientAreaToDecorationsHint = true;
            TransparencyLevelHint = WindowTransparencyLevel.Transparent;
            WindowStartupLocation = WindowStartupLocation.Manual;
            SystemDecorations = SystemDecorations.None;
            ExtendClientAreaTitleBarHeightHint = 0;
            Background = Brushes.Transparent;
            CanResize = false;
        }
    }
}