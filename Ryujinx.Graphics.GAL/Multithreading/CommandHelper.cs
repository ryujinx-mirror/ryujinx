using Ryujinx.Graphics.GAL.Multithreading.Commands;
using Ryujinx.Graphics.GAL.Multithreading.Commands.Buffer;
using Ryujinx.Graphics.GAL.Multithreading.Commands.CounterEvent;
using Ryujinx.Graphics.GAL.Multithreading.Commands.Program;
using Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer;
using Ryujinx.Graphics.GAL.Multithreading.Commands.Sampler;
using Ryujinx.Graphics.GAL.Multithreading.Commands.Texture;
using Ryujinx.Graphics.GAL.Multithreading.Commands.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.GAL.Multithreading
{
    static class CommandHelper
    {
        private delegate void CommandDelegate(Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer);

        private static int _totalCommands = (int)Enum.GetValues<CommandType>().Max() + 1;
        private static CommandDelegate[] _lookup = new CommandDelegate[_totalCommands];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref T GetCommand<T>(Span<byte> memory)
        {
            return ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(memory));
        }

        public static int GetMaxCommandSize()
        {
            Assembly assembly = typeof(CommandHelper).Assembly;

            IEnumerable<Type> commands = assembly.GetTypes().Where(type => typeof(IGALCommand).IsAssignableFrom(type) && type.IsValueType);

            int maxSize = commands.Max(command =>
            {
                MethodInfo method = typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf));
                MethodInfo generic = method.MakeGenericMethod(command);
                int size = (int)generic.Invoke(null, null);

                return size;
            });

            InitLookup();

            return maxSize + 1; // 1 byte reserved for command size.
        }

        private static void InitLookup()
        {
            _lookup[(int)CommandType.Action] = (memory, threaded, renderer) =>
                ActionCommand.Run(ref GetCommand<ActionCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CreateBuffer] = (memory, threaded, renderer) =>
                CreateBufferCommand.Run(ref GetCommand<CreateBufferCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CreateProgram] = (memory, threaded, renderer) =>
                CreateProgramCommand.Run(ref GetCommand<CreateProgramCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CreateSampler] = (memory, threaded, renderer) =>
                CreateSamplerCommand.Run(ref GetCommand<CreateSamplerCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CreateSync] = (memory, threaded, renderer) =>
                CreateSyncCommand.Run(ref GetCommand<CreateSyncCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CreateTexture] = (memory, threaded, renderer) =>
                CreateTextureCommand.Run(ref GetCommand<CreateTextureCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.GetCapabilities] = (memory, threaded, renderer) =>
                GetCapabilitiesCommand.Run(ref GetCommand<GetCapabilitiesCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.PreFrame] = (memory, threaded, renderer) =>
                PreFrameCommand.Run(ref GetCommand<PreFrameCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ReportCounter] = (memory, threaded, renderer) =>
                ReportCounterCommand.Run(ref GetCommand<ReportCounterCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ResetCounter] = (memory, threaded, renderer) =>
                ResetCounterCommand.Run(ref GetCommand<ResetCounterCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.UpdateCounters] = (memory, threaded, renderer) =>
                UpdateCountersCommand.Run(ref GetCommand<UpdateCountersCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.BufferDispose] = (memory, threaded, renderer) =>
                BufferDisposeCommand.Run(ref GetCommand<BufferDisposeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.BufferGetData] = (memory, threaded, renderer) =>
                BufferGetDataCommand.Run(ref GetCommand<BufferGetDataCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.BufferSetData] = (memory, threaded, renderer) =>
                BufferSetDataCommand.Run(ref GetCommand<BufferSetDataCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.CounterEventDispose] = (memory, threaded, renderer) =>
                CounterEventDisposeCommand.Run(ref GetCommand<CounterEventDisposeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CounterEventFlush] = (memory, threaded, renderer) =>
                CounterEventFlushCommand.Run(ref GetCommand<CounterEventFlushCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.ProgramDispose] = (memory, threaded, renderer) =>
                ProgramDisposeCommand.Run(ref GetCommand<ProgramDisposeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ProgramGetBinary] = (memory, threaded, renderer) =>
                ProgramGetBinaryCommand.Run(ref GetCommand<ProgramGetBinaryCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ProgramCheckLink] = (memory, threaded, renderer) =>
                ProgramCheckLinkCommand.Run(ref GetCommand<ProgramCheckLinkCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.SamplerDispose] = (memory, threaded, renderer) =>
                SamplerDisposeCommand.Run(ref GetCommand<SamplerDisposeCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.TextureCopyTo] = (memory, threaded, renderer) =>
                TextureCopyToCommand.Run(ref GetCommand<TextureCopyToCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureCopyToScaled] = (memory, threaded, renderer) =>
                TextureCopyToScaledCommand.Run(ref GetCommand<TextureCopyToScaledCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureCopyToSlice] = (memory, threaded, renderer) =>
                TextureCopyToSliceCommand.Run(ref GetCommand<TextureCopyToSliceCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureCreateView] = (memory, threaded, renderer) =>
                TextureCreateViewCommand.Run(ref GetCommand<TextureCreateViewCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureGetData] = (memory, threaded, renderer) =>
                TextureGetDataCommand.Run(ref GetCommand<TextureGetDataCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureGetDataSlice] = (memory, threaded, renderer) =>
                TextureGetDataSliceCommand.Run(ref GetCommand<TextureGetDataSliceCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureRelease] = (memory, threaded, renderer) =>
                TextureReleaseCommand.Run(ref GetCommand<TextureReleaseCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureSetData] = (memory, threaded, renderer) =>
                TextureSetDataCommand.Run(ref GetCommand<TextureSetDataCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureSetDataSlice] = (memory, threaded, renderer) =>
                TextureSetDataSliceCommand.Run(ref GetCommand<TextureSetDataSliceCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureSetDataSliceRegion] = (memory, threaded, renderer) =>
                TextureSetDataSliceRegionCommand.Run(ref GetCommand<TextureSetDataSliceRegionCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureSetStorage] = (memory, threaded, renderer) =>
                TextureSetStorageCommand.Run(ref GetCommand<TextureSetStorageCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.WindowPresent] = (memory, threaded, renderer) =>
                WindowPresentCommand.Run(ref GetCommand<WindowPresentCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.Barrier] = (memory, threaded, renderer) =>
                BarrierCommand.Run(ref GetCommand<BarrierCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.BeginTransformFeedback] = (memory, threaded, renderer) =>
                BeginTransformFeedbackCommand.Run(ref GetCommand<BeginTransformFeedbackCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ClearBuffer] = (memory, threaded, renderer) =>
                ClearBufferCommand.Run(ref GetCommand<ClearBufferCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ClearRenderTargetColor] = (memory, threaded, renderer) =>
                ClearRenderTargetColorCommand.Run(ref GetCommand<ClearRenderTargetColorCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ClearRenderTargetDepthStencil] = (memory, threaded, renderer) =>
                ClearRenderTargetDepthStencilCommand.Run(ref GetCommand<ClearRenderTargetDepthStencilCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CommandBufferBarrier] = (memory, threaded, renderer) =>
                CommandBufferBarrierCommand.Run(ref GetCommand<CommandBufferBarrierCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CopyBuffer] = (memory, threaded, renderer) =>
                CopyBufferCommand.Run(ref GetCommand<CopyBufferCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.DispatchCompute] = (memory, threaded, renderer) =>
                DispatchComputeCommand.Run(ref GetCommand<DispatchComputeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.Draw] = (memory, threaded, renderer) =>
                DrawCommand.Run(ref GetCommand<DrawCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.DrawIndexed] = (memory, threaded, renderer) =>
                DrawIndexedCommand.Run(ref GetCommand<DrawIndexedCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.DrawIndexedIndirect] = (memory, threaded, renderer) =>
                DrawIndexedIndirectCommand.Run(ref GetCommand<DrawIndexedIndirectCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.DrawIndexedIndirectCount] = (memory, threaded, renderer) =>
                DrawIndexedIndirectCountCommand.Run(ref GetCommand<DrawIndexedIndirectCountCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.DrawIndirect] = (memory, threaded, renderer) =>
                DrawIndirectCommand.Run(ref GetCommand<DrawIndirectCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.DrawIndirectCount] = (memory, threaded, renderer) =>
                DrawIndirectCountCommand.Run(ref GetCommand<DrawIndirectCountCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.DrawTexture] = (memory, threaded, renderer) =>
                DrawTextureCommand.Run(ref GetCommand<DrawTextureCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.EndHostConditionalRendering] = (memory, threaded, renderer) =>
                EndHostConditionalRenderingCommand.Run(renderer);
            _lookup[(int)CommandType.EndTransformFeedback] = (memory, threaded, renderer) =>
                EndTransformFeedbackCommand.Run(ref GetCommand<EndTransformFeedbackCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetAlphaTest] = (memory, threaded, renderer) =>
                SetAlphaTestCommand.Run(ref GetCommand<SetAlphaTestCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetBlendState] = (memory, threaded, renderer) =>
                SetBlendStateCommand.Run(ref GetCommand<SetBlendStateCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetDepthBias] = (memory, threaded, renderer) =>
                SetDepthBiasCommand.Run(ref GetCommand<SetDepthBiasCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetDepthClamp] = (memory, threaded, renderer) =>
                SetDepthClampCommand.Run(ref GetCommand<SetDepthClampCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetDepthMode] = (memory, threaded, renderer) =>
                SetDepthModeCommand.Run(ref GetCommand<SetDepthModeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetDepthTest] = (memory, threaded, renderer) =>
                SetDepthTestCommand.Run(ref GetCommand<SetDepthTestCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetFaceCulling] = (memory, threaded, renderer) =>
                SetFaceCullingCommand.Run(ref GetCommand<SetFaceCullingCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetFrontFace] = (memory, threaded, renderer) =>
                SetFrontFaceCommand.Run(ref GetCommand<SetFrontFaceCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetStorageBuffers] = (memory, threaded, renderer) =>
                SetStorageBuffersCommand.Run(ref GetCommand<SetStorageBuffersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetTransformFeedbackBuffers] = (memory, threaded, renderer) =>
                SetTransformFeedbackBuffersCommand.Run(ref GetCommand<SetTransformFeedbackBuffersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetUniformBuffers] = (memory, threaded, renderer) =>
                SetUniformBuffersCommand.Run(ref GetCommand<SetUniformBuffersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetImage] = (memory, threaded, renderer) =>
                SetImageCommand.Run(ref GetCommand<SetImageCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetIndexBuffer] = (memory, threaded, renderer) =>
                SetIndexBufferCommand.Run(ref GetCommand<SetIndexBufferCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetLineParameters] = (memory, threaded, renderer) =>
                SetLineParametersCommand.Run(ref GetCommand<SetLineParametersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetLogicOpState] = (memory, threaded, renderer) =>
                SetLogicOpStateCommand.Run(ref GetCommand<SetLogicOpStateCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetMultisampleState] = (memory, threaded, renderer) =>
                SetMultisampleStateCommand.Run(ref GetCommand<SetMultisampleStateCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetPatchParameters] = (memory, threaded, renderer) =>
                SetPatchParametersCommand.Run(ref GetCommand<SetPatchParametersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetPointParameters] = (memory, threaded, renderer) =>
                SetPointParametersCommand.Run(ref GetCommand<SetPointParametersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetPolygonMode] = (memory, threaded, renderer) =>
                SetPolygonModeCommand.Run(ref GetCommand<SetPolygonModeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetPrimitiveRestart] = (memory, threaded, renderer) =>
                SetPrimitiveRestartCommand.Run(ref GetCommand<SetPrimitiveRestartCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetPrimitiveTopology] = (memory, threaded, renderer) =>
                SetPrimitiveTopologyCommand.Run(ref GetCommand<SetPrimitiveTopologyCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetProgram] = (memory, threaded, renderer) =>
                SetProgramCommand.Run(ref GetCommand<SetProgramCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetRasterizerDiscard] = (memory, threaded, renderer) =>
                SetRasterizerDiscardCommand.Run(ref GetCommand<SetRasterizerDiscardCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetRenderTargetColorMasks] = (memory, threaded, renderer) =>
                SetRenderTargetColorMasksCommand.Run(ref GetCommand<SetRenderTargetColorMasksCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetRenderTargetScale] = (memory, threaded, renderer) =>
                SetRenderTargetScaleCommand.Run(ref GetCommand<SetRenderTargetScaleCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetRenderTargets] = (memory, threaded, renderer) =>
                SetRenderTargetsCommand.Run(ref GetCommand<SetRenderTargetsCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetScissor] = (memory, threaded, renderer) =>
                SetScissorsCommand.Run(ref GetCommand<SetScissorsCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetStencilTest] = (memory, threaded, renderer) =>
                SetStencilTestCommand.Run(ref GetCommand<SetStencilTestCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetTextureAndSampler] = (memory, threaded, renderer) =>
                SetTextureAndSamplerCommand.Run(ref GetCommand<SetTextureAndSamplerCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetUserClipDistance] = (memory, threaded, renderer) =>
                SetUserClipDistanceCommand.Run(ref GetCommand<SetUserClipDistanceCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetVertexAttribs] = (memory, threaded, renderer) =>
                SetVertexAttribsCommand.Run(ref GetCommand<SetVertexAttribsCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetVertexBuffers] = (memory, threaded, renderer) =>
                SetVertexBuffersCommand.Run(ref GetCommand<SetVertexBuffersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetViewports] = (memory, threaded, renderer) =>
                SetViewportsCommand.Run(ref GetCommand<SetViewportsCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureBarrier] = (memory, threaded, renderer) =>
                TextureBarrierCommand.Run(ref GetCommand<TextureBarrierCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureBarrierTiled] = (memory, threaded, renderer) =>
                TextureBarrierTiledCommand.Run(ref GetCommand<TextureBarrierTiledCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TryHostConditionalRendering] = (memory, threaded, renderer) =>
                TryHostConditionalRenderingCommand.Run(ref GetCommand<TryHostConditionalRenderingCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TryHostConditionalRenderingFlush] = (memory, threaded, renderer) =>
                TryHostConditionalRenderingFlushCommand.Run(ref GetCommand<TryHostConditionalRenderingFlushCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.UpdateRenderScale] = (memory, threaded, renderer) =>
                UpdateRenderScaleCommand.Run(ref GetCommand<UpdateRenderScaleCommand>(memory), threaded, renderer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunCommand(Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer)
        {
            _lookup[memory[memory.Length - 1]](memory, threaded, renderer);
        }
    }
}
