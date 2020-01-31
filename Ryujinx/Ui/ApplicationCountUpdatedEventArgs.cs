using System;

namespace Ryujinx.Ui
{
    public class ApplicationCountUpdatedEventArgs : EventArgs
    {
        public int NumAppsFound  { get; set; }
        public int NumAppsLoaded { get; set; }
    }
}