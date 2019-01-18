using Ryujinx.HLE.HOS.Kernel.Common;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KServerPort : KSynchronizationObject
    {
        private LinkedList<KServerSession>      _incomingConnections;
        private LinkedList<KLightServerSession> _lightIncomingConnections;

        private KPort _parent;

        public bool IsLight => _parent.IsLight;

        public KServerPort(Horizon system, KPort parent) : base(system)
        {
            _parent = parent;

            _incomingConnections      = new LinkedList<KServerSession>();
            _lightIncomingConnections = new LinkedList<KLightServerSession>();
        }

        public void EnqueueIncomingSession(KServerSession session)
        {
            AcceptIncomingConnection(_incomingConnections, session);
        }

        public void EnqueueIncomingLightSession(KLightServerSession session)
        {
            AcceptIncomingConnection(_lightIncomingConnections, session);
        }

        private void AcceptIncomingConnection<T>(LinkedList<T> list, T session)
        {
            System.CriticalSection.Enter();

            list.AddLast(session);

            if (list.Count == 1)
            {
                Signal();
            }

            System.CriticalSection.Leave();
        }

        public KServerSession AcceptIncomingConnection()
        {
            return AcceptIncomingConnection(_incomingConnections);
        }

        public KLightServerSession AcceptIncomingLightConnection()
        {
            return AcceptIncomingConnection(_lightIncomingConnections);
        }

        private T AcceptIncomingConnection<T>(LinkedList<T> list)
        {
            T session = default(T);

            System.CriticalSection.Enter();

            if (list.Count != 0)
            {
                session = list.First.Value;

                list.RemoveFirst();
            }

            System.CriticalSection.Leave();

            return session;
        }

        public override bool IsSignaled()
        {
            if (_parent.IsLight)
            {
                return _lightIncomingConnections.Count != 0;
            }
            else
            {
                return _incomingConnections.Count != 0;
            }
        }
    }
}