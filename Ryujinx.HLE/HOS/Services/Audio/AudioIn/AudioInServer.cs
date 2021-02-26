using Ryujinx.Audio.Common;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioIn
{
    class AudioInServer : IpcService, IDisposable
    {
        private IAudioIn _impl;

        public AudioInServer(IAudioIn impl)
        {
            _impl = impl;
        }

        [Command(0)]
        // GetAudioInState() -> u32 state
        public ResultCode GetAudioInState(ServiceCtx context)
        {
            context.ResponseData.Write((uint)_impl.GetState());

            return ResultCode.Success;
        }

        [Command(1)]
        // Start()
        public ResultCode Start(ServiceCtx context)
        {
            return _impl.Start();
        }

        [Command(2)]
        // Stop()
        public ResultCode StopAudioIn(ServiceCtx context)
        {
            return _impl.Stop();
        }

        [Command(3)]
        // AppendAudioInBuffer(u64 tag, buffer<nn::audio::AudioInBuffer, 5>)
        public ResultCode AppendAudioInBuffer(ServiceCtx context)
        {
            long position = context.Request.SendBuff[0].Position;

            ulong bufferTag = context.RequestData.ReadUInt64();

            AudioUserBuffer data = MemoryHelper.Read<AudioUserBuffer>(context.Memory, position);

            return _impl.AppendBuffer(bufferTag, ref data);
        }

        [Command(4)]
        // RegisterBufferEvent() -> handle<copy>
        public ResultCode RegisterBufferEvent(ServiceCtx context)
        {
            KEvent bufferEvent = _impl.RegisterBufferEvent();

            if (context.Process.HandleTable.GenerateHandle(bufferEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return ResultCode.Success;
        }

        [Command(5)]
        // GetReleasedAudioInBuffers() -> (u32 count, buffer<u64, 6> tags)
        public ResultCode GetReleasedAudioInBuffers(ServiceCtx context)
        {
            long position = context.Request.ReceiveBuff[0].Position;
            long size = context.Request.ReceiveBuff[0].Size;

            using (WritableRegion outputRegion = context.Memory.GetWritableRegion((ulong)position, (int)size))
            {
                ResultCode result = _impl.GetReleasedBuffers(MemoryMarshal.Cast<byte, ulong>(outputRegion.Memory.Span), out uint releasedCount);

                context.ResponseData.Write(releasedCount);

                return result;
            }
        }

        [Command(6)]
        // ContainsAudioInBuffer(u64 tag) -> b8
        public ResultCode ContainsAudioInBuffer(ServiceCtx context)
        {
            ulong bufferTag = context.RequestData.ReadUInt64();

            context.ResponseData.Write(_impl.ContainsBuffer(bufferTag));

            return ResultCode.Success;
        }

        [Command(7)] // 3.0.0+
        // AppendUacInBuffer(u64 tag, handle<copy, unknown>, buffer<nn::audio::AudioInBuffer, 5>)
        public ResultCode AppendUacInBuffer(ServiceCtx context)
        {
            long position = context.Request.SendBuff[0].Position;

            ulong bufferTag = context.RequestData.ReadUInt64();
            uint handle = (uint)context.Request.HandleDesc.ToCopy[0];

            AudioUserBuffer data = MemoryHelper.Read<AudioUserBuffer>(context.Memory, position);

            return _impl.AppendUacBuffer(bufferTag, ref data, handle);
        }

        [Command(8)] // 3.0.0+
        // AppendAudioInBufferAuto(u64 tag, buffer<nn::audio::AudioInBuffer, 0x21>)
        public ResultCode AppendAudioInBufferAuto(ServiceCtx context)
        {
            (long position, _) = context.Request.GetBufferType0x21();

            ulong bufferTag = context.RequestData.ReadUInt64();

            AudioUserBuffer data = MemoryHelper.Read<AudioUserBuffer>(context.Memory, position);

            return _impl.AppendBuffer(bufferTag, ref data);
        }

        [Command(9)] // 3.0.0+
        // GetReleasedAudioInBuffersAuto() -> (u32 count, buffer<u64, 0x22> tags)
        public ResultCode GetReleasedAudioInBuffersAuto(ServiceCtx context)
        {
            (long position, long size) = context.Request.GetBufferType0x22();

            using (WritableRegion outputRegion = context.Memory.GetWritableRegion((ulong)position, (int)size))
            {
                ResultCode result = _impl.GetReleasedBuffers(MemoryMarshal.Cast<byte, ulong>(outputRegion.Memory.Span), out uint releasedCount);

                context.ResponseData.Write(releasedCount);

                return result;
            }
        }

        [Command(10)] // 3.0.0+
        // AppendUacInBufferAuto(u64 tag, handle<copy, event>, buffer<nn::audio::AudioInBuffer, 0x21>)
        public ResultCode AppendUacInBufferAuto(ServiceCtx context)
        {
            (long position, _) = context.Request.GetBufferType0x21();

            ulong bufferTag = context.RequestData.ReadUInt64();
            uint handle = (uint)context.Request.HandleDesc.ToCopy[0];

            AudioUserBuffer data = MemoryHelper.Read<AudioUserBuffer>(context.Memory, position);

            return _impl.AppendUacBuffer(bufferTag, ref data, handle);
        }

        [Command(11)] // 4.0.0+
        // GetAudioInBufferCount() -> u32
        public ResultCode GetAudioInBufferCount(ServiceCtx context)
        {
            context.ResponseData.Write(_impl.GetBufferCount());

            return ResultCode.Success;
        }

        [Command(12)] // 4.0.0+
        // SetAudioInVolume(s32)
        public ResultCode SetAudioInVolume(ServiceCtx context)
        {
            float volume = context.RequestData.ReadSingle();

            _impl.SetVolume(volume);

            return ResultCode.Success;
        }

        [Command(13)] // 4.0.0+
        // GetAudioInVolume() -> s32
        public ResultCode GetAudioInVolume(ServiceCtx context)
        {
            context.ResponseData.Write(_impl.GetVolume());

            return ResultCode.Success;
        }

        [Command(14)] // 6.0.0+
        // FlushAudioInBuffers() -> b8
        public ResultCode FlushAudioInBuffers(ServiceCtx context)
        {
            context.ResponseData.Write(_impl.FlushBuffers());

            return ResultCode.Success;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _impl.Dispose();
            }
        }
    }
}
