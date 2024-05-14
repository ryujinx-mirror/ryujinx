using System;

namespace Ryujinx.Ava.Common
{
    public static class ThemeManager
    {
        public static event EventHandler ThemeChanged;

        public static void OnThemeChanged()
        {
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
