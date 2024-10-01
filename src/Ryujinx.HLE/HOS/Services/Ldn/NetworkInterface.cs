using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Ldn.Types;
using System.Net;

namespace Ryujinx.HLE.HOS.Services.Ldn
{
    internal class NetworkInterface
    {
        public ResultCode NifmState { get; set; }
        public KEvent StateChangeEvent { get; private set; }

        private NetworkState _state;

        public NetworkInterface(Horizon system)
        {
            // TODO(Ac_K): Determine where the internal state is set.
            NifmState = ResultCode.Success;
            StateChangeEvent = new KEvent(system.KernelContext);

            _state = NetworkState.None;
        }

        public ResultCode Initialize(int unknown, int version, IPAddress ipv4Address, IPAddress subnetMaskAddress)
        {
            // TODO(Ac_K): Call nn::nifm::InitializeSystem().
            //             If the call failed, it returns the result code.
            //             If the call succeed, it signal and clear an event then start a new thread named nn.ldn.NetworkInterfaceMonitor.

            Logger.Stub?.PrintStub(LogClass.ServiceLdn, new { version });

            // NOTE: Since we don't support ldn for now, we can return this following result code to make it disabled.
            return ResultCode.DeviceDisabled;
        }

        public ResultCode GetState(out NetworkState state)
        {
            // Return ResultCode.InvalidArgument if _state is null, doesn't occur in our case.

            state = _state;

            return ResultCode.Success;
        }

        public ResultCode Finalize()
        {
            // TODO(Ac_K): Finalize nifm service then kill the thread named nn.ldn.NetworkInterfaceMonitor.

            _state = NetworkState.None;

            StateChangeEvent.WritableEvent.Signal();
            StateChangeEvent.WritableEvent.Clear();

            Logger.Stub?.PrintStub(LogClass.ServiceLdn);

            return ResultCode.Success;
        }
    }
}
