using ARMeilleure.CodeGen.Unwinding;
using System;
using System.IO;

namespace ARMeilleure.Translation.PTC
{
    class PtcInfo : IDisposable
    {
        private readonly BinaryWriter _relocWriter;
        private readonly BinaryWriter _unwindInfoWriter;

        public byte[] Code { get; set; }

        public MemoryStream RelocStream      { get; }
        public MemoryStream UnwindInfoStream { get; }

        public int RelocEntriesCount { get; private set; }

        public PtcInfo()
        {
            RelocStream      = new MemoryStream();
            UnwindInfoStream = new MemoryStream();

            _relocWriter      = new BinaryWriter(RelocStream,      EncodingCache.UTF8NoBOM, true);
            _unwindInfoWriter = new BinaryWriter(UnwindInfoStream, EncodingCache.UTF8NoBOM, true);

            RelocEntriesCount = 0;
        }

        public void WriteRelocEntry(RelocEntry relocEntry)
        {
            _relocWriter.Write((int)relocEntry.Position);
            _relocWriter.Write((int)relocEntry.Index);

            RelocEntriesCount++;
        }

        public void WriteUnwindInfo(UnwindInfo unwindInfo)
        {
            _unwindInfoWriter.Write((int)unwindInfo.PushEntries.Length);

            foreach (UnwindPushEntry unwindPushEntry in unwindInfo.PushEntries)
            {
                _unwindInfoWriter.Write((int)unwindPushEntry.PseudoOp);
                _unwindInfoWriter.Write((int)unwindPushEntry.PrologOffset);
                _unwindInfoWriter.Write((int)unwindPushEntry.RegIndex);
                _unwindInfoWriter.Write((int)unwindPushEntry.StackOffsetOrAllocSize);
            }

            _unwindInfoWriter.Write((int)unwindInfo.PrologSize);
        }

        public void Dispose()
        {
            _relocWriter.Dispose();
            _unwindInfoWriter.Dispose();

            RelocStream.Dispose();
            UnwindInfoStream.Dispose();
        }
    }
}
