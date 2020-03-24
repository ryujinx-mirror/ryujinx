using System;

namespace Ryujinx.Ui
{
    public class StatusUpdatedEventArgs : EventArgs
    {
        public bool   VSyncEnabled;
        public string HostStatus;
        public string GameStatus;
        public string GpuName;

        public StatusUpdatedEventArgs(bool vSyncEnabled, string hostStatus, string gameStatus, string gpuName)
        {
            VSyncEnabled = vSyncEnabled;
            HostStatus   = hostStatus;
            GameStatus   = gameStatus;
            GpuName      = gpuName;
        }
    }
}