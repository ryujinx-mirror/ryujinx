using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Nvdec.Image;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Nvdec
{
    public class NvdecDevice : IDeviceState
    {
        private readonly ResourceManager _rm;
        private readonly DeviceState<NvdecRegisters> _state;

        public event Action<FrameDecodedEventArgs> FrameDecoded;

        public NvdecDevice(MemoryManager gmm)
        {
            _rm = new ResourceManager(gmm, new SurfaceCache(gmm));
            _state = new DeviceState<NvdecRegisters>(new Dictionary<string, RwCallback>
            {
                { nameof(NvdecRegisters.Execute), new RwCallback(Execute, null) }
            });
        }

        public int Read(int offset) => _state.Read(offset);
        public void Write(int offset, int data) => _state.Write(offset, data);

        private void Execute(int data)
        {
            Decode((CodecId)_state.State.SetCodecID);
        }

        private void Decode(CodecId codecId)
        {
            switch (codecId)
            {
                case CodecId.H264:
                    H264Decoder.Decode(this, _rm, ref _state.State);
                    break;
                case CodecId.Vp9:
                    Vp9Decoder.Decode(this, _rm, ref _state.State);
                    break;
                default:
                    Logger.Error?.Print(LogClass.Nvdec, $"Unsupported codec \"{codecId}\".");
                    break;
            }
        }

        internal void OnFrameDecoded(CodecId codecId, uint lumaOffset, uint chromaOffset)
        {
            FrameDecoded?.Invoke(new FrameDecodedEventArgs(codecId, lumaOffset, chromaOffset));
        }
    }
}
