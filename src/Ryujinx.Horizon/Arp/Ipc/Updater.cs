using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Arp;
using Ryujinx.Horizon.Sdk.Arp.Detail;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Arp.Ipc
{
    partial class Updater : IUpdater, IServiceObject
    {
        private readonly ApplicationInstanceManager _applicationInstanceManager;
        private readonly ulong _applicationInstanceId;
        private readonly bool _forCertificate;

        public Updater(ApplicationInstanceManager applicationInstanceManager, ulong applicationInstanceId, bool forCertificate)
        {
            _applicationInstanceManager = applicationInstanceManager;
            _applicationInstanceId = applicationInstanceId;
            _forCertificate = forCertificate;
        }

        [CmifCommand(0)]
        public Result Issue()
        {
            throw new NotImplementedException();
        }

        [CmifCommand(1)]
        public Result SetApplicationProcessProperty(ulong pid, ApplicationProcessProperty applicationProcessProperty)
        {
            if (_forCertificate)
            {
                return ArpResult.DataAlreadyBound;
            }

            if (pid == 0)
            {
                return ArpResult.InvalidPid;
            }

            _applicationInstanceManager.Entries[_applicationInstanceId].Pid = pid;
            _applicationInstanceManager.Entries[_applicationInstanceId].ProcessProperty = applicationProcessProperty;

            return Result.Success;
        }

        [CmifCommand(2)]
        public Result DeleteApplicationProcessProperty()
        {
            if (_forCertificate)
            {
                return ArpResult.DataAlreadyBound;
            }

            _applicationInstanceManager.Entries[_applicationInstanceId].ProcessProperty = new ApplicationProcessProperty();

            return Result.Success;
        }

        [CmifCommand(3)]
        public Result SetApplicationCertificate([Buffer(HipcBufferFlags.In | HipcBufferFlags.AutoSelect)] ApplicationCertificate applicationCertificate)
        {
            if (!_forCertificate)
            {
                return ArpResult.DataAlreadyBound;
            }

            _applicationInstanceManager.Entries[_applicationInstanceId].Certificate = applicationCertificate;

            return Result.Success;
        }
    }
}
