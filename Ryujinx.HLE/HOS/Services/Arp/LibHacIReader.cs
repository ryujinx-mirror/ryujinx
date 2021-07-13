using LibHac;
using LibHac.Ncm;
using LibHac.Ns;
using System;

using ApplicationId = LibHac.ApplicationId;

namespace Ryujinx.HLE.HOS.Services.Arp
{
    class LibHacIReader : LibHac.Arp.Impl.IReader
    {
        public ApplicationId ApplicationId { get; set; }

        public Result GetApplicationLaunchProperty(out LibHac.Arp.ApplicationLaunchProperty launchProperty, ulong processId)
        {
            launchProperty = new LibHac.Arp.ApplicationLaunchProperty
            {
                BaseStorageId = StorageId.BuiltInUser,
                ApplicationId = ApplicationId
            };

            return Result.Success;
        }

        public Result GetApplicationLaunchPropertyWithApplicationId(out LibHac.Arp.ApplicationLaunchProperty launchProperty, ApplicationId applicationId)
        {
            launchProperty = new LibHac.Arp.ApplicationLaunchProperty
            {
                BaseStorageId = StorageId.BuiltInUser,
                ApplicationId = applicationId
            };

            return Result.Success;
        }

        public Result GetApplicationControlProperty(out ApplicationControlProperty controlProperty, ulong processId)
        {
            throw new NotImplementedException();
        }

        public Result GetApplicationControlPropertyWithApplicationId(out ApplicationControlProperty controlProperty, ApplicationId applicationId)
        {
            throw new NotImplementedException();
        }

        public Result GetServiceObject(out object serviceObject)
        {
            throw new NotImplementedException();
        }
    }

    internal class LibHacArpServiceObject : LibHac.Sm.IServiceObject
    {
        private LibHacIReader _serviceObject;

        public LibHacArpServiceObject(LibHacIReader serviceObject)
        {
            _serviceObject = serviceObject;
        }

        public Result GetServiceObject(out object serviceObject)
        {
            serviceObject = _serviceObject;

            return Result.Success;
        }
    }
}