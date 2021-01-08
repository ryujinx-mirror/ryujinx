using Ryujinx.Common.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ryujinx.Ui.Helper
{
    static class OpenHelper
    {
        public static void OpenFolder(string path)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName        = path,
                UseShellExecute = true,
                Verb            = "open"
            });
        }

        public static void OpenUrl(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                Logger.Notice.Print(LogClass.Application, $"Cannot open url \"{url}\" on this platform!");
            }
        }
    }
}