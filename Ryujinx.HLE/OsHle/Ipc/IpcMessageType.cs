namespace Ryujinx.HLE.OsHle.Ipc
{
    enum IpcMessageType
    {
        Response     = 0,
        CloseSession = 2,
        Request      = 4,
        Control      = 5
    }
}