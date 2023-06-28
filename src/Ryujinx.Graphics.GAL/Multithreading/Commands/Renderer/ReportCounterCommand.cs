using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;
using System;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct ReportCounterCommand : IGALCommand, IGALCommand<ReportCounterCommand>
    {
        public readonly CommandType CommandType => CommandType.ReportCounter;
        private TableRef<ThreadedCounterEvent> _event;
        private CounterType _type;
        private TableRef<EventHandler<ulong>> _resultHandler;
        private bool _hostReserved;

        public void Set(TableRef<ThreadedCounterEvent> evt, CounterType type, TableRef<EventHandler<ulong>> resultHandler, bool hostReserved)
        {
            _event = evt;
            _type = type;
            _resultHandler = resultHandler;
            _hostReserved = hostReserved;
        }

        public static void Run(ref ReportCounterCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ThreadedCounterEvent evt = command._event.Get(threaded);

            evt.Create(renderer, command._type, command._resultHandler.Get(threaded), command._hostReserved);
        }
    }
}
