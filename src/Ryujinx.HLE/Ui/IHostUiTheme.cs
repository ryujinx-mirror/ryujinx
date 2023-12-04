namespace Ryujinx.HLE.Ui
{
    public interface IHostUiTheme
    {
        string FontFamily { get; }

        ThemeColor DefaultBackgroundColor { get; }
        ThemeColor DefaultForegroundColor { get; }
        ThemeColor DefaultBorderColor { get; }
        ThemeColor SelectionBackgroundColor { get; }
        ThemeColor SelectionForegroundColor { get; }
    }
}
