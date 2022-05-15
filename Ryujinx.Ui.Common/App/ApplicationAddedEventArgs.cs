using System;

namespace Ryujinx.Ui.App.Common
{
    public class ApplicationAddedEventArgs : EventArgs
    {
        public ApplicationData AppData { get; set; }
    }
}