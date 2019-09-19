namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    static class AudioRendererConsts
    {
        // Revision Consts
        public const int Revision  = 7;
        public const int Rev0Magic = ('R' << 0) | ('E' << 8) | ('V' << 16) | ('0' << 24);
        public const int RevMagic  = Rev0Magic + (Revision << 24);

        // Misc Consts
        public const int BufferAlignment = 0x40;

        // Host Consts
        public const int HostSampleRate    = 48000;
        public const int HostChannelsCount = 2;
    }
}