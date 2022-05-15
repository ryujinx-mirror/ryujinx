using System;

namespace Ryujinx.Ava.Ui.Models
{
    public class StatusUpdatedEventArgs : EventArgs
    {
        public bool VSyncEnabled { get; }
        public float Volume { get; }
        public string AspectRatio { get; }
        public string DockedMode { get; }
        public string FifoStatus { get; }
        public string GameStatus { get; }
        public string GpuName { get; }

        public StatusUpdatedEventArgs(bool vSyncEnabled, float volume, string dockedMode, string aspectRatio, string gameStatus, string fifoStatus, string gpuName)
        {
            VSyncEnabled = vSyncEnabled;
            Volume = volume;
            DockedMode = dockedMode;
            AspectRatio = aspectRatio;
            GameStatus = gameStatus;
            FifoStatus = fifoStatus;
            GpuName = gpuName;
        }
    }
}