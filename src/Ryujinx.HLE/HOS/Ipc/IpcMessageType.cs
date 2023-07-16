namespace Ryujinx.HLE.HOS.Ipc
{
    enum IpcMessageType
    {
        CmifResponse = 0,
        CmifCloseSession = 2,
        CmifRequest = 4,
        CmifControl = 5,
        CmifRequestWithContext = 6,
        CmifControlWithContext = 7,
        TipcCloseSession = 0xF,
    }
}
