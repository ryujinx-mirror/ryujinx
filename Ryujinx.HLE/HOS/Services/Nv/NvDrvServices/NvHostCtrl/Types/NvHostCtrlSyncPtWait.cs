namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl
{
    struct NvHostCtrlSyncptWait
    {
        public int Id;
        public int Thresh;
        public int Timeout;
    }
}