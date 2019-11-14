using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using System;

namespace Ryujinx.HLE.HOS.Applets
{
    interface IApplet
    {
        event EventHandler AppletStateChanged;

        ResultCode Start(AppletFifo<byte[]> inData, AppletFifo<byte[]> outData);
        ResultCode GetResult();
    }
}
