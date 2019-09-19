namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl
{
    enum NvHostEventState
    {
        Registered = 0,
        Waiting    = 1,
        Busy       = 2,
        Free       = 5
    }
}