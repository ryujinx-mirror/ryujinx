namespace Ryujinx.Audio.Backends.SoundIo.Native
{
    public enum SoundIoError
    {
        None = 0,
        NoMem = 1,
        InitAudioBackend = 2,
        SystemResources = 3,
        OpeningDevice = 4,
        NoSuchDevice = 5,
        Invalid = 6,
        BackendUnavailable = 7,
        Streaming = 8,
        IncompatibleDevice = 9,
        NoSuchClient = 10,
        IncompatibleBackend = 11,
        BackendDisconnected = 12,
        Interrupted = 13,
        Underflow = 14,
        EncodingString = 15,
    }
}
