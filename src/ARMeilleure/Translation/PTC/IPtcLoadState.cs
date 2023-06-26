using System;

namespace ARMeilleure.Translation.PTC
{
    public interface IPtcLoadState
    {
        event Action<PtcLoadingState, int, int> PtcStateChanged;
        void Continue();
    }
}
