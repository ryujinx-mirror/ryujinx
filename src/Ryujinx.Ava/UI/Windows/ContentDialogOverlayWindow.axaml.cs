using Avalonia.Controls;
using Avalonia.Media;
#if DEBUG
using Avalonia;
#endif

namespace Ryujinx.Ava.UI.Windows
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
