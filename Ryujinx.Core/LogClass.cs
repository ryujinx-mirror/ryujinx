using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.Core
{
    public enum LogClass
    {
        Audio,
        CPU,
        GPU,
        Kernel,
        KernelIpc,
        KernelScheduler,
        KernelSvc,
        Loader,
        Service,
        ServiceAcc,
        ServiceAm,
        ServiceApm,
        ServiceAudio,
        ServiceBsd,
        ServiceFriend,
        ServiceFs,
        ServiceHid,
        ServiceLm,
        ServiceNifm,
        ServiceNs,
        ServiceNv,
        ServicePctl,
        ServicePl,
        ServiceSet,
        ServiceSfdnsres,
        ServiceSm,
        ServiceSss,
        ServiceTime,
        ServiceVi,
        Count,
    }
}
