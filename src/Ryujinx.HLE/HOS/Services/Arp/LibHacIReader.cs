using LibHac;
using LibHac.Common;
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
                StorageId = StorageId.BuiltInUser,
                ApplicationId = ApplicationId,
            };

            return Result.Success;
        }

        public void Dispose() { }

        public Result GetApplicationLaunchPropertyWithApplicationId(out LibHac.Arp.ApplicationLaunchProperty launchProperty, ApplicationId applicationId)
        {
            launchProperty = new LibHac.Arp.ApplicationLaunchProperty
            {
                StorageId = StorageId.BuiltInUser,
                ApplicationId = applicationId,
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
        private SharedRef<LibHacIReader> _serviceObject;

        public LibHacArpServiceObject(ref SharedRef<LibHacIReader> serviceObject)
        {
            _serviceObject = SharedRef<LibHacIReader>.CreateCopy(in serviceObject);
        }

        public void Dispose()
        {
            _serviceObject.Destroy();
        }

        public Result GetServiceObject(ref SharedRef<IDisposable> serviceObject)
        {
            serviceObject.SetByCopy(in _serviceObject);

            return Result.Success;
        }
    }
}
