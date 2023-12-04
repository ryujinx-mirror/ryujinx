using Ryujinx.Common.Memory;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf.Cmif;
using System;

namespace Ryujinx.Horizon.Sdk.Sm
{
    public class SmApi : IDisposable
    {
        private const string SmName = "sm:";

        private int _portHandle;

        public Result Initialize()
        {
            Result result = HorizonStatic.Syscall.ConnectToNamedPort(out int portHandle, SmName);

            while (result == KernelResult.NotFound)
            {
                HorizonStatic.Syscall.SleepThread(50000000L);
                result = HorizonStatic.Syscall.ConnectToNamedPort(out portHandle, SmName);
            }

            if (result.IsFailure)
            {
                return result;
            }

            _portHandle = portHandle;

            return RegisterClient();
        }

        private Result RegisterClient()
        {
            Span<byte> data = stackalloc byte[8];

            SpanWriter writer = new(data);

            writer.Write(0UL);

            return ServiceUtil.SendRequest(out _, _portHandle, 0, sendPid: true, data);
        }

        public Result GetServiceHandle(out int handle, ServiceName name)
        {
            Span<byte> data = stackalloc byte[8];

            SpanWriter writer = new(data);

            writer.Write(name);

            Result result = ServiceUtil.SendRequest(out CmifResponse response, _portHandle, 1, sendPid: false, data);

            if (result.IsFailure)
            {
                handle = 0;

                return result;
            }

            handle = response.MoveHandles[0];

            return Result.Success;
        }

        public Result RegisterService(out int handle, ServiceName name, int maxSessions, bool isLight)
        {
            Span<byte> data = stackalloc byte[16];

            SpanWriter writer = new(data);

            writer.Write(name);
            writer.Write(isLight ? 1 : 0);
            writer.Write(maxSessions);

            Result result = ServiceUtil.SendRequest(out CmifResponse response, _portHandle, 2, sendPid: false, data);

            if (result.IsFailure)
            {
                handle = 0;

                return result;
            }

            handle = response.MoveHandles[0];

            return Result.Success;
        }

        public Result UnregisterService(ServiceName name)
        {
            Span<byte> data = stackalloc byte[8];

            SpanWriter writer = new(data);

            writer.Write(name);

            return ServiceUtil.SendRequest(out _, _portHandle, 3, sendPid: false, data);
        }

        public Result DetachClient()
        {
            Span<byte> data = stackalloc byte[8];

            SpanWriter writer = new(data);

            writer.Write(0UL);

            return ServiceUtil.SendRequest(out _, _portHandle, 4, sendPid: true, data);
        }

        public void Dispose()
        {
            if (_portHandle != 0)
            {
                HorizonStatic.Syscall.CloseHandle(_portHandle);

                _portHandle = 0;
            }

            GC.SuppressFinalize(this);
        }
    }
}
