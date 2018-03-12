using Ryujinx.Core.OsHle.IpcServices;
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
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle
{
    class ServiceMgr : IDisposable
    {
        private Dictionary<string, IIpcService> Services;

        public ServiceMgr()
        {
            Services = new Dictionary<string, IIpcService>();
        }

        public IIpcService GetService(string Name)
        {
            lock (Services)
            {
                if (!Services.TryGetValue(Name, out IIpcService Service))
                {
                    switch (Name)
                    {
                        case "acc:u0":   Service = new ServiceAcc();      break;
                        case "aoc:u":    Service = new ServiceNs();       break;
                        case "apm":      Service = new ServiceApm();      break;
                        case "apm:p":    Service = new ServiceApm();      break;
                        case "appletOE": Service = new ServiceAppletOE(); break;
                        case "audout:u": Service = new ServiceAudOut();   break;
                        case "audren:u": Service = new ServiceAudRen();   break;
                        case "bsd:s":    Service = new ServiceBsd();      break;
                        case "bsd:u":    Service = new ServiceBsd();      break;
                        case "friend:a": Service = new ServiceFriend();   break;
                        case "fsp-srv":  Service = new ServiceFspSrv();   break;
                        case "hid":      Service = new ServiceHid();      break;
                        case "lm":       Service = new ServiceLm();       break;
                        case "nifm:u":   Service = new ServiceNifm();     break;
                        case "nvdrv":    Service = new ServiceNvDrv();    break;
                        case "nvdrv:a":  Service = new ServiceNvDrv();    break;
                        case "pctl:a":   Service = new ServicePctl();     break;
                        case "pl:u":     Service = new ServicePl();       break;
                        case "set":      Service = new ServiceSet();      break;
                        case "set:sys":  Service = new ServiceSetSys();   break;
                        case "sfdnsres": Service = new ServiceSfdnsres(); break;
                        case "sm:":      Service = new ServiceSm();       break;
                        case "ssl":      Service = new ServiceSsl();      break;
                        case "time:s":   Service = new ServiceTime();     break;
                        case "time:u":   Service = new ServiceTime();     break;
                        case "vi:m":     Service = new ServiceVi();       break;
                        case "vi:s":     Service = new ServiceVi();       break;
                        case "vi:u":     Service = new ServiceVi();       break;
                    }

                    if (Service == null)
                    {
                        throw new NotImplementedException(Name);
                    }

                    Services.Add(Name, Service);
                }

                return Service;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                lock (Services)
                {
                    foreach (IIpcService Service in Services.Values)
                    {
                        if (Service is IDisposable DisposableSrv)
                        {
                            DisposableSrv.Dispose();
                        }
                    }

                    Services.Clear();
                }
            }
        }
    }
}