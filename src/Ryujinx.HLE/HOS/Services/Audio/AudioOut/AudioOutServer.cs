using Ryujinx.Audio.Common;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioOut
{
    class AudioOutServer : DisposableIpcService
    {
        private IAudioOut _impl;

        public AudioOutServer(IAudioOut impl)
        {
            _impl = impl;
        }

        [CommandCmif(0)]
        // GetAudioOutState() -> u32 state
        public ResultCode GetAudioOutState(ServiceCtx context)
        {
            context.ResponseData.Write((uint)_impl.GetState());

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // Start()
        public ResultCode Start(ServiceCtx context)
        {
            return _impl.Start();
        }

        [CommandCmif(2)]
        // Stop()
        public ResultCode Stop(ServiceCtx context)
        {
            return _impl.Stop();
        }

        [CommandCmif(3)]
        // AppendAudioOutBuffer(u64 bufferTag, buffer<nn::audio::AudioOutBuffer, 5> buffer)
        public ResultCode AppendAudioOutBuffer(ServiceCtx context)
        {
            ulong position = context.Request.SendBuff[0].Position;

            ulong bufferTag = context.RequestData.ReadUInt64();

            AudioUserBuffer data = MemoryHelper.Read<AudioUserBuffer>(context.Memory, position);

            return _impl.AppendBuffer(bufferTag, ref data);
        }

        [CommandCmif(4)]
        // RegisterBufferEvent() -> handle<copy>
        public ResultCode RegisterBufferEvent(ServiceCtx context)
        {
            KEvent bufferEvent = _impl.RegisterBufferEvent();

            if (context.Process.HandleTable.GenerateHandle(bufferEvent.ReadableEvent, out int handle) != Result.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // GetReleasedAudioOutBuffers() -> (u32 count, buffer<u64, 6> tags)
        public ResultCode GetReleasedAudioOutBuffers(ServiceCtx context)
        {
            ulong position = context.Request.ReceiveBuff[0].Position;
            ulong size = context.Request.ReceiveBuff[0].Size;

            using (WritableRegion outputRegion = context.Memory.GetWritableRegion(position, (int)size))
            {
                ResultCode result = _impl.GetReleasedBuffers(MemoryMarshal.Cast<byte, ulong>(outputRegion.Memory.Span), out uint releasedCount);

                context.ResponseData.Write(releasedCount);

                return result;
            }
        }

        [CommandCmif(6)]
        // ContainsAudioOutBuffer(u64 tag) -> b8
        public ResultCode ContainsAudioOutBuffer(ServiceCtx context)
        {
            ulong bufferTag = context.RequestData.ReadUInt64();

            context.ResponseData.Write(_impl.ContainsBuffer(bufferTag));

            return ResultCode.Success;
        }

        [CommandCmif(7)] // 3.0.0+
        // AppendAudioOutBufferAuto(u64 tag, buffer<nn::audio::AudioOutBuffer, 0x21>)
        public ResultCode AppendAudioOutBufferAuto(ServiceCtx context)
        {
            (ulong position, _) = context.Request.GetBufferType0x21();

            ulong bufferTag = context.RequestData.ReadUInt64();

            AudioUserBuffer data = MemoryHelper.Read<AudioUserBuffer>(context.Memory, position);

            return _impl.AppendBuffer(bufferTag, ref data);
        }

        [CommandCmif(8)] // 3.0.0+
        // GetReleasedAudioOutBuffersAuto() -> (u32 count, buffer<u64, 0x22> tags)
        public ResultCode GetReleasedAudioOutBuffersAuto(ServiceCtx context)
        {
            (ulong position, ulong size) = context.Request.GetBufferType0x22();

            using (WritableRegion outputRegion = context.Memory.GetWritableRegion(position, (int)size))
            {
                ResultCode result = _impl.GetReleasedBuffers(MemoryMarshal.Cast<byte, ulong>(outputRegion.Memory.Span), out uint releasedCount);

                context.ResponseData.Write(releasedCount);

                return result;
            }
        }

        [CommandCmif(9)] // 4.0.0+
        // GetAudioOutBufferCount() -> u32
        public ResultCode GetAudioOutBufferCount(ServiceCtx context)
        {
            context.ResponseData.Write(_impl.GetBufferCount());

            return ResultCode.Success;
        }

        [CommandCmif(10)] // 4.0.0+
        // GetAudioOutPlayedSampleCount() -> u64
        public ResultCode GetAudioOutPlayedSampleCount(ServiceCtx context)
        {
            context.ResponseData.Write(_impl.GetPlayedSampleCount());

            return ResultCode.Success;
        }

        [CommandCmif(11)] // 4.0.0+
        // FlushAudioOutBuffers() -> b8
        public ResultCode FlushAudioOutBuffers(ServiceCtx context)
        {
            context.ResponseData.Write(_impl.FlushBuffers());

            return ResultCode.Success;
        }

        [CommandCmif(12)] // 6.0.0+
        // SetAudioOutVolume(s32)
        public ResultCode SetAudioOutVolume(ServiceCtx context)
        {
            float volume = context.RequestData.ReadSingle();

            _impl.SetVolume(volume);

            return ResultCode.Success;
        }

        [CommandCmif(13)] // 6.0.0+
        // GetAudioOutVolume() -> s32
        public ResultCode GetAudioOutVolume(ServiceCtx context)
        {
            context.ResponseData.Write(_impl.GetVolume());

            return ResultCode.Success;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _impl.Dispose();
            }
        }
    }
}
