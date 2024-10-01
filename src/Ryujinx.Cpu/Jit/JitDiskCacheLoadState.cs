using ARMeilleure.Translation.PTC;
using System;

namespace Ryujinx.Cpu.Jit
{
    public class JitDiskCacheLoadState : IDiskCacheLoadState
    {
        /// <inheritdoc/>
        public event Action<LoadState, int, int> StateChanged;

        private readonly IPtcLoadState _loadState;

        public JitDiskCacheLoadState(IPtcLoadState loadState)
        {
            loadState.PtcStateChanged += LoadStateChanged;
            _loadState = loadState;
        }

        private void LoadStateChanged(PtcLoadingState newState, int current, int total)
        {
            LoadState state = newState switch
            {
                PtcLoadingState.Start => LoadState.Unloaded,
                PtcLoadingState.Loading => LoadState.Loading,
                PtcLoadingState.Loaded => LoadState.Loaded,
                _ => throw new ArgumentException($"Invalid load state \"{newState}\"."),
            };

            StateChanged?.Invoke(state, current, total);
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            _loadState.Continue();
        }
    }
}
