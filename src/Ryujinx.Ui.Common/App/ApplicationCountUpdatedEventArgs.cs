using System;

namespace Ryujinx.Ui.App.Common
{
    public class ApplicationCountUpdatedEventArgs : EventArgs
    {
        public int NumAppsFound { get; set; }
        public int NumAppsLoaded { get; set; }
    }
}
