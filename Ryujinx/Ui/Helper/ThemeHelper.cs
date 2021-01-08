using Gtk;
using Ryujinx.Common.Logging;
using Ryujinx.Configuration;
using System.IO;

namespace Ryujinx.Ui.Helper
{
    static class ThemeHelper
    {
        public static void ApplyTheme()
        {
            if (!ConfigurationState.Instance.Ui.EnableCustomTheme)
            {
                return;
            }

            if (File.Exists(ConfigurationState.Instance.Ui.CustomThemePath) && (Path.GetExtension(ConfigurationState.Instance.Ui.CustomThemePath) == ".css"))
            {
                CssProvider cssProvider = new CssProvider();

                cssProvider.LoadFromPath(ConfigurationState.Instance.Ui.CustomThemePath);

                StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800);
            }
            else
            {
                Logger.Warning?.Print(LogClass.Application, $"The \"custom_theme_path\" section in \"Config.json\" contains an invalid path: \"{ConfigurationState.Instance.Ui.CustomThemePath}\".");

                ConfigurationState.Instance.Ui.CustomThemePath.Value   = "";
                ConfigurationState.Instance.Ui.EnableCustomTheme.Value = false;
                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
            }
        }
    }
}