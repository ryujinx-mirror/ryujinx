using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.OsTypes;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    partial class FinalOutputRecorder : IFinalOutputRecorder, IDisposable
    {
        private int _processHandle;
        private SystemEventType _event;

        public FinalOutputRecorder(int processHandle)
        {
            _processHandle = processHandle;
            Os.CreateSystemEvent(out _event, EventClearMode.ManualClear, interProcess: true);
        }

        [CmifCommand(0)]
        public Result GetFinalOutputRecorderState(out uint state)
        {
            state = 0;

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result Start()
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return Result.Success;
        }

        [CmifCommand(2)]
        public Result Stop()
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return Result.Success;
        }

        [CmifCommand(3)]
        public Result AppendFinalOutputRecorderBuffer([Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<byte> buffer, ulong bufferClientPtr)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio, new { bufferClientPtr });

            return Result.Success;
        }

        [CmifCommand(4)]
        public Result RegisterBufferEvent([CopyHandle] out int eventHandle)
        {
            eventHandle = Os.GetReadableHandleOfSystemEvent(ref _event);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return Result.Success;
        }

        [CmifCommand(5)]
        public Result GetReleasedFinalOutputRecorderBuffers([Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias)] Span<byte> buffer, out uint count, out ulong released)
        {
            count = 0;
            released = 0;

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return Result.Success;
        }

        [CmifCommand(6)]
        public Result ContainsFinalOutputRecorderBuffer(ulong bufferPointer, out bool contains)
        {
            contains = false;

            Logger.Stub?.PrintStub(LogClass.ServiceAudio, new { bufferPointer });

            return Result.Success;
        }

        [CmifCommand(7)]
        public Result GetFinalOutputRecorderBufferEndTime(ulong bufferPointer, out ulong released)
        {
            released = 0;

            Logger.Stub?.PrintStub(LogClass.ServiceAudio, new { bufferPointer });

            return Result.Success;
        }

        [CmifCommand(8)] // 3.0.0+
        public Result AppendFinalOutputRecorderBufferAuto([Buffer(HipcBufferFlags.In | HipcBufferFlags.AutoSelect)] ReadOnlySpan<byte> buffer, ulong bufferClientPtr)
        {
            return AppendFinalOutputRecorderBuffer(buffer, bufferClientPtr);
        }

        [CmifCommand(9)] // 3.0.0+
        public Result GetReleasedFinalOutputRecorderBuffersAuto([Buffer(HipcBufferFlags.Out | HipcBufferFlags.AutoSelect)] Span<byte> buffer, out uint count, out ulong released)
        {
            return GetReleasedFinalOutputRecorderBuffers(buffer, out count, out released);
        }

        [CmifCommand(10)] // 6.0.0+
        public Result FlushFinalOutputRecorderBuffers(out bool pending)
        {
            pending = false;

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return Result.Success;
        }

        [CmifCommand(11)] // 9.0.0+
        public Result AttachWorkBuffer(FinalOutputRecorderParameterInternal parameter)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio, new { parameter });

            return Result.Success;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Os.DestroySystemEvent(ref _event);

                if (_processHandle != 0)
                {
                    HorizonStatic.Syscall.CloseHandle(_processHandle);

                    _processHandle = 0;
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
