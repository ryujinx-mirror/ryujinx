using LibHac;
using LibHac.Arp.Impl;
using LibHac.Ncm;
using LibHac.Ns;
using System;

using ApplicationId = LibHac.ApplicationId;

namespace Ryujinx.HLE.HOS.Services.Arp
{
    class LibHacIReader : IReader
    {
        private Horizon System { get; }

        public LibHacIReader(Horizon system)
        {
            System = system;
        }

        public Result GetApplicationLaunchProperty(out LibHac.Arp.ApplicationLaunchProperty launchProperty, ulong processId)
        {
            launchProperty = new LibHac.Arp.ApplicationLaunchProperty();

            launchProperty.BaseStorageId = StorageId.BuiltInUser;
            launchProperty.ApplicationId = new ApplicationId(System.TitleId);

            return Result.Success;
        }

        public Result GetApplicationLaunchPropertyWithApplicationId(out LibHac.Arp.ApplicationLaunchProperty launchProperty,
            ApplicationId applicationId)
        {
            launchProperty = new LibHac.Arp.ApplicationLaunchProperty();

            launchProperty.BaseStorageId = StorageId.BuiltInUser;
            launchProperty.ApplicationId = applicationId;

            return Result.Success;
        }

        public Result GetApplicationControlProperty(out ApplicationControlProperty controlProperty, ulong processId)
        {
            throw new NotImplementedException();
        }

        public Result GetApplicationControlPropertyWithApplicationId(out ApplicationControlProperty controlProperty,
            ApplicationId applicationId)
        {
            throw new NotImplementedException();
        }
    }
}
