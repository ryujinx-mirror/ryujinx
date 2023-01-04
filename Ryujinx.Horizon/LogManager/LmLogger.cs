using Ryujinx.Common.Logging;
using Ryujinx.Common.Memory;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Lm;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.Horizon.LogManager
{
    partial class LmLogger : IServiceObject
    {
        private readonly LmLog _log;
        private readonly ulong _clientProcessId;

        public LmLogger(LmLog log, ulong clientProcessId)
        {
            _log = log;
            _clientProcessId = clientProcessId;
        }

        [CmifCommand(0)]
        public Result Log([Buffer(HipcBufferFlags.In | HipcBufferFlags.AutoSelect)] Span<byte> message)
        {
            if (!SetProcessId(message, _clientProcessId))
            {
                return Result.Success;
            }

            Logger.Guest?.Print(LogClass.ServiceLm, LogImpl(message));

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result SetDestination(LogDestination destination)
        {
            _log.LogDestination = destination;

            return Result.Success;
        }

        private static bool SetProcessId(Span<byte> message, ulong processId)
        {
            ref LogPacketHeader header = ref MemoryMarshal.Cast<byte, LogPacketHeader>(message)[0];

            uint expectedMessageSize = (uint)Unsafe.SizeOf<LogPacketHeader>() + header.PayloadSize;

            if (expectedMessageSize != (uint)message.Length)
            {
                Logger.Warning?.Print(LogClass.ServiceLm, $"Invalid message size (expected 0x{expectedMessageSize:X} but got 0x{message.Length:X}).");

                return false;
            }

            header.ProcessId = processId;

            return true;
        }

        private static string LogImpl(ReadOnlySpan<byte> message)
        {
            SpanReader reader = new SpanReader(message);

            LogPacketHeader header = reader.Read<LogPacketHeader>();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Guest Log:\n  Log level: {header.Severity}");

            while (reader.Length > 0)
            {
                int type = ReadUleb128(ref reader);
                int size = ReadUleb128(ref reader);

                LogDataChunkKey field = (LogDataChunkKey)type;

                string fieldStr = string.Empty;

                if (field == LogDataChunkKey.Start)
                {
                    reader.Skip(size);

                    continue;
                }
                else if (field == LogDataChunkKey.Stop)
                {
                    break;
                }
                else if (field == LogDataChunkKey.Line)
                {
                    fieldStr = $"{field}: {reader.Read<int>()}";
                }
                else if (field == LogDataChunkKey.DropCount)
                {
                    fieldStr = $"{field}: {reader.Read<long>()}";
                }
                else if (field == LogDataChunkKey.Time)
                {
                    fieldStr = $"{field}: {reader.Read<long>()}s";
                }
                else if (field < LogDataChunkKey.Count)
                {
                    fieldStr = $"{field}: '{Encoding.UTF8.GetString(reader.GetSpan(size)).TrimEnd()}'";
                }
                else
                {
                    fieldStr = $"Field{field}: '{Encoding.UTF8.GetString(reader.GetSpan(size)).TrimEnd()}'";
                }

                sb.AppendLine($"    {fieldStr}");
            }

            return sb.ToString();
        }

        private static int ReadUleb128(ref SpanReader reader)
        {
            int result = 0;
            int count = 0;

            byte encoded;

            do
            {
                encoded = reader.Read<byte>();

                result += (encoded & 0x7F) << (7 * count);

                count++;
            } while ((encoded & 0x80) != 0);

            return result;
        }
    }
}
