using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    class ServiceObjectHolder
    {
        public IServiceObject ServiceObject { get; }

        private readonly ServiceDispatchMeta _dispatchMeta;

        public ServiceObjectHolder(ServiceObjectHolder objectHolder)
        {
            ServiceObject = objectHolder.ServiceObject;
            _dispatchMeta = objectHolder._dispatchMeta;
        }

        public ServiceObjectHolder(IServiceObject serviceImpl)
        {
            ServiceObject = serviceImpl;
            _dispatchMeta = new ServiceDispatchMeta(ServiceDispatchTable.Create(serviceImpl));
        }

        public ServiceObjectHolder Clone()
        {
            return new ServiceObjectHolder(this);
        }

        public Result ProcessMessage(ref ServiceDispatchContext context, ReadOnlySpan<byte> inRawData)
        {
            return _dispatchMeta.DispatchTable.ProcessMessage(ref context, inRawData);
        }
    }
}
