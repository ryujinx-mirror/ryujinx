using Ryujinx.HLE.Ui;

namespace Ryujinx.Headless.SDL2
{
    internal class HeadlessHostUiTheme : IHostUiTheme
    {
        public string FontFamily => "sans-serif";

        public ThemeColor DefaultBackgroundColor => new ThemeColor(1, 0, 0, 0);
        public ThemeColor DefaultForegroundColor => new ThemeColor(1, 1, 1, 1);
        public ThemeColor DefaultBorderColor => new ThemeColor(1, 1, 1, 1);
        public ThemeColor SelectionBackgroundColor => new ThemeColor(1, 1, 1, 1);
        public ThemeColor SelectionForegroundColor => new ThemeColor(1, 0, 0, 0);

        public HeadlessHostUiTheme() { }
    }
}