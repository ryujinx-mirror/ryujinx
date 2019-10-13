using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.State
{
    class GpuState
    {
        private const int RegistersCount = 0xe00;

        public delegate void MethodCallback(int argument);

        private int[] _backingMemory;

        private struct Register
        {
            public MethodCallback Callback;

            public StateWriteFlags WriteFlag;
        }

        private Register[] _registers;

        public StateWriteFlags StateWriteFlags { get; set; }

        public GpuState()
        {
            _backingMemory = new int[RegistersCount];

            _registers = new Register[RegistersCount];

            StateWriteFlags = StateWriteFlags.Any;

            InitializeDefaultState();
            InitializeStateWatchers();
        }

        public bool ExitEarly;

        public void CallMethod(MethodParams meth)
        {
            if (ExitEarly)
            {
                return;
            }

            Register register = _registers[meth.Method];

            if (_backingMemory[meth.Method] != meth.Argument)
            {
                StateWriteFlags |= register.WriteFlag;
            }

            _backingMemory[meth.Method] = meth.Argument;

            MethodCallback callback = register.Callback;

            if (callback != null)
            {
                callback(meth.Argument);
            }
        }

        public int Read(int offset)
        {
            return _backingMemory[offset];
        }

        public void RegisterCopyBufferCallback(MethodCallback callback)
        {
            RegisterCallback(0xc0, callback);
        }

        public void RegisterCopyTextureCallback(MethodCallback callback)
        {
            RegisterCallback(0x237, callback);
        }

        public void RegisterDrawEndCallback(MethodCallback callback)
        {
            RegisterCallback(0x585, callback);
        }

        public void RegisterDrawBeginCallback(MethodCallback callback)
        {
            RegisterCallback(0x586, callback);
        }

        public void RegisterSetIndexCountCallback(MethodCallback callback)
        {
            RegisterCallback(0x5f8, callback);
        }

        public void RegisterClearCallback(MethodCallback callback)
        {
            RegisterCallback(0x674, callback);
        }

        public void RegisterReportCallback(MethodCallback callback)
        {
            RegisterCallback(0x6c3, callback);
        }

        public void RegisterUniformBufferUpdateCallback(MethodCallback callback)
        {
            for (int index = 0; index < 16; index++)
            {
                RegisterCallback(0x8e4 + index, callback);
            }
        }

        public void RegisterUniformBufferBind0Callback(MethodCallback callback)
        {
            RegisterCallback(0x904, callback);
        }

        public void RegisterUniformBufferBind1Callback(MethodCallback callback)
        {
            RegisterCallback(0x90c, callback);
        }

        public void RegisterUniformBufferBind2Callback(MethodCallback callback)
        {
            RegisterCallback(0x914, callback);
        }

        public void RegisterUniformBufferBind3Callback(MethodCallback callback)
        {
            RegisterCallback(0x91c, callback);
        }

        public void RegisterUniformBufferBind4Callback(MethodCallback callback)
        {
            RegisterCallback(0x924, callback);
        }

        public CopyTexture GetCopyDstTexture()
        {
            return Get<CopyTexture>(MethodOffset.CopyDstTexture);
        }

        public CopyTexture GetCopySrcTexture()
        {
            return Get<CopyTexture>(MethodOffset.CopySrcTexture);
        }

        public RtColorState GetRtColorState(int index)
        {
            return Get<RtColorState>(MethodOffset.RtColorState + 16 * index);
        }

        public CopyTextureControl GetCopyTextureControl()
        {
            return Get<CopyTextureControl>(MethodOffset.CopyTextureControl);
        }

        public CopyRegion GetCopyRegion()
        {
            return Get<CopyRegion>(MethodOffset.CopyRegion);
        }

        public ViewportTransform GetViewportTransform(int index)
        {
            return Get<ViewportTransform>(MethodOffset.ViewportTransform + 8 * index);
        }

        public ViewportExtents GetViewportExtents(int index)
        {
            return Get<ViewportExtents>(MethodOffset.ViewportExtents + 4 * index);
        }

        public VertexBufferDrawState GetVertexBufferDrawState()
        {
            return Get<VertexBufferDrawState>(MethodOffset.VertexBufferDrawState);
        }

        public ClearColors GetClearColors()
        {
            return Get<ClearColors>(MethodOffset.ClearColors);
        }

        public float GetClearDepthValue()
        {
            return Get<float>(MethodOffset.ClearDepthValue);
        }

        public int GetClearStencilValue()
        {
            return _backingMemory[(int)MethodOffset.ClearStencilValue];
        }

        public StencilBackMasks GetStencilBackMasks()
        {
            return Get<StencilBackMasks>(MethodOffset.StencilBackMasks);
        }

        public RtDepthStencilState GetRtDepthStencilState()
        {
            return Get<RtDepthStencilState>(MethodOffset.RtDepthStencilState);
        }

        public VertexAttribState GetVertexAttribState(int index)
        {
            return Get<VertexAttribState>(MethodOffset.VertexAttribState + index);
        }

        public Size3D GetRtDepthStencilSize()
        {
            return Get<Size3D>(MethodOffset.RtDepthStencilSize);
        }

        public Bool GetDepthTestEnable()
        {
            return Get<Bool>(MethodOffset.DepthTestEnable);
        }

        public CompareOp GetDepthTestFunc()
        {
            return Get<CompareOp>(MethodOffset.DepthTestFunc);
        }

        public Bool GetDepthWriteEnable()
        {
            return Get<Bool>(MethodOffset.DepthWriteEnable);
        }

        public Bool GetBlendEnable(int index)
        {
            return Get<Bool>(MethodOffset.BlendEnable + index);
        }

        public StencilTestState GetStencilTestState()
        {
            return Get<StencilTestState>(MethodOffset.StencilTestState);
        }

        public int GetBaseVertex()
        {
            return _backingMemory[(int)MethodOffset.FirstVertex];
        }

        public int GetBaseInstance()
        {
            return _backingMemory[(int)MethodOffset.FirstInstance];
        }

        public PoolState GetSamplerPoolState()
        {
            return Get<PoolState>(MethodOffset.SamplerPoolState);
        }

        public PoolState GetTexturePoolState()
        {
            return Get<PoolState>(MethodOffset.TexturePoolState);
        }

        public StencilBackTestState GetStencilBackTestState()
        {
            return Get<StencilBackTestState>(MethodOffset.StencilBackTestState);
        }

        public TextureMsaaMode GetRtMsaaMode()
        {
            return Get<TextureMsaaMode>(MethodOffset.RtMsaaMode);
        }

        public GpuVa GetShaderBaseAddress()
        {
            return Get<GpuVa>(MethodOffset.ShaderBaseAddress);
        }

        public PrimitiveRestartState GetPrimitiveRestartState()
        {
            return Get<PrimitiveRestartState>(MethodOffset.PrimitiveRestartState);
        }

        public IndexBufferState GetIndexBufferState()
        {
            return Get<IndexBufferState>(MethodOffset.IndexBufferState);
        }

        public FaceState GetFaceState()
        {
            return Get<FaceState>(MethodOffset.FaceState);
        }

        public ReportState GetReportState()
        {
            return Get<ReportState>(MethodOffset.ReportState);
        }

        public VertexBufferState GetVertexBufferState(int index)
        {
            return Get<VertexBufferState>(MethodOffset.VertexBufferState + 4 * index);
        }

        public BlendState GetBlendState(int index)
        {
            return Get<BlendState>(MethodOffset.BlendState + 8 * index);
        }

        public GpuVa GetVertexBufferEndAddress(int index)
        {
            return Get<GpuVa>(MethodOffset.VertexBufferEndAddress + 2 * index);
        }

        public ShaderState GetShaderState(int index)
        {
            return Get<ShaderState>(MethodOffset.ShaderState + 16 * index);
        }

        public UniformBufferState GetUniformBufferState()
        {
            return Get<UniformBufferState>(MethodOffset.UniformBufferState);
        }

        public void SetUniformBufferOffset(int offset)
        {
            _backingMemory[(int)MethodOffset.UniformBufferState + 3] = offset;
        }

        public int GetTextureBufferIndex()
        {
            return _backingMemory[(int)MethodOffset.TextureBufferIndex];
        }

        private void InitializeDefaultState()
        {
            // Depth ranges.
            for (int index = 0; index < 8; index++)
            {
                _backingMemory[(int)MethodOffset.ViewportExtents + index * 4 + 2] = 0;
                _backingMemory[(int)MethodOffset.ViewportExtents + index * 4 + 3] = 0x3F800000;
            }

            // Default front stencil mask.
            _backingMemory[0x4e7] = 0xff;

            // Default color mask.
            _backingMemory[(int)MethodOffset.RtColorMask] = 0x1111;
        }

        private void InitializeStateWatchers()
        {
            SetWriteStateFlag(MethodOffset.RtColorState, StateWriteFlags.RtColorState, 16 * 8);

            SetWriteStateFlag(MethodOffset.ViewportTransform, StateWriteFlags.ViewportTransform, 8 * 8);
            SetWriteStateFlag(MethodOffset.ViewportExtents,   StateWriteFlags.ViewportTransform, 4 * 8);

            SetWriteStateFlag<VertexBufferDrawState>(MethodOffset.VertexBufferDrawState, StateWriteFlags.VertexBufferState);

            SetWriteStateFlag<DepthBiasState>(MethodOffset.DepthBiasState, StateWriteFlags.DepthBiasState);

            SetWriteStateFlag(MethodOffset.DepthBiasFactor, StateWriteFlags.DepthBiasState, 1);
            SetWriteStateFlag(MethodOffset.DepthBiasUnits,  StateWriteFlags.DepthBiasState, 1);
            SetWriteStateFlag(MethodOffset.DepthBiasClamp,  StateWriteFlags.DepthBiasState, 1);

            SetWriteStateFlag<RtDepthStencilState>(MethodOffset.RtDepthStencilState, StateWriteFlags.RtDepthStencilState);
            SetWriteStateFlag<Size3D>             (MethodOffset.RtDepthStencilSize,  StateWriteFlags.RtDepthStencilState);

            SetWriteStateFlag(MethodOffset.DepthTestEnable,  StateWriteFlags.DepthTestState, 1);
            SetWriteStateFlag(MethodOffset.DepthWriteEnable, StateWriteFlags.DepthTestState, 1);
            SetWriteStateFlag(MethodOffset.DepthTestFunc,    StateWriteFlags.DepthTestState, 1);

            SetWriteStateFlag(MethodOffset.VertexAttribState, StateWriteFlags.VertexAttribState, 16);

            SetWriteStateFlag<StencilBackMasks>    (MethodOffset.StencilBackMasks,     StateWriteFlags.StencilTestState);
            SetWriteStateFlag<StencilTestState>    (MethodOffset.StencilTestState,     StateWriteFlags.StencilTestState);
            SetWriteStateFlag<StencilBackTestState>(MethodOffset.StencilBackTestState, StateWriteFlags.StencilTestState);

            SetWriteStateFlag<PoolState>(MethodOffset.SamplerPoolState, StateWriteFlags.SamplerPoolState);
            SetWriteStateFlag<PoolState>(MethodOffset.TexturePoolState, StateWriteFlags.TexturePoolState);

            SetWriteStateFlag<ShaderState>(MethodOffset.ShaderBaseAddress, StateWriteFlags.ShaderState);

            SetWriteStateFlag<PrimitiveRestartState>(MethodOffset.PrimitiveRestartState, StateWriteFlags.PrimitiveRestartState);

            SetWriteStateFlag<IndexBufferState>(MethodOffset.IndexBufferState, StateWriteFlags.IndexBufferState);

            SetWriteStateFlag<FaceState>(MethodOffset.FaceState, StateWriteFlags.FaceState);

            SetWriteStateFlag<RtColorMask>(MethodOffset.RtColorMask, StateWriteFlags.RtColorMask);

            SetWriteStateFlag(MethodOffset.VertexBufferInstanced,  StateWriteFlags.VertexBufferState, 16);
            SetWriteStateFlag(MethodOffset.VertexBufferState,      StateWriteFlags.VertexBufferState, 4 * 16);
            SetWriteStateFlag(MethodOffset.VertexBufferEndAddress, StateWriteFlags.VertexBufferState, 2 * 16);

            SetWriteStateFlag(MethodOffset.BlendEnable, StateWriteFlags.BlendState, 8);
            SetWriteStateFlag(MethodOffset.BlendState,  StateWriteFlags.BlendState, 8 * 8);

            SetWriteStateFlag(MethodOffset.ShaderState, StateWriteFlags.ShaderState, 16 * 6);

            SetWriteStateFlag(MethodOffset.TextureBufferIndex, StateWriteFlags.TexturePoolState, 1);
        }

        private void SetWriteStateFlag<T>(MethodOffset offset, StateWriteFlags flag)
        {
            SetWriteStateFlag(offset, flag, Marshal.SizeOf<T>());
        }

        private void SetWriteStateFlag(MethodOffset offset, StateWriteFlags flag, int size)
        {
            for (int index = 0; index < size; index++)
            {
                _registers[(int)offset + index].WriteFlag = flag;
            }
        }

        public void RegisterCallback(MethodOffset offset, MethodCallback callback)
        {
            _registers[(int)offset].Callback = callback;
        }

        private void RegisterCallback(int offset, MethodCallback callback)
        {
            _registers[offset].Callback = callback;
        }

        public T Get<T>(MethodOffset offset) where T : struct
        {
            return MemoryMarshal.Cast<int, T>(_backingMemory.AsSpan().Slice((int)offset))[0];
        }
    }
}
