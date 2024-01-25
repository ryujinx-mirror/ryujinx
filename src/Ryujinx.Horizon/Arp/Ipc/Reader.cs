using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Arp;
using Ryujinx.Horizon.Sdk.Arp.Detail;
using Ryujinx.Horizon.Sdk.Ns;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Arp.Ipc
{
    partial class Reader : IReader, IServiceObject
    {
        private readonly ApplicationInstanceManager _applicationInstanceManager;

        public Reader(ApplicationInstanceManager applicationInstanceManager)
        {
            _applicationInstanceManager = applicationInstanceManager;
        }

        [CmifCommand(0)]
        public Result GetApplicationLaunchProperty(out ApplicationLaunchProperty applicationLaunchProperty, ulong applicationInstanceId)
        {
            if (_applicationInstanceManager.Entries[applicationInstanceId] == null || !_applicationInstanceManager.Entries[applicationInstanceId].LaunchProperty.HasValue)
            {
                applicationLaunchProperty = default;

                return ArpResult.InvalidInstanceId;
            }

            applicationLaunchProperty = _applicationInstanceManager.Entries[applicationInstanceId].LaunchProperty.Value;

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result GetApplicationControlProperty([Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias, 0x4000)] out ApplicationControlProperty applicationControlProperty, ulong applicationInstanceId)
        {
            if (_applicationInstanceManager.Entries[applicationInstanceId] == null || !_applicationInstanceManager.Entries[applicationInstanceId].ControlProperty.HasValue)
            {
                applicationControlProperty = default;

                return ArpResult.InvalidInstanceId;
            }

            applicationControlProperty = _applicationInstanceManager.Entries[applicationInstanceId].ControlProperty.Value;

            return Result.Success;
        }

        [CmifCommand(2)]
        public Result GetApplicationProcessProperty(out ApplicationProcessProperty applicationProcessProperty, ulong applicationInstanceId)
        {
            if (_applicationInstanceManager.Entries[applicationInstanceId] == null || !_applicationInstanceManager.Entries[applicationInstanceId].ProcessProperty.HasValue)
            {
                applicationProcessProperty = default;

                return ArpResult.InvalidInstanceId;
            }

            applicationProcessProperty = _applicationInstanceManager.Entries[applicationInstanceId].ProcessProperty.Value;

            return Result.Success;
        }

        [CmifCommand(3)]
        public Result GetApplicationInstanceId(out ulong applicationInstanceId, ulong pid)
        {
            applicationInstanceId = 0;

            if (pid == 0)
            {
                return ArpResult.InvalidPid;
            }

            for (int i = 0; i < _applicationInstanceManager.Entries.Length; i++)
            {
                if (_applicationInstanceManager.Entries[i] != null && _applicationInstanceManager.Entries[i].Pid == pid)
                {
                    applicationInstanceId = (ulong)i;

                    return Result.Success;
                }
            }

            return ArpResult.InvalidPid;
        }

        [CmifCommand(4)]
        public Result GetApplicationInstanceUnregistrationNotifier(out IUnregistrationNotifier unregistrationNotifier)
        {
            unregistrationNotifier = new UnregistrationNotifier(_applicationInstanceManager);

            return Result.Success;
        }

        [CmifCommand(5)]
        public Result ListApplicationInstanceId(out int count, [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias)] Span<ulong> applicationInstanceIdList)
        {
            count = 0;

            if (_applicationInstanceManager.Entries[0] != null)
            {
                applicationInstanceIdList[count++] = 0;
            }

            if (_applicationInstanceManager.Entries[1] != null)
            {
                applicationInstanceIdList[count++] = 1;
            }

            return Result.Success;
        }

        [CmifCommand(6)]
        public Result GetMicroApplicationInstanceId(out ulong microApplicationInstanceId, [ClientProcessId] ulong pid)
        {
            return GetApplicationInstanceId(out microApplicationInstanceId, pid);
        }

        [CmifCommand(7)]
        public Result GetApplicationCertificate([Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias | HipcBufferFlags.FixedSize, 0x528)] out ApplicationCertificate applicationCertificate, ulong applicationInstanceId)
        {
            if (_applicationInstanceManager.Entries[applicationInstanceId] == null)
            {
                applicationCertificate = default;

                return ArpResult.InvalidInstanceId;
            }

            applicationCertificate = _applicationInstanceManager.Entries[applicationInstanceId].Certificate.Value;

            return Result.Success;
        }
    }
}
