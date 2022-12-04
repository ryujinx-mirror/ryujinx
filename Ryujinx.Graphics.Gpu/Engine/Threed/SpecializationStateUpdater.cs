using Ryujinx.Common.Memory;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Engine.Threed
{
    /// <summary>
    /// Maintains a "current" specialiation state, and provides a flag to check if it has changed meaningfully.
    /// </summary>
    internal class SpecializationStateUpdater
    {
        private GpuChannelGraphicsState _graphics;
        private GpuChannelPoolState _pool;

        private bool _usesDrawParameters;
        private bool _usesTopology;

        private bool _changed;

        /// <summary>
        /// Signal that the specialization state has changed.
        /// </summary>
        private void Signal()
        {
            _changed = true;
        }

        /// <summary>
        /// Checks if the specialization state has changed since the last check.
        /// </summary>
        /// <returns>True if it has changed, false otherwise</returns>
        public bool HasChanged()
        {
            if (_changed)
            {
                _changed = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the active shader, clearing the dirty state and recording if certain specializations are noteworthy.
        /// </summary>
        /// <param name="gs">The active shader</param>
        public void SetShader(CachedShaderProgram gs)
        {
            _usesDrawParameters = gs.Shaders[1]?.Info.UsesDrawParameters ?? false;
            _usesTopology = gs.SpecializationState.IsPrimitiveTopologyQueried();

            _changed = false;
        }

        /// <summary>
        /// Get the current graphics state.
        /// </summary>
        /// <returns>GPU graphics state</returns>
        public ref GpuChannelGraphicsState GetGraphicsState()
        {
            return ref _graphics;
        }

        /// <summary>
        /// Get the current pool state.
        /// </summary>
        /// <returns>GPU pool state</returns>
        public ref GpuChannelPoolState GetPoolState()
        {
            return ref _pool;
        }

        /// <summary>
        /// Early Z force enable.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetEarlyZForce(bool value)
        {
            _graphics.EarlyZForce = value;

            Signal();
        }

        /// <summary>
        /// Primitive topology of current draw.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetTopology(PrimitiveTopology value)
        {
            if (value != _graphics.Topology)
            {
                _graphics.Topology = value;

                if (_usesTopology)
                {
                    Signal();
                }
            }
        }

        /// <summary>
        /// Tessellation mode.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetTessellationMode(TessMode value)
        {
            if (value.Packed != _graphics.TessellationMode.Packed)
            {
                _graphics.TessellationMode = value;

                Signal();
            }
        }

        /// <summary>
        /// Updates alpha-to-coverage state, and sets it as changed.
        /// </summary>
        /// <param name="enable">Whether alpha-to-coverage is enabled</param>
        /// <param name="ditherEnable">Whether alpha-to-coverage dithering is enabled</param>
        public void SetAlphaToCoverageEnable(bool enable, bool ditherEnable)
        {
            _graphics.AlphaToCoverageEnable = enable;
            _graphics.AlphaToCoverageDitherEnable = ditherEnable;

            Signal();
        }

        /// <summary>
        /// Indicates whether the viewport transform is disabled.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetViewportTransformDisable(bool value)
        {
            if (value != _graphics.ViewportTransformDisable)
            {
                _graphics.ViewportTransformDisable = value;

                Signal();
            }
        }

        /// <summary>
        /// Depth mode zero to one or minus one to one.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetDepthMode(bool value)
        {
            if (value != _graphics.DepthMode)
            {
                _graphics.DepthMode = value;

                Signal();
            }
        }

        /// <summary>
        /// Indicates if the point size is set on the shader or is fixed.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetProgramPointSizeEnable(bool value)
        {
            if (value != _graphics.ProgramPointSizeEnable)
            {
                _graphics.ProgramPointSizeEnable = value;

                Signal();
            }
        }

        /// <summary>
        /// Point size used if <see cref="SetProgramPointSizeEnable" /> is provided false.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetPointSize(float value)
        {
            if (value != _graphics.PointSize)
            {
                _graphics.PointSize = value;

                Signal();
            }
        }

        /// <summary>
        /// Updates alpha test specialization state, and sets it as changed.
        /// </summary>
        /// <param name="enable">Whether alpha test is enabled</param>
        /// <param name="reference">The value to compare with the fragment output alpha</param>
        /// <param name="op">The comparison that decides if the fragment should be discarded</param>
        public void SetAlphaTest(bool enable, float reference, CompareOp op)
        {
            _graphics.AlphaTestEnable = enable;
            _graphics.AlphaTestReference = reference;
            _graphics.AlphaTestCompare = op;

            Signal();
        }

        /// <summary>
        /// Updates the type of the vertex attributes consumed by the shader.
        /// </summary>
        /// <param name="state">The new state</param>
        public void SetAttributeTypes(ref Array32<VertexAttribState> state)
        {
            bool changed = false;
            ref Array32<AttributeType> attributeTypes = ref _graphics.AttributeTypes;

            for (int location = 0; location < state.Length; location++)
            {
                VertexAttribType type = state[location].UnpackType();

                AttributeType value = type switch
                {
                    VertexAttribType.Sint => AttributeType.Sint,
                    VertexAttribType.Uint => AttributeType.Uint,
                    _ => AttributeType.Float
                };

                if (attributeTypes[location] != value)
                {
                    attributeTypes[location] = value;
                    changed = true;
                }
            }

            if (changed)
            {
                Signal();
            }
        }

        /// <summary>
        /// Indicates that the draw is writing the base vertex, base instance and draw index to Constant Buffer 0.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetHasConstantBufferDrawParameters(bool value)
        {
            if (value != _graphics.HasConstantBufferDrawParameters)
            {
                _graphics.HasConstantBufferDrawParameters = value;

                if (_usesDrawParameters)
                {
                    Signal();
                }
            }
        }

        /// <summary>
        /// Indicates that any storage buffer use is unaligned.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetHasUnalignedStorageBuffer(bool value)
        {
            if (value != _graphics.HasUnalignedStorageBuffer)
            {
                _graphics.HasUnalignedStorageBuffer = value;

                Signal();
            }
        }

        /// <summary>
        /// Sets the GPU pool state.
        /// </summary>
        /// <param name="state">The new state</param>
        public void SetPoolState(GpuChannelPoolState state)
        {
            if (!state.Equals(_pool))
            {
                _pool = state;

                Signal();
            }
        }
    }
}
