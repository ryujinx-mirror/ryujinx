using Ryujinx.Core.OsHle.IpcServices.Acc;
using Ryujinx.Core.OsHle.IpcServices.Am;
using Ryujinx.Core.OsHle.IpcServices.Apm;
using Ryujinx.Core.OsHle.IpcServices.Aud;
using Ryujinx.Core.OsHle.IpcServices.Bsd;
using Ryujinx.Core.OsHle.IpcServices.Friend;
using Ryujinx.Core.OsHle.IpcServices.FspSrv;
using Ryujinx.Core.OsHle.IpcServices.Hid;
using Ryujinx.Core.OsHle.IpcServices.Lm;
using Ryujinx.Core.OsHle.IpcServices.Nifm;
using Ryujinx.Core.OsHle.IpcServices.Ns;
using Ryujinx.Core.OsHle.IpcServices.NvServices;
using Ryujinx.Core.OsHle.IpcServices.Pctl;
using Ryujinx.Core.OsHle.IpcServices.Pl;
using Ryujinx.Core.OsHle.IpcServices.Set;
using Ryujinx.Core.OsHle.IpcServices.Sfdnsres;
using Ryujinx.Core.OsHle.IpcServices.Sm;
using Ryujinx.Core.OsHle.IpcServices.Ssl;
using Ryujinx.Core.OsHle.IpcServices.Time;
using Ryujinx.Core.OsHle.IpcServices.Vi;
using System;

namespace Ryujinx.Core.OsHle.IpcServices
{
    static class ServiceFactory
    {
        public static IpcService MakeService(string Name)
        {
            switch (Name)
            {
                case "acc:u0":
                    return new ServiceAcc();

                case "aoc:u":
                    return new ServiceNs();

                case "apm":
                    return new ServiceApm();

                case "apm:p":
                    return new ServiceApm();

                case "appletOE":
                    return new ServiceAppletOE();

                case "audout:u":
                    return new ServiceAudOut();

                case "audren:u":
                    return new ServiceAudRen();

                case "bsd:s":
                    return new ServiceBsd();

                case "bsd:u":
                    return new ServiceBsd();

                case "friend:a":
                    return new ServiceFriend();

                case "fsp-srv":
                    return new ServiceFspSrv();

                case "hid":
                    return new ServiceHid();

                case "lm":
                    return new ServiceLm();

                case "nifm:u":
                    return new ServiceNifm();

                case "nvdrv":
                    return new ServiceNvDrv();

                case "nvdrv:a":
                    return new ServiceNvDrv();

                case "pctl:a":
                    return new ServicePctl();

                case "pl:u":
                    return new ServicePl();

                case "set":
                    return new ServiceSet();

                case "set:sys":
                    return new ServiceSetSys();

                case "sfdnsres":
                    return new ServiceSfdnsres();

                case "sm:":
                    return new ServiceSm();

                case "ssl":
                    return new ServiceSsl();

                case "time:s":
                    return new ServiceTime();

                case "time:u":
                    return new ServiceTime();

                case "vi:m":
                    return new ServiceVi();

                case "vi:s":
                    return new ServiceVi();

                case "vi:u":
                    return new ServiceVi();
            }

            throw new NotImplementedException(Name);
        }
    }
}