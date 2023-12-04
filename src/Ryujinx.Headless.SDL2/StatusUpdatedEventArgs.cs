using System;

namespace Ryujinx.Headless.SDL2
{
    class StatusUpdatedEventArgs : EventArgs
    {
        public bool VSyncEnabled;
        public string DockedMode;
        public string AspectRatio;
        public string GameStatus;
        public string FifoStatus;
        public string GpuName;

        public StatusUpdatedEventArgs(bool vSyncEnabled, string dockedMode, string aspectRatio, string gameStatus, string fifoStatus, string gpuName)
        {
            VSyncEnabled = vSyncEnabled;
            DockedMode = dockedMode;
            AspectRatio = aspectRatio;
            GameStatus = gameStatus;
            FifoStatus = fifoStatus;
            GpuName = gpuName;
        }
    }
}
