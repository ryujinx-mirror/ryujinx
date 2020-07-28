using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Memory cache of shader code.
    /// </summary>
    class ShaderCache : IDisposable
    {
        private const TranslationFlags DefaultFlags = TranslationFlags.DebugMode;

        private readonly GpuContext _context;

        private readonly ShaderDumper _dumper;

        private readonly Dictionary<ulong, List<ShaderBundle>> _cpPrograms;
        private readonly Dictionary<ShaderAddresses, List<ShaderBundle>> _gpPrograms;

        /// <summary>
        /// Creates a new instance of the shader cache.
        /// </summary>
        /// <param name="context">GPU context that the shader cache belongs to</param>
        public ShaderCache(GpuContext context)
        {
            _context = context;

            _dumper = new ShaderDumper();

            _cpPrograms = new Dictionary<ulong, List<ShaderBundle>>();
            _gpPrograms = new Dictionary<ShaderAddresses, List<ShaderBundle>>();
        }

        /// <summary>
        /// Gets a compute shader from the cache.
        /// </summary>
        /// <remarks>
        /// This automatically translates, compiles and adds the code to the cache if not present.
        /// </remarks>
        /// <param name="state">Current GPU state</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <param name="localSizeX">Local group size X of the computer shader</param>
        /// <param name="localSizeY">Local group size Y of the computer shader</param>
        /// <param name="localSizeZ">Local group size Z of the computer shader</param>
        /// <param name="localMemorySize">Local memory size of the compute shader</param>
        /// <param name="sharedMemorySize">Shared memory size of the compute shader</param>
        /// <returns>Compiled compute shader code</returns>
        public ShaderBundle GetComputeShader(
            GpuState state,
            ulong gpuVa,
            int localSizeX,
            int localSizeY,
            int localSizeZ,
            int localMemorySize,
            int sharedMemorySize)
        {
            bool isCached = _cpPrograms.TryGetValue(gpuVa, out List<ShaderBundle> list);

            if (isCached)
            {
                foreach (ShaderBundle cachedCpShader in list)
                {
                    if (IsShaderEqual(cachedCpShader, gpuVa))
                    {
                        return cachedCpShader;
                    }
                }
            }

            ShaderCodeHolder shader = TranslateComputeShader(
                state,
                gpuVa,
                localSizeX,
                localSizeY,
                localSizeZ,
                localMemorySize,
                sharedMemorySize);

            shader.HostShader = _context.Renderer.CompileShader(shader.Program);

            IProgram hostProgram = _context.Renderer.CreateProgram(new IShader[] { shader.HostShader }, null);

            ShaderBundle cpShader = new ShaderBundle(hostProgram, shader);

            if (!isCached)
            {
                list = new List<ShaderBundle>();

                _cpPrograms.Add(gpuVa, list);
            }

            list.Add(cpShader);

            return cpShader;
        }

        /// <summary>
        /// Gets a graphics shader program from the shader cache.
        /// This includes all the specified shader stages.
        /// </summary>
        /// <remarks>
        /// This automatically translates, compiles and adds the code to the cache if not present.
        /// </remarks>
        /// <param name="state">Current GPU state</param>
        /// <param name="addresses">Addresses of the shaders for each stage</param>
        /// <returns>Compiled graphics shader code</returns>
        public ShaderBundle GetGraphicsShader(GpuState state, ShaderAddresses addresses)
        {
            bool isCached = _gpPrograms.TryGetValue(addresses, out List<ShaderBundle> list);

            if (isCached)
            {
                foreach (ShaderBundle cachedGpShaders in list)
                {
                    if (IsShaderEqual(cachedGpShaders, addresses))
                    {
                        return cachedGpShaders;
                    }
                }
            }

            ShaderCodeHolder[] shaders = new ShaderCodeHolder[Constants.ShaderStages];

            var tfd = GetTransformFeedbackDescriptors(state);

            TranslationFlags flags = DefaultFlags;

            if (tfd != null)
            {
                flags |= TranslationFlags.Feedback;
            }

            if (addresses.VertexA != 0)
            {
                shaders[0] = TranslateGraphicsShader(state, flags, ShaderStage.Vertex, addresses.Vertex, addresses.VertexA);
            }
            else
            {
                shaders[0] = TranslateGraphicsShader(state, flags, ShaderStage.Vertex, addresses.Vertex);
            }

            shaders[1] = TranslateGraphicsShader(state, flags, ShaderStage.TessellationControl,    addresses.TessControl);
            shaders[2] = TranslateGraphicsShader(state, flags, ShaderStage.TessellationEvaluation, addresses.TessEvaluation);
            shaders[3] = TranslateGraphicsShader(state, flags, ShaderStage.Geometry,               addresses.Geometry);
            shaders[4] = TranslateGraphicsShader(state, flags, ShaderStage.Fragment,               addresses.Fragment);

            List<IShader> hostShaders = new List<IShader>();

            for (int stage = 0; stage < Constants.ShaderStages; stage++)
            {
                ShaderProgram program = shaders[stage]?.Program;

                if (program == null)
                {
                    continue;
                }

                IShader hostShader = _context.Renderer.CompileShader(program);

                shaders[stage].HostShader = hostShader;

                hostShaders.Add(hostShader);
            }

            IProgram hostProgram = _context.Renderer.CreateProgram(hostShaders.ToArray(), tfd);

            ShaderBundle gpShaders = new ShaderBundle(hostProgram, shaders);

            if (!isCached)
            {
                list = new List<ShaderBundle>();

                _gpPrograms.Add(addresses, list);
            }

            list.Add(gpShaders);

            return gpShaders;
        }

        /// <summary>
        /// Gets transform feedback state from the current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <returns>Four transform feedback descriptors for the enabled TFBs, or null if TFB is disabled</returns>
        private TransformFeedbackDescriptor[] GetTransformFeedbackDescriptors(GpuState state)
        {
            bool tfEnable = state.Get<Boolean32>(MethodOffset.TfEnable);

            if (!tfEnable)
            {
                return null;
            }

            TransformFeedbackDescriptor[] descs = new TransformFeedbackDescriptor[Constants.TotalTransformFeedbackBuffers];

            for (int i = 0; i < Constants.TotalTransformFeedbackBuffers; i++)
            {
                var tf = state.Get<TfState>(MethodOffset.TfState, i);

                int length = (int)Math.Min((uint)tf.VaryingsCount, 0x80);

                var varyingLocations = state.GetSpan(MethodOffset.TfVaryingLocations + i * 0x80, length).ToArray();

                descs[i] = new TransformFeedbackDescriptor(tf.BufferIndex, tf.Stride, varyingLocations);
            }

            return descs;
        }

        /// <summary>
        /// Checks if compute shader code in memory is equal to the cached shader.
        /// </summary>
        /// <param name="cpShader">Cached compute shader</param>
        /// <param name="gpuVa">GPU virtual address of the shader code in memory</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private bool IsShaderEqual(ShaderBundle cpShader, ulong gpuVa)
        {
            return IsShaderEqual(cpShader.Shaders[0], gpuVa);
        }

        /// <summary>
        /// Checks if graphics shader code from all stages in memory are equal to the cached shaders.
        /// </summary>
        /// <param name="gpShaders">Cached graphics shaders</param>
        /// <param name="addresses">GPU virtual addresses of all enabled shader stages</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private bool IsShaderEqual(ShaderBundle gpShaders, ShaderAddresses addresses)
        {
            for (int stage = 0; stage < gpShaders.Shaders.Length; stage++)
            {
                ShaderCodeHolder shader = gpShaders.Shaders[stage];

                ulong gpuVa = 0;

                switch (stage)
                {
                    case 0: gpuVa = addresses.Vertex;         break;
                    case 1: gpuVa = addresses.TessControl;    break;
                    case 2: gpuVa = addresses.TessEvaluation; break;
                    case 3: gpuVa = addresses.Geometry;       break;
                    case 4: gpuVa = addresses.Fragment;       break;
                }

                if (!IsShaderEqual(shader, gpuVa, addresses.VertexA))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the code of the specified cached shader is different from the code in memory.
        /// </summary>
        /// <param name="shader">Cached shader to compare with</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <param name="gpuVaA">Optional GPU virtual address of the "Vertex A" binary shader code</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private bool IsShaderEqual(ShaderCodeHolder shader, ulong gpuVa, ulong gpuVaA = 0)
        {
            if (shader == null)
            {
                return true;
            }

            ReadOnlySpan<byte> memoryCode = _context.MemoryManager.GetSpan(gpuVa, shader.Code.Length);

            bool equals = memoryCode.SequenceEqual(shader.Code);

            if (equals && shader.Code2 != null)
            {
                memoryCode = _context.MemoryManager.GetSpan(gpuVaA, shader.Code2.Length);

                equals = memoryCode.SequenceEqual(shader.Code2);
            }

            return equals;
        }

        /// <summary>
        /// Translates the binary Maxwell shader code to something that the host API accepts.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <param name="localSizeX">Local group size X of the computer shader</param>
        /// <param name="localSizeY">Local group size Y of the computer shader</param>
        /// <param name="localSizeZ">Local group size Z of the computer shader</param>
        /// <param name="localMemorySize">Local memory size of the compute shader</param>
        /// <param name="sharedMemorySize">Shared memory size of the compute shader</param>
        /// <returns>Compiled compute shader code</returns>
        private ShaderCodeHolder TranslateComputeShader(
            GpuState state,
            ulong gpuVa,
            int localSizeX,
            int localSizeY,
            int localSizeZ,
            int localMemorySize,
            int sharedMemorySize)
        {
            if (gpuVa == 0)
            {
                return null;
            }

            GpuAccessor gpuAccessor = new GpuAccessor(_context, state, localSizeX, localSizeY, localSizeZ, localMemorySize, sharedMemorySize);

            ShaderProgram program;

            program = Translator.Translate(gpuVa, gpuAccessor, DefaultFlags | TranslationFlags.Compute);

            byte[] code = _context.MemoryManager.GetSpan(gpuVa, program.Size).ToArray();

            _dumper.Dump(code, compute: true, out string fullPath, out string codePath);

            if (fullPath != null && codePath != null)
            {
                program.Prepend("// " + codePath);
                program.Prepend("// " + fullPath);
            }

            return new ShaderCodeHolder(program, code);
        }

        /// <summary>
        /// Translates the binary Maxwell shader code to something that the host API accepts.
        /// </summary>
        /// <remarks>
        /// This will combine the "Vertex A" and "Vertex B" shader stages, if specified, into one shader.
        /// </remarks>
        /// <param name="state">Current GPU state</param>
        /// <param name="flags">Flags that controls shader translation</param>
        /// <param name="stage">Shader stage</param>
        /// <param name="gpuVa">GPU virtual address of the shader code</param>
        /// <param name="gpuVaA">Optional GPU virtual address of the "Vertex A" shader code</param>
        /// <returns>Compiled graphics shader code</returns>
        private ShaderCodeHolder TranslateGraphicsShader(GpuState state, TranslationFlags flags, ShaderStage stage, ulong gpuVa, ulong gpuVaA = 0)
        {
            if (gpuVa == 0)
            {
                return null;
            }

            GpuAccessor gpuAccessor = new GpuAccessor(_context, state, (int)stage - 1);

            if (gpuVaA != 0)
            {
                ShaderProgram program = Translator.Translate(gpuVaA, gpuVa, gpuAccessor, flags);

                byte[] codeA = _context.MemoryManager.GetSpan(gpuVaA, program.SizeA).ToArray();
                byte[] codeB = _context.MemoryManager.GetSpan(gpuVa,  program.Size).ToArray();

                _dumper.Dump(codeA, compute: false, out string fullPathA, out string codePathA);
                _dumper.Dump(codeB, compute: false, out string fullPathB, out string codePathB);

                if (fullPathA != null && fullPathB != null && codePathA != null && codePathB != null)
                {
                    program.Prepend("// " + codePathB);
                    program.Prepend("// " + fullPathB);
                    program.Prepend("// " + codePathA);
                    program.Prepend("// " + fullPathA);
                }

                return new ShaderCodeHolder(program, codeB, codeA);
            }
            else
            {
                ShaderProgram program = Translator.Translate(gpuVa, gpuAccessor, flags);

                byte[] code = _context.MemoryManager.GetSpan(gpuVa, program.Size).ToArray();

                _dumper.Dump(code, compute: false, out string fullPath, out string codePath);

                if (fullPath != null && codePath != null)
                {
                    program.Prepend("// " + codePath);
                    program.Prepend("// " + fullPath);
                }

                return new ShaderCodeHolder(program, code);
            }
        }

        /// <summary>
        /// Disposes the shader cache, deleting all the cached shaders.
        /// It's an error to use the shader cache after disposal.
        /// </summary>
        public void Dispose()
        {
            foreach (List<ShaderBundle> list in _cpPrograms.Values)
            {
                foreach (ShaderBundle bundle in list)
                {
                    bundle.Dispose();
                }
            }

            foreach (List<ShaderBundle> list in _gpPrograms.Values)
            {
                foreach (ShaderBundle bundle in list)
                {
                    bundle.Dispose();
                }
            }
        }
    }
}