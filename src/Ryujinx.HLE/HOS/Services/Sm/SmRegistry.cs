using Ryujinx.HLE.HOS.Kernel.Ipc;
using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Sm
{
    class SmRegistry
    {
        private readonly ConcurrentDictionary<string, KPort> _registeredServices;
        private readonly AutoResetEvent _serviceRegistrationEvent;

        public SmRegistry()
        {
            _registeredServices = new ConcurrentDictionary<string, KPort>();
            _serviceRegistrationEvent = new AutoResetEvent(false);
        }

        public bool TryGetService(string name, out KPort port)
        {
            return _registeredServices.TryGetValue(name, out port);
        }

        public bool TryRegister(string name, KPort port)
        {
            if (_registeredServices.TryAdd(name, port))
            {
                _serviceRegistrationEvent.Set();
                return true;
            }

            return false;
        }

        public bool Unregister(string name)
        {
            return _registeredServices.TryRemove(name, out _);
        }

        public bool IsServiceRegistered(string name)
        {
            return _registeredServices.TryGetValue(name, out _);
        }

        public void WaitForServiceRegistration()
        {
            _serviceRegistrationEvent.WaitOne();
        }
    }
}
