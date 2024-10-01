using Ryujinx.Horizon.Sdk.Diag;

namespace Ryujinx.Horizon.Sdk.Lm
{
    struct LogPacketHeader
    {
        public ulong ProcessId;
        public ulong ThreadId;
        public LogPacketFlags Flags;
        public byte Padding;
        public LogSeverity Severity;
        public byte Verbosity;
        public uint PayloadSize;
    }
}
