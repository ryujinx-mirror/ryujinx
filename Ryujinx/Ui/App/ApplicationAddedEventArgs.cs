using System;

namespace Ryujinx.Ui.App
{
    public class ApplicationAddedEventArgs : EventArgs
    {
        public ApplicationData AppData { get; set; }
    }
}