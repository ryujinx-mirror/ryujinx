using Ryujinx.Common.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.Ui.Common.Helper
{
    public static partial class OpenHelper
    {
        [LibraryImport("shell32.dll", SetLastError = true)]
        public static partial int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr apidl, uint dwFlags);

        [LibraryImport("shell32.dll", SetLastError = true)]
        public static partial void ILFree(IntPtr pidlList);

        [LibraryImport("shell32.dll", SetLastError = true)]
        public static partial IntPtr ILCreateFromPathW([MarshalAs(UnmanagedType.LPWStr)] string pszPath);

        public static void OpenFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName        = path,
                    UseShellExecute = true,
                    Verb            = "open"
                });
            }
            else
            {
                Logger.Notice.Print(LogClass.Application, $"Directory \"{path}\" doesn't exist!");
            }
        }

        public static void LocateFile(string path)
        {
            if (File.Exists(path))
            {
                if (OperatingSystem.IsWindows())
                {
                    IntPtr pidlList = ILCreateFromPathW(path);
                    if (pidlList != IntPtr.Zero)
                    {
                        try
                        {
                            Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
                        }
                        finally
                        {
                            ILFree(pidlList);
                        }
                    }
                }
                else if (OperatingSystem.IsMacOS())
                {
                    Process.Start("open", $"-R \"{path}\"");
                }
                else if (OperatingSystem.IsLinux())
                {
                    Process.Start("dbus-send", $"--session --print-reply --dest=org.freedesktop.FileManager1 --type=method_call /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems array:string:\"file://{path}\" string:\"\"");
                }
                else
                {
                    OpenFolder(Path.GetDirectoryName(path));
                }
            }
            else
            {
                Logger.Notice.Print(LogClass.Application, $"File \"{path}\" doesn't exist!");
            }
        }

        public static void OpenUrl(string url)
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}"));
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", url);
            }
            else if (OperatingSystem.IsMacOS())
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