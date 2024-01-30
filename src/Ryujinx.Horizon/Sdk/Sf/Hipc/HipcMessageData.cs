using System;

namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    ref struct HipcMessageData
    {
        public Span<HipcStaticDescriptor> SendStatics;
        public Span<HipcBufferDescriptor> SendBuffers;
        public Span<HipcBufferDescriptor> ReceiveBuffers;
        public Span<HipcBufferDescriptor> ExchangeBuffers;
        public Span<uint> DataWords;
        public Span<uint> DataWordsPadded;
        public Span<HipcReceiveListEntry> ReceiveList;
        public Span<int> CopyHandles;
        public Span<int> MoveHandles;
    }
}
