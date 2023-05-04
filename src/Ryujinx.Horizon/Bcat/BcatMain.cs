using Ryujinx.Horizon.LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryujinx.Horizon.Bcat
{
    internal class BcatMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            BcatIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
