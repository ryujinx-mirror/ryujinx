using Ryujinx.HLE.HOS.Kernel.Threading;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    abstract class SystemClockContextUpdateCallback
    {
        private   List<KWritableEvent> _operationEventList;
        protected SystemClockContext   _context;
        private   bool                 _hasContext;

        public SystemClockContextUpdateCallback()
        {
            _operationEventList = new List<KWritableEvent>();
            _context            = new SystemClockContext();
            _hasContext         = false;
        }

        private bool NeedUpdate(SystemClockContext context)
        {
            if (_hasContext)
            {
                return _context.Offset != context.Offset || _context.SteadyTimePoint.ClockSourceId != context.SteadyTimePoint.ClockSourceId;
            }

            return true;
        }

        public void RegisterOperationEvent(KWritableEvent writableEvent)
        {
            Monitor.Enter(_operationEventList);
            _operationEventList.Add(writableEvent);
            Monitor.Exit(_operationEventList);
        }

        private void BroadcastOperationEvent()
        {
            Monitor.Enter(_operationEventList);

            foreach (KWritableEvent e in _operationEventList)
            {
                e.Signal();
            }

            Monitor.Exit(_operationEventList);
        }

        protected abstract ResultCode Update();

        public ResultCode Update(SystemClockContext context)
        {
            ResultCode result = ResultCode.Success;

            if (NeedUpdate(context))
            {
                _context    = context;
                _hasContext = true;

                result = Update();

                if (result == ResultCode.Success)
                {
                    BroadcastOperationEvent();
                }
            }

            return result;
        }
    }
}
