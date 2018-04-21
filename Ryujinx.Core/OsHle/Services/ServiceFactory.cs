using Ryujinx.Core.OsHle.Services.Acc;
using Ryujinx.Core.OsHle.Services.Am;
using Ryujinx.Core.OsHle.Services.Apm;
using Ryujinx.Core.OsHle.Services.Aud;
using Ryujinx.Core.OsHle.Services.Bsd;
using Ryujinx.Core.OsHle.Services.Friend;
using Ryujinx.Core.OsHle.Services.FspSrv;
using Ryujinx.Core.OsHle.Services.Hid;
using Ryujinx.Core.OsHle.Services.Lm;
using Ryujinx.Core.OsHle.Services.Ns;
using Ryujinx.Core.OsHle.Services.Nv;
using Ryujinx.Core.OsHle.Services.Pctl;
using Ryujinx.Core.OsHle.Services.Pl;
using Ryujinx.Core.OsHle.Services.Prepo;
using Ryujinx.Core.OsHle.Services.Set;
using Ryujinx.Core.OsHle.Services.Sfdnsres;
using Ryujinx.Core.OsHle.Services.Sm;
using Ryujinx.Core.OsHle.Services.Ssl;
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
                    return new IAccountServiceForApplication();

                case "aoc:u":
                    return new IAddOnContentManager();

                case "apm":
                    return new IManager();

                case "apm:p":
                    return new IManager();

                case "appletAE":
                    return new IAllSystemAppletProxiesService();

                case "appletOE":
                    return new IApplicationProxyService();

                case "audout:u":
                    return new IAudioOutManager();

                case "audren:u":
                    return new IAudioRendererManager();

                case "bsd:s":
                    return new IClient();

                case "bsd:u":
                    return new IClient();

                case "friend:a":
                    return new IServiceCreator();

                case "fsp-srv":
                    return new IFileSystemProxy();

                case "hid":
                    return new IHidServer();

                case "lm":
                    return new ILogService();

                case "nifm:u":
                    return new Nifm.IStaticService();

                case "ns:ec":
                    return new IServiceGetterInterface();

                case "ns:su":
                    return new ISystemUpdateInterface();

                case "ns:vm":
                    return new IVulnerabilityManagerInterface();

                case "nvdrv":
                    return new INvDrvServices();

                case "nvdrv:a":
                    return new INvDrvServices();

                case "pctl:a":
                    return new IParentalControlServiceFactory();

                case "pl:u":
                    return new ISharedFontManager();

                case "prepo:u":
                    return new IPrepoService();

                case "set":
                    return new ISettingsServer();

                case "set:sys":
                    return new ISystemSettingsServer();

                case "sfdnsres":
                    return new IResolver();

                case "sm:":
                    return new IUserInterface();

                case "ssl":
                    return new ISslService();

                case "time:a":
                    return new Time.IStaticService();

                case "time:s":
                    return new Time.IStaticService();

                case "time:u":
                    return new Time.IStaticService();

                case "vi:m":
                    return new IManagerRootService();

                case "vi:s":
                    return new ISystemRootService();

                case "vi:u":
                    return new IApplicationRootService();
            }

            throw new NotImplementedException(Name);
        }
    }
}