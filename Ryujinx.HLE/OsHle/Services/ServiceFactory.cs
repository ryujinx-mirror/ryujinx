using Ryujinx.HLE.OsHle.Services.Acc;
using Ryujinx.HLE.OsHle.Services.Am;
using Ryujinx.HLE.OsHle.Services.Apm;
using Ryujinx.HLE.OsHle.Services.Aud;
using Ryujinx.HLE.OsHle.Services.Bcat;
using Ryujinx.HLE.OsHle.Services.Bsd;
using Ryujinx.HLE.OsHle.Services.Caps;
using Ryujinx.HLE.OsHle.Services.Friend;
using Ryujinx.HLE.OsHle.Services.FspSrv;
using Ryujinx.HLE.OsHle.Services.Hid;
using Ryujinx.HLE.OsHle.Services.Lm;
using Ryujinx.HLE.OsHle.Services.Mm;
using Ryujinx.HLE.OsHle.Services.Nfp;
using Ryujinx.HLE.OsHle.Services.Ns;
using Ryujinx.HLE.OsHle.Services.Nv;
using Ryujinx.HLE.OsHle.Services.Pctl;
using Ryujinx.HLE.OsHle.Services.Pl;
using Ryujinx.HLE.OsHle.Services.Prepo;
using Ryujinx.HLE.OsHle.Services.Set;
using Ryujinx.HLE.OsHle.Services.Sfdnsres;
using Ryujinx.HLE.OsHle.Services.Sm;
using Ryujinx.HLE.OsHle.Services.Spl;
using Ryujinx.HLE.OsHle.Services.Ssl;
using Ryujinx.HLE.OsHle.Services.Vi;
using System;

namespace Ryujinx.HLE.OsHle.Services
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

                case "bcat:a":
                    return new Bcat.IServiceCreator();

                case "bcat:m":
                    return new Bcat.IServiceCreator();

                case "bcat:u":
                    return new Bcat.IServiceCreator();

                case "bcat:s":
                    return new Bcat.IServiceCreator();

                case "bsd:s":
                    return new IClient();

                case "bsd:u":
                    return new IClient();

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
                    return new IHidServer();

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
                    return new INvDrvServices();

                case "nvdrv:a":
                    return new INvDrvServices();

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
