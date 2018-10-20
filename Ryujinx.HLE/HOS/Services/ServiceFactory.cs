using Ryujinx.HLE.HOS.Services.Acc;
using Ryujinx.HLE.HOS.Services.Am;
using Ryujinx.HLE.HOS.Services.Apm;
using Ryujinx.HLE.HOS.Services.Aud;
using Ryujinx.HLE.HOS.Services.Bsd;
using Ryujinx.HLE.HOS.Services.Caps;
using Ryujinx.HLE.HOS.Services.FspSrv;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.Services.Irs;
using Ryujinx.HLE.HOS.Services.Ldr;
using Ryujinx.HLE.HOS.Services.Lm;
using Ryujinx.HLE.HOS.Services.Mm;
using Ryujinx.HLE.HOS.Services.Nfp;
using Ryujinx.HLE.HOS.Services.Ns;
using Ryujinx.HLE.HOS.Services.Nv;
using Ryujinx.HLE.HOS.Services.Pctl;
using Ryujinx.HLE.HOS.Services.Pl;
using Ryujinx.HLE.HOS.Services.Prepo;
using Ryujinx.HLE.HOS.Services.Set;
using Ryujinx.HLE.HOS.Services.Sfdnsres;
using Ryujinx.HLE.HOS.Services.Sm;
using Ryujinx.HLE.HOS.Services.Spl;
using Ryujinx.HLE.HOS.Services.Ssl;
using Ryujinx.HLE.HOS.Services.Vi;
using System;

namespace Ryujinx.HLE.HOS.Services
{
    static class ServiceFactory
    {
        public static IpcService MakeService(Horizon System, string Name)
        {
            switch (Name)
            {
                case "acc:u0":
                    return new IAccountService();

                case "acc:u1":
                    return new IAccountService();

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

                case "bcat:a":
                    return new Bcat.IServiceCreator();

                case "bcat:m":
                    return new Bcat.IServiceCreator();

                case "bcat:u":
                    return new Bcat.IServiceCreator();

                case "bcat:s":
                    return new Bcat.IServiceCreator();

                case "bsd:s":
                    return new IClient(true);

                case "bsd:u":
                    return new IClient(false);

                case "caps:a":
                    return new IAlbumAccessorService();

                case "caps:ss":
                    return new IScreenshotService();

                case "csrng":
                    return new IRandomInterface();

                case "friend:a":
                    return new Friend.IServiceCreator();

                case "friend:u":
                    return new Friend.IServiceCreator();

                case "fsp-srv":
                    return new IFileSystemProxy();

                case "hid":
                    return new IHidServer(System);

                case "irs":
                    return new IIrSensorServer();

                case "ldr:ro":
                    return new IRoInterface();

                case "lm":
                    return new ILogService();

                case "mm:u":
                    return new IRequest();

                case "nfp:user":
                    return new IUserManager();

                case "nifm:u":
                    return new Nifm.IStaticService();

                case "ns:ec":
                    return new IServiceGetterInterface();

                case "ns:su":
                    return new ISystemUpdateInterface();

                case "ns:vm":
                    return new IVulnerabilityManagerInterface();

                case "nvdrv":
                    return new INvDrvServices(System);

                case "nvdrv:a":
                    return new INvDrvServices(System);

                case "pctl:s":
                    return new IParentalControlServiceFactory();

                case "pctl:r":
                    return new IParentalControlServiceFactory();

                case "pctl:a":
                    return new IParentalControlServiceFactory();

                case "pctl":
                    return new IParentalControlServiceFactory();

                case "pl:u":
                    return new ISharedFontManager();

                case "prepo:a":
                    return new IPrepoService();

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
