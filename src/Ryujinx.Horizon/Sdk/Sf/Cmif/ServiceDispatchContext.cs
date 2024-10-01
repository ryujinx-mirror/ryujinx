using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    ref struct ServiceDispatchContext
    {
        public IServiceObject ServiceObject;
        public ServerSessionManager Manager;
        public ServerSession Session;
        public ServerMessageProcessor Processor;
        public HandlesToClose HandlesToClose;
        public PointerAndSize PointerBuffer;
        public ReadOnlySpan<byte> InMessageBuffer;
        public Span<byte> OutMessageBuffer;
        public HipcMessage Request;
    }
}
