using System;

namespace Ryujinx.Ui
{
    public class StatusUpdatedEventArgs : EventArgs
    {
        public bool   VSyncEnabled;
        public string DockedMode;
        public string HostStatus;
        public string GameStatus;
        public string GpuName;

        public StatusUpdatedEventArgs(bool vSyncEnabled, string dockedMode, string hostStatus, string gameStatus, string gpuName)
        {
            VSyncEnabled = vSyncEnabled;
            DockedMode   = dockedMode;
            HostStatus   = hostStatus;
            GameStatus   = gameStatus;
            GpuName      = gpuName;
        }
    }
}
