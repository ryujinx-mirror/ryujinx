namespace Ryujinx.HLE.HOS.Services.Ldr
{
    static class LoaderErr
    {
        public const int InvalidMemoryState = 51;
        public const int InvalidNro         = 52;
        public const int InvalidNrr         = 53;
        public const int MaxNro             = 55;
        public const int MaxNrr             = 56;
        public const int NroAlreadyLoaded   = 57;
        public const int NroHashNotPresent  = 54;
        public const int UnalignedAddress   = 81;
        public const int BadSize            = 82;
        public const int BadNroAddress      = 84;
        public const int BadNrrAddress      = 85;
        public const int BadInitialization  = 87;
    }
}
