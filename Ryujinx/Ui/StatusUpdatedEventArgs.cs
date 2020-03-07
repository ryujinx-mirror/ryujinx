using System;

namespace Ryujinx.Ui
{
    public class StatusUpdatedEventArgs : EventArgs
    {
        public bool   VSyncEnabled;
        public string HostStatus;
        public string GameStatus;

        public StatusUpdatedEventArgs(bool vSyncEnabled, string hostStatus, string gameStatus)
        {
            VSyncEnabled = vSyncEnabled;
            HostStatus   = hostStatus;
            GameStatus   = gameStatus;
        }
    }
}