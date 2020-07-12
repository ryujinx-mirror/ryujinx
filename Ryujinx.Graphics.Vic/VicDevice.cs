using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Vic.Image;
using Ryujinx.Graphics.Vic.Types;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Vic
{
    public class VicDevice : IDeviceState
    {
        private readonly MemoryManager _gmm;
        private readonly ResourceManager _rm;
        private readonly DeviceState<VicRegisters> _state;

        private PlaneOffsets _overrideOffsets;
        private bool _hasOverride;

        public VicDevice(MemoryManager gmm)
        {
            _gmm = gmm;
            _rm = new ResourceManager(gmm, new BufferPool<Pixel>(), new BufferPool<byte>());
            _state = new DeviceState<VicRegisters>(new Dictionary<string, RwCallback>
            {
                { nameof(VicRegisters.Execute), new RwCallback(Execute, null) }
            });
        }

        /// <summary>
        /// Overrides all input surfaces with a custom surface.
        /// </summary>
        /// <param name="lumaOffset">Offset of the luma plane or packed data for this surface</param>
        /// <param name="chromaUOffset">Offset of the U chroma plane (for planar formats) or both chroma planes (for semiplanar formats)</param>
        /// <param name="chromaVOffset">Offset of the V chroma plane for planar formats</param>
        public void SetSurfaceOverride(uint lumaOffset, uint chromaUOffset, uint chromaVOffset)
        {
            _overrideOffsets.LumaOffset = lumaOffset;
            _overrideOffsets.ChromaUOffset = chromaUOffset;
            _overrideOffsets.ChromaVOffset = chromaVOffset;
            _hasOverride = true;
        }

        /// <summary>
        /// Disables overriding input surfaces.
        /// </summary>
        /// <remarks>
        /// Surface overrides are disabled by default.
        /// Call this if you previously called <see cref="SetSurfaceOverride(uint, uint, uint)"/> and which to disable it.
        /// </remarks>
        public void DisableSurfaceOverride()
        {
            _hasOverride = false;
        }

        public int Read(int offset) => _state.Read(offset);
        public void Write(int offset, int data) => _state.Write(offset, data);

        private void Execute(int data)
        {
            ConfigStruct config = ReadIndirect<ConfigStruct>(_state.State.SetConfigStructOffset);

            using Surface output = new Surface(
                _rm.SurfacePool,
                config.OutputSurfaceConfig.OutSurfaceWidth + 1,
                config.OutputSurfaceConfig.OutSurfaceHeight + 1);

            for (int i = 0; i < config.SlotStruct.Length; i++)
            {
                ref SlotStruct slot = ref config.SlotStruct[i];

                if (!slot.SlotConfig.SlotEnable)
                {
                    continue;
                }

                var offsets = _state.State.SetSurfacexSlotx[i][0];

                if (_hasOverride)
                {
                    offsets = _overrideOffsets;
                }

                using Surface src = SurfaceReader.Read(_rm, ref slot.SlotSurfaceConfig, ref offsets);

                Blender.BlendOne(output, src, ref slot);
            }

            SurfaceWriter.Write(_rm, output, ref config.OutputSurfaceConfig, ref _state.State.SetOutputSurface);
        }

        private T ReadIndirect<T>(uint offset) where T : unmanaged
        {
            return _gmm.Read<T>((ulong)offset << 8);
        }
    }
}
