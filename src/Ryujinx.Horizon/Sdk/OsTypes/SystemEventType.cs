namespace Ryujinx.Horizon.Sdk.OsTypes
{
    struct SystemEventType
    {
        public enum InitializationState : byte
        {
            NotInitialized,
            InitializedAsEvent,
            InitializedAsInterProcess
        }

        public InterProcessEventType InterProcessEvent;
        public InitializationState State;

        public bool NotInitialized => State == InitializationState.NotInitialized;
    }
}
