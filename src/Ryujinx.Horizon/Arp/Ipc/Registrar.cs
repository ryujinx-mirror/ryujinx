using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Arp;
using Ryujinx.Horizon.Sdk.Arp.Detail;
using Ryujinx.Horizon.Sdk.Ns;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Arp.Ipc
{
    partial class Registrar : IRegistrar, IServiceObject
    {
        private readonly ApplicationInstance _applicationInstance;

        public Registrar(ApplicationInstance applicationInstance)
        {
            _applicationInstance = applicationInstance;
        }

        [CmifCommand(0)]
        public Result Issue(out ulong applicationInstanceId)
        {
            throw new NotImplementedException();
        }

        [CmifCommand(1)]
        public Result SetApplicationLaunchProperty(ApplicationLaunchProperty applicationLaunchProperty)
        {
            if (_applicationInstance.LaunchProperty != null)
            {
                return ArpResult.DataAlreadyBound;
            }

            _applicationInstance.LaunchProperty = applicationLaunchProperty;

            return Result.Success;
        }

        [CmifCommand(2)]
        public Result SetApplicationControlProperty([Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias | HipcBufferFlags.FixedSize, 0x4000)] in ApplicationControlProperty applicationControlProperty)
        {
            if (_applicationInstance.ControlProperty != null)
            {
                return ArpResult.DataAlreadyBound;
            }

            _applicationInstance.ControlProperty = applicationControlProperty;

            return Result.Success;
        }
    }
}
