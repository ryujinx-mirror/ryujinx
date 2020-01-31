using System;

namespace Ryujinx.Ui
{
    public class ApplicationAddedEventArgs : EventArgs
    {
        public ApplicationData AppData { get; set; }
    }
}