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

namespace Ryujinx.Horizon.LogManager.Ipc
{
    partial class LmLogger : ILmLogger
    {
        private readonly LogService _log;
        private readonly ulong      _pid;

        public LmLogger(LogService log, ulong pid)
        {
            _log = log;
            _pid = pid;
        }

        [CmifCommand(0)]
        public Result Log([Buffer(HipcBufferFlags.In | HipcBufferFlags.AutoSelect)] Span<byte> message)
        {
            if (!SetProcessId(message, _pid))
            {
                return Result.Success;
            }

            Logger.Guest?.Print(LogClass.ServiceLm, LogImpl(message));

            return Result.Success;
        }

        [CmifCommand(1)] // 3.0.0+
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
            SpanReader      reader  = new(message);
            LogPacketHeader header  = reader.Read<LogPacketHeader>();
            StringBuilder   builder = new();

            builder.AppendLine($"Guest Log:\n  Log level: {header.Severity}");

            while (reader.Length > 0)
            {
                int type = ReadUleb128(ref reader);
                int size = ReadUleb128(ref reader);

                LogDataChunkKey field = (LogDataChunkKey)type;

                string fieldStr;

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

                builder.AppendLine($"    {fieldStr}");
            }

            return builder.ToString();
        }

        private static int ReadUleb128(ref SpanReader reader)
        {
            int result = 0;
            int count  = 0;

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