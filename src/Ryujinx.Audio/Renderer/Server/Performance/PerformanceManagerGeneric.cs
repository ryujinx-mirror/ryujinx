using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Performance
{
    /// <summary>
    /// A Generic implementation of <see cref="PerformanceManager"/>.
    /// </summary>
    /// <typeparam name="THeader">The header implementation of the performance frame.</typeparam>
    /// <typeparam name="TEntry">The entry implementation of the performance frame.</typeparam>
    /// <typeparam name="TEntryDetail">A detailed implementation of the performance frame.</typeparam>
    public class PerformanceManagerGeneric<THeader, TEntry, TEntryDetail> : PerformanceManager where THeader : unmanaged, IPerformanceHeader where TEntry : unmanaged, IPerformanceEntry where TEntryDetail : unmanaged, IPerformanceDetailEntry
    {
        /// <summary>
        /// The magic used for the <see cref="THeader"/>.
        /// </summary>
        private const uint MagicPerformanceBuffer = 0x46524550;

        /// <summary>
        /// The fixed amount of <see cref="TEntryDetail"/> that can be stored in a frame.
        /// </summary>
        private const int MaxFrameDetailCount = 100;

        private readonly Memory<byte> _buffer;
        private readonly Memory<byte> _historyBuffer;

        private Memory<byte> CurrentBuffer => _buffer[.._frameSize];
        private Memory<byte> CurrentBufferData => CurrentBuffer[Unsafe.SizeOf<THeader>()..];

        private ref THeader CurrentHeader => ref MemoryMarshal.Cast<byte, THeader>(CurrentBuffer.Span)[0];

        private Span<TEntry> Entries => MemoryMarshal.Cast<byte, TEntry>(CurrentBufferData.Span[..GetEntriesSize()]);
        private Span<TEntryDetail> EntriesDetail => MemoryMarshal.Cast<byte, TEntryDetail>(CurrentBufferData.Span.Slice(GetEntriesSize(), GetEntriesDetailSize()));

        private readonly int _frameSize;
        private readonly int _availableFrameCount;
        private readonly int _entryCountPerFrame;
        private int _detailTarget;
        private int _entryIndex;
        private int _entryDetailIndex;
        private int _indexHistoryWrite;
        private int _indexHistoryRead;
        private uint _historyFrameIndex;

        public PerformanceManagerGeneric(Memory<byte> buffer, ref AudioRendererConfiguration parameter)
        {
            _buffer = buffer;
            _frameSize = GetRequiredBufferSizeForPerformanceMetricsPerFrame(ref parameter);

            _entryCountPerFrame = (int)GetEntryCount(ref parameter);
            _availableFrameCount = buffer.Length / _frameSize - 1;

            _historyFrameIndex = 0;

            _historyBuffer = _buffer[_frameSize..];

            SetupNewHeader();
        }

        private Span<byte> GetBufferFromIndex(Span<byte> data, int index)
        {
            return data.Slice(index * _frameSize, _frameSize);
        }

        private ref THeader GetHeaderFromBuffer(Span<byte> data, int index)
        {
            return ref MemoryMarshal.Cast<byte, THeader>(GetBufferFromIndex(data, index))[0];
        }

        private Span<TEntry> GetEntriesFromBuffer(Span<byte> data, int index)
        {
            return MemoryMarshal.Cast<byte, TEntry>(GetBufferFromIndex(data, index).Slice(Unsafe.SizeOf<THeader>(), GetEntriesSize()));
        }

        private Span<TEntryDetail> GetEntriesDetailFromBuffer(Span<byte> data, int index)
        {
            return MemoryMarshal.Cast<byte, TEntryDetail>(GetBufferFromIndex(data, index).Slice(Unsafe.SizeOf<THeader>() + GetEntriesSize(), GetEntriesDetailSize()));
        }

        private void SetupNewHeader()
        {
            _entryIndex = 0;
            _entryDetailIndex = 0;

            CurrentHeader.SetEntryCount(0);
            CurrentHeader.SetEntryDetailCount(0);
        }

        public static uint GetEntryCount(ref AudioRendererConfiguration parameter)
        {
            return parameter.VoiceCount + parameter.EffectCount + parameter.SubMixBufferCount + parameter.SinkCount + 1;
        }

        public int GetEntriesSize()
        {
            return Unsafe.SizeOf<TEntry>() * _entryCountPerFrame;
        }

        public static int GetEntriesDetailSize()
        {
            return Unsafe.SizeOf<TEntryDetail>() * MaxFrameDetailCount;
        }

        public static int GetRequiredBufferSizeForPerformanceMetricsPerFrame(ref AudioRendererConfiguration parameter)
        {
            return Unsafe.SizeOf<TEntry>() * (int)GetEntryCount(ref parameter) + GetEntriesDetailSize() + Unsafe.SizeOf<THeader>();
        }

        public override uint CopyHistories(Span<byte> performanceOutput)
        {
            if (performanceOutput.IsEmpty)
            {
                return 0;
            }

            int nextOffset = 0;

            while (_indexHistoryRead != _indexHistoryWrite)
            {
                if (nextOffset >= performanceOutput.Length)
                {
                    break;
                }

                ref THeader inputHeader = ref GetHeaderFromBuffer(_historyBuffer.Span, _indexHistoryRead);
                Span<TEntry> inputEntries = GetEntriesFromBuffer(_historyBuffer.Span, _indexHistoryRead);
                Span<TEntryDetail> inputEntriesDetail = GetEntriesDetailFromBuffer(_historyBuffer.Span, _indexHistoryRead);

                Span<byte> targetSpan = performanceOutput[nextOffset..];

                // NOTE: We check for the space for two headers for the final blank header.
                int requiredSpace = Unsafe.SizeOf<THeader>() + Unsafe.SizeOf<TEntry>() * inputHeader.GetEntryCount()
                                                             + Unsafe.SizeOf<TEntryDetail>() * inputHeader.GetEntryDetailCount()
                                                             + Unsafe.SizeOf<THeader>();

                if (targetSpan.Length < requiredSpace)
                {
                    break;
                }

                ref THeader outputHeader = ref MemoryMarshal.Cast<byte, THeader>(targetSpan)[0];

                nextOffset += Unsafe.SizeOf<THeader>();

                Span<TEntry> outputEntries = MemoryMarshal.Cast<byte, TEntry>(performanceOutput[nextOffset..]);

                int totalProcessingTime = 0;

                int effectiveEntryCount = 0;

                for (int entryIndex = 0; entryIndex < inputHeader.GetEntryCount(); entryIndex++)
                {
                    ref TEntry input = ref inputEntries[entryIndex];

                    if (input.GetProcessingTime() != 0 || input.GetStartTime() != 0)
                    {
                        ref TEntry output = ref outputEntries[effectiveEntryCount++];

                        output = input;

                        nextOffset += Unsafe.SizeOf<TEntry>();

                        totalProcessingTime += input.GetProcessingTime();
                    }
                }

                Span<TEntryDetail> outputEntriesDetail = MemoryMarshal.Cast<byte, TEntryDetail>(performanceOutput[nextOffset..]);

                int effectiveEntryDetailCount = 0;

                for (int entryDetailIndex = 0; entryDetailIndex < inputHeader.GetEntryDetailCount(); entryDetailIndex++)
                {
                    ref TEntryDetail input = ref inputEntriesDetail[entryDetailIndex];

                    if (input.GetProcessingTime() != 0 || input.GetStartTime() != 0)
                    {
                        ref TEntryDetail output = ref outputEntriesDetail[effectiveEntryDetailCount++];

                        output = input;

                        nextOffset += Unsafe.SizeOf<TEntryDetail>();
                    }
                }

                outputHeader = inputHeader;
                outputHeader.SetMagic(MagicPerformanceBuffer);
                outputHeader.SetTotalProcessingTime(totalProcessingTime);
                outputHeader.SetNextOffset(nextOffset);
                outputHeader.SetEntryCount(effectiveEntryCount);
                outputHeader.SetEntryDetailCount(effectiveEntryDetailCount);

                _indexHistoryRead = (_indexHistoryRead + 1) % _availableFrameCount;
            }

            if (nextOffset < performanceOutput.Length && (performanceOutput.Length - nextOffset) >= Unsafe.SizeOf<THeader>())
            {
                ref THeader outputHeader = ref MemoryMarshal.Cast<byte, THeader>(performanceOutput[nextOffset..])[0];

                outputHeader = default;
            }

            return (uint)nextOffset;
        }

        public override bool GetNextEntry(out PerformanceEntryAddresses performanceEntry, PerformanceEntryType entryType, int nodeId)
        {
            performanceEntry = new PerformanceEntryAddresses
            {
                BaseMemory = SpanMemoryManager<int>.Cast(CurrentBuffer),
                EntryCountOffset = (uint)CurrentHeader.GetEntryCountOffset(),
            };

            uint baseEntryOffset = (uint)(Unsafe.SizeOf<THeader>() + Unsafe.SizeOf<TEntry>() * _entryIndex);

            ref TEntry entry = ref Entries[_entryIndex];

            performanceEntry.StartTimeOffset = baseEntryOffset + (uint)entry.GetStartTimeOffset();
            performanceEntry.ProcessingTimeOffset = baseEntryOffset + (uint)entry.GetProcessingTimeOffset();

            entry = default;
            entry.SetEntryType(entryType);
            entry.SetNodeId(nodeId);

            _entryIndex++;

            return true;
        }

        public override bool GetNextEntry(out PerformanceEntryAddresses performanceEntry, PerformanceDetailType detailType, PerformanceEntryType entryType, int nodeId)
        {
            performanceEntry = null;

            if (_entryDetailIndex >= MaxFrameDetailCount)
            {
                return false;
            }

            performanceEntry = new PerformanceEntryAddresses
            {
                BaseMemory = SpanMemoryManager<int>.Cast(CurrentBuffer),
                EntryCountOffset = (uint)CurrentHeader.GetEntryCountOffset(),
            };

            uint baseEntryOffset = (uint)(Unsafe.SizeOf<THeader>() + GetEntriesSize() + Unsafe.SizeOf<TEntryDetail>() * _entryDetailIndex);

            ref TEntryDetail entryDetail = ref EntriesDetail[_entryDetailIndex];

            performanceEntry.StartTimeOffset = baseEntryOffset + (uint)entryDetail.GetStartTimeOffset();
            performanceEntry.ProcessingTimeOffset = baseEntryOffset + (uint)entryDetail.GetProcessingTimeOffset();

            entryDetail = default;
            entryDetail.SetDetailType(detailType);
            entryDetail.SetEntryType(entryType);
            entryDetail.SetNodeId(nodeId);

            _entryDetailIndex++;

            return true;
        }

        public override bool IsTargetNodeId(int target)
        {
            return _detailTarget == target;
        }

        public override void SetTargetNodeId(int target)
        {
            _detailTarget = target;
        }

        public override void TapFrame(bool dspRunningBehind, uint voiceDropCount, ulong startRenderingTicks)
        {
            if (_availableFrameCount > 0)
            {
                int targetIndexForHistory = _indexHistoryWrite;

                _indexHistoryWrite = (_indexHistoryWrite + 1) % _availableFrameCount;

                ref THeader targetHeader = ref GetHeaderFromBuffer(_historyBuffer.Span, targetIndexForHistory);

                CurrentBuffer.Span.CopyTo(GetBufferFromIndex(_historyBuffer.Span, targetIndexForHistory));

                uint targetHistoryFrameIndex = _historyFrameIndex;

                if (_historyFrameIndex == uint.MaxValue)
                {
                    _historyFrameIndex = 0;
                }
                else
                {
                    _historyFrameIndex++;
                }

                targetHeader.SetDspRunningBehind(dspRunningBehind);
                targetHeader.SetVoiceDropCount(voiceDropCount);
                targetHeader.SetStartRenderingTicks(startRenderingTicks);
                targetHeader.SetIndex(targetHistoryFrameIndex);

                // Finally setup the new header
                SetupNewHeader();
            }
        }
    }
}
