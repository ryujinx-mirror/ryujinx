namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// State used by the <see cref="GpuAccessor"/>.
    /// </summary>
    class GpuAccessorState
    {
        /// <summary>
        /// GPU texture pool state.
        /// </summary>
        public readonly GpuChannelPoolState PoolState;

        /// <summary>
        /// GPU compute state, for compute shaders.
        /// </summary>
        public readonly GpuChannelComputeState ComputeState;

        /// <summary>
        /// GPU graphics state, for vertex, tessellation, geometry and fragment shaders.
        /// </summary>
        public readonly GpuChannelGraphicsState GraphicsState;

        /// <summary>
        /// Shader specialization state (shared by all stages).
        /// </summary>
        public readonly ShaderSpecializationState SpecializationState;

        /// <summary>
        /// Transform feedback information, if the shader uses transform feedback. Otherwise, should be null.
        /// </summary>
        public readonly TransformFeedbackDescriptor[] TransformFeedbackDescriptors;

        /// <summary>
        /// Shader resource counts (shared by all stages).
        /// </summary>
        public readonly ResourceCounts ResourceCounts;

        /// <summary>
        /// Creates a new GPU accessor state.
        /// </summary>
        /// <param name="poolState">GPU texture pool state</param>
        /// <param name="computeState">GPU compute state, for compute shaders</param>
        /// <param name="graphicsState">GPU graphics state, for vertex, tessellation, geometry and fragment shaders</param>
        /// <param name="specializationState">Shader specialization state (shared by all stages)</param>
        /// <param name="transformFeedbackDescriptors">Transform feedback information, if the shader uses transform feedback. Otherwise, should be null</param>
        public GpuAccessorState(
            GpuChannelPoolState poolState,
            GpuChannelComputeState computeState,
            GpuChannelGraphicsState graphicsState,
            ShaderSpecializationState specializationState,
            TransformFeedbackDescriptor[] transformFeedbackDescriptors = null)
        {
            PoolState = poolState;
            GraphicsState = graphicsState;
            ComputeState = computeState;
            SpecializationState = specializationState;
            TransformFeedbackDescriptors = transformFeedbackDescriptors;
            ResourceCounts = new ResourceCounts();
        }
    }
}