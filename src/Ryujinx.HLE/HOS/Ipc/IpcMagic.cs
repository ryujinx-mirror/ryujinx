namespace Ryujinx.HLE.HOS.Ipc
{
    abstract class IpcMagic
    {
        public const long Sfci = 'S' << 0 | 'F' << 8 | 'C' << 16 | 'I' << 24;
        public const long Sfco = 'S' << 0 | 'F' << 8 | 'C' << 16 | 'O' << 24;
    }
}
