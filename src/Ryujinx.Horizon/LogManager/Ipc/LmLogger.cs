using Ryujinx.Common.Logging;
using Ryujinx.Common.Memory;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.LogManager.Types;
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
        private const int MessageLengthLimit = 5000;

        private readonly LogService _log;
        private readonly ulong      _pid;

        private LogPacket _logPacket;

        public LmLogger(LogService log, ulong pid)
        {
            _log = log;
            _pid = pid;

            _logPacket = new LogPacket();
        }

        [CmifCommand(0)]
        public Result Log([Buffer(HipcBufferFlags.In | HipcBufferFlags.AutoSelect)] Span<byte> message)
        {
            if (!SetProcessId(message, _pid))
            {
                return Result.Success;
            }

            if (LogImpl(message))
            {
                Logger.Guest?.Print(LogClass.ServiceLm, _logPacket.ToString());

                _logPacket = new LogPacket();
            }

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

        private bool LogImpl(ReadOnlySpan<byte> message)
        {
            SpanReader      reader = new(message);
            LogPacketHeader header = reader.Read<LogPacketHeader>();

            bool isHeadPacket = (header.Flags & LogPacketFlags.IsHead) != 0;
            bool isTailPacket = (header.Flags & LogPacketFlags.IsTail) != 0;

            _logPacket.Severity = header.Severity;

            while (reader.Length > 0)
            {
                int type = ReadUleb128(ref reader);
                int size = ReadUleb128(ref reader);

                LogDataChunkKey key = (LogDataChunkKey)type;

                if (key == LogDataChunkKey.Start)
                {
                    reader.Skip(size);

                    continue;
                }
                else if (key == LogDataChunkKey.Stop)
                {
                    break;
                }
                else if (key == LogDataChunkKey.Line)
                {
                    _logPacket.Line = reader.Read<int>();
                }
                else if (key == LogDataChunkKey.DropCount)
                {
                    _logPacket.DropCount = reader.Read<long>();
                }
                else if (key == LogDataChunkKey.Time)
                {
                    _logPacket.Time = reader.Read<long>();
                }
                else if (key == LogDataChunkKey.Message)
                {
                    string text = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();

                    if (isHeadPacket && isTailPacket)
                    {
                        _logPacket.Message = text;
                    }
                    else
                    {
                        _logPacket.Message += text;

                        if (_logPacket.Message.Length >= MessageLengthLimit)
                        {
                            isTailPacket = true;
                        }
                    }
                }
                else if (key == LogDataChunkKey.Filename)
                {
                    _logPacket.Filename = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();
                }
                else if (key == LogDataChunkKey.Function)
                {
                    _logPacket.Function = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();
                }
                else if (key == LogDataChunkKey.Module)
                {
                    _logPacket.Module = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();
                }
                else if (key == LogDataChunkKey.Thread)
                {
                    _logPacket.Thread = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();
                }
                else if (key == LogDataChunkKey.ProgramName)
                {
                    _logPacket.ProgramName = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();
                }
            }

            return isTailPacket;
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