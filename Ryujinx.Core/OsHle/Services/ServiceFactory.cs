using Ryujinx.Core.OsHle.Services.Acc;
using Ryujinx.Core.OsHle.Services.Am;
using Ryujinx.Core.OsHle.Services.Apm;
using Ryujinx.Core.OsHle.Services.Aud;
using Ryujinx.Core.OsHle.Services.Bsd;
using Ryujinx.Core.OsHle.Services.Friend;
using Ryujinx.Core.OsHle.Services.FspSrv;
using Ryujinx.Core.OsHle.Services.Hid;
using Ryujinx.Core.OsHle.Services.Lm;
using Ryujinx.Core.OsHle.Services.Nifm;
using Ryujinx.Core.OsHle.Services.Ns;
using Ryujinx.Core.OsHle.Services.Nv;
using Ryujinx.Core.OsHle.Services.Pctl;
using Ryujinx.Core.OsHle.Services.Pl;
using Ryujinx.Core.OsHle.Services.Set;
using Ryujinx.Core.OsHle.Services.Sfdnsres;
using Ryujinx.Core.OsHle.Services.Sm;
using Ryujinx.Core.OsHle.Services.Ssl;
using Ryujinx.Core.OsHle.Services.Time;
using Ryujinx.Core.OsHle.Services.Vi;
using System;

namespace Ryujinx.Core.OsHle.Services
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
                    return new IAudioOutManager();

                case "audren:u":
                    return new IAudioRendererManager();

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