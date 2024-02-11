using System;

namespace Ryujinx.UI.App.Common
{
    public class ApplicationAddedEventArgs : EventArgs
    {
        public ApplicationData AppData { get; set; }
    }
}
