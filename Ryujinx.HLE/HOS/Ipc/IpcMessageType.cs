namespace Ryujinx.HLE.HOS.Ipc
{
    enum IpcMessageType
    {
        HipcResponse           = 0,
        HipcCloseSession       = 2,
        HipcRequest            = 4,
        HipcControl            = 5,
        HipcRequestWithContext = 6,
        HipcControlWithContext = 7,
        TipcCloseSession       = 0xF
    }
}