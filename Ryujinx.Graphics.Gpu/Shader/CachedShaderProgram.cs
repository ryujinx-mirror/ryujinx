using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Represents a program composed of one or more shader stages (for graphics shaders),
    /// or a single shader (for compute shaders).
    /// </summary>
    class CachedShaderProgram : IDisposable
    {
        /// <summary>
        /// Host shader program object.
        /// </summary>
        public IProgram HostProgram { get; }

        /// <summary>
        /// GPU state used to create this version of the shader.
        /// </summary>
        public ShaderSpecializationState SpecializationState { get; }

        /// <summary>
        /// Compiled shader for each shader stage.
        /// </summary>
        public CachedShaderStage[] Shaders { get; }

        /// <summary>
        /// Creates a new instance of the shader bundle.
        /// </summary>
        /// <param name="hostProgram">Host program with all the shader stages</param>
        /// <param name="specializationState">GPU state used to create this version of the shader</param>
        /// <param name="shaders">Shaders</param>
        public CachedShaderProgram(IProgram hostProgram, ShaderSpecializationState specializationState, params CachedShaderStage[] shaders)
        {
            HostProgram = hostProgram;
            SpecializationState = specializationState;
            Shaders = shaders;
        }

        /// <summary>
        /// Dispose of the host shader resources.
        /// </summary>
        public void Dispose()
        {
            HostProgram.Dispose();
        }
    }
}
