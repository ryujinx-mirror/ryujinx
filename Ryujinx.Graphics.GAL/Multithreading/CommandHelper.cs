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
            _lookup[(int)CommandType.Action] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                ActionCommand.Run(ref GetCommand<ActionCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CreateBuffer] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                CreateBufferCommand.Run(ref GetCommand<CreateBufferCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CreateProgram] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                CreateProgramCommand.Run(ref GetCommand<CreateProgramCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CreateSampler] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                CreateSamplerCommand.Run(ref GetCommand<CreateSamplerCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CreateSync] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                CreateSyncCommand.Run(ref GetCommand<CreateSyncCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CreateTexture] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                CreateTextureCommand.Run(ref GetCommand<CreateTextureCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.GetCapabilities] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                GetCapabilitiesCommand.Run(ref GetCommand<GetCapabilitiesCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.PreFrame] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                PreFrameCommand.Run(ref GetCommand<PreFrameCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ReportCounter] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                ReportCounterCommand.Run(ref GetCommand<ReportCounterCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ResetCounter] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                ResetCounterCommand.Run(ref GetCommand<ResetCounterCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.UpdateCounters] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                UpdateCountersCommand.Run(ref GetCommand<UpdateCountersCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.BufferDispose] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                BufferDisposeCommand.Run(ref GetCommand<BufferDisposeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.BufferGetData] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                BufferGetDataCommand.Run(ref GetCommand<BufferGetDataCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.BufferSetData] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                BufferSetDataCommand.Run(ref GetCommand<BufferSetDataCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.CounterEventDispose] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                CounterEventDisposeCommand.Run(ref GetCommand<CounterEventDisposeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CounterEventFlush] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                CounterEventFlushCommand.Run(ref GetCommand<CounterEventFlushCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.ProgramDispose] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                ProgramDisposeCommand.Run(ref GetCommand<ProgramDisposeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ProgramGetBinary] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                ProgramGetBinaryCommand.Run(ref GetCommand<ProgramGetBinaryCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ProgramCheckLink] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                ProgramCheckLinkCommand.Run(ref GetCommand<ProgramCheckLinkCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.SamplerDispose] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SamplerDisposeCommand.Run(ref GetCommand<SamplerDisposeCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.TextureCopyTo] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureCopyToCommand.Run(ref GetCommand<TextureCopyToCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureCopyToScaled] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureCopyToScaledCommand.Run(ref GetCommand<TextureCopyToScaledCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureCopyToSlice] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureCopyToSliceCommand.Run(ref GetCommand<TextureCopyToSliceCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureCreateView] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureCreateViewCommand.Run(ref GetCommand<TextureCreateViewCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureGetData] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureGetDataCommand.Run(ref GetCommand<TextureGetDataCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureGetDataSlice] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureGetDataSliceCommand.Run(ref GetCommand<TextureGetDataSliceCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureRelease] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureReleaseCommand.Run(ref GetCommand<TextureReleaseCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureSetData] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureSetDataCommand.Run(ref GetCommand<TextureSetDataCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureSetDataSlice] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureSetDataSliceCommand.Run(ref GetCommand<TextureSetDataSliceCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureSetStorage] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureSetStorageCommand.Run(ref GetCommand<TextureSetStorageCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.WindowPresent] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                WindowPresentCommand.Run(ref GetCommand<WindowPresentCommand>(memory), threaded, renderer);

            _lookup[(int)CommandType.Barrier] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                BarrierCommand.Run(ref GetCommand<BarrierCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.BeginTransformFeedback] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                BeginTransformFeedbackCommand.Run(ref GetCommand<BeginTransformFeedbackCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ClearBuffer] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                ClearBufferCommand.Run(ref GetCommand<ClearBufferCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ClearRenderTargetColor] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                ClearRenderTargetColorCommand.Run(ref GetCommand<ClearRenderTargetColorCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.ClearRenderTargetDepthStencil] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                ClearRenderTargetDepthStencilCommand.Run(ref GetCommand<ClearRenderTargetDepthStencilCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CommandBufferBarrier] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                CommandBufferBarrierCommand.Run(ref GetCommand<CommandBufferBarrierCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.CopyBuffer] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                CopyBufferCommand.Run(ref GetCommand<CopyBufferCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.DispatchCompute] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                DispatchComputeCommand.Run(ref GetCommand<DispatchComputeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.Draw] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                DrawCommand.Run(ref GetCommand<DrawCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.DrawIndexed] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                DrawIndexedCommand.Run(ref GetCommand<DrawIndexedCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.DrawTexture] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                DrawTextureCommand.Run(ref GetCommand<DrawTextureCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.EndHostConditionalRendering] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                EndHostConditionalRenderingCommand.Run(renderer);
            _lookup[(int)CommandType.EndTransformFeedback] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                EndTransformFeedbackCommand.Run(ref GetCommand<EndTransformFeedbackCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.MultiDrawIndirectCount] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                MultiDrawIndirectCountCommand.Run(ref GetCommand<MultiDrawIndirectCountCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.MultiDrawIndexedIndirectCount] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                MultiDrawIndexedIndirectCountCommand.Run(ref GetCommand<MultiDrawIndexedIndirectCountCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetAlphaTest] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetAlphaTestCommand.Run(ref GetCommand<SetAlphaTestCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetBlendState] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetBlendStateCommand.Run(ref GetCommand<SetBlendStateCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetDepthBias] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetDepthBiasCommand.Run(ref GetCommand<SetDepthBiasCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetDepthClamp] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetDepthClampCommand.Run(ref GetCommand<SetDepthClampCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetDepthMode] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetDepthModeCommand.Run(ref GetCommand<SetDepthModeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetDepthTest] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetDepthTestCommand.Run(ref GetCommand<SetDepthTestCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetFaceCulling] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetFaceCullingCommand.Run(ref GetCommand<SetFaceCullingCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetFrontFace] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetFrontFaceCommand.Run(ref GetCommand<SetFrontFaceCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetStorageBuffers] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetStorageBuffersCommand.Run(ref GetCommand<SetStorageBuffersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetTransformFeedbackBuffers] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetTransformFeedbackBuffersCommand.Run(ref GetCommand<SetTransformFeedbackBuffersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetUniformBuffers] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetUniformBuffersCommand.Run(ref GetCommand<SetUniformBuffersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetImage] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetImageCommand.Run(ref GetCommand<SetImageCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetIndexBuffer] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetIndexBufferCommand.Run(ref GetCommand<SetIndexBufferCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetLineParameters] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetLineParametersCommand.Run(ref GetCommand<SetLineParametersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetLogicOpState] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetLogicOpStateCommand.Run(ref GetCommand<SetLogicOpStateCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetPatchParameters] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetPatchParametersCommand.Run(ref GetCommand<SetPatchParametersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetPointParameters] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetPointParametersCommand.Run(ref GetCommand<SetPointParametersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetPolygonMode] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetPolygonModeCommand.Run(ref GetCommand<SetPolygonModeCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetPrimitiveRestart] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetPrimitiveRestartCommand.Run(ref GetCommand<SetPrimitiveRestartCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetPrimitiveTopology] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetPrimitiveTopologyCommand.Run(ref GetCommand<SetPrimitiveTopologyCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetProgram] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetProgramCommand.Run(ref GetCommand<SetProgramCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetRasterizerDiscard] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetRasterizerDiscardCommand.Run(ref GetCommand<SetRasterizerDiscardCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetRenderTargetColorMasks] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetRenderTargetColorMasksCommand.Run(ref GetCommand<SetRenderTargetColorMasksCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetRenderTargetScale] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetRenderTargetScaleCommand.Run(ref GetCommand<SetRenderTargetScaleCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetRenderTargets] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetRenderTargetsCommand.Run(ref GetCommand<SetRenderTargetsCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetSampler] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetSamplerCommand.Run(ref GetCommand<SetSamplerCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetScissor] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetScissorCommand.Run(ref GetCommand<SetScissorCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetStencilTest] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetStencilTestCommand.Run(ref GetCommand<SetStencilTestCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetTexture] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetTextureCommand.Run(ref GetCommand<SetTextureCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetUserClipDistance] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetUserClipDistanceCommand.Run(ref GetCommand<SetUserClipDistanceCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetVertexAttribs] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetVertexAttribsCommand.Run(ref GetCommand<SetVertexAttribsCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetVertexBuffers] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetVertexBuffersCommand.Run(ref GetCommand<SetVertexBuffersCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.SetViewports] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                SetViewportsCommand.Run(ref GetCommand<SetViewportsCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureBarrier] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureBarrierCommand.Run(ref GetCommand<TextureBarrierCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TextureBarrierTiled] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TextureBarrierTiledCommand.Run(ref GetCommand<TextureBarrierTiledCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TryHostConditionalRendering] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TryHostConditionalRenderingCommand.Run(ref GetCommand<TryHostConditionalRenderingCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.TryHostConditionalRenderingFlush] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                TryHostConditionalRenderingFlushCommand.Run(ref GetCommand<TryHostConditionalRenderingFlushCommand>(memory), threaded, renderer);
            _lookup[(int)CommandType.UpdateRenderScale] = (Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer) =>
                UpdateRenderScaleCommand.Run(ref GetCommand<UpdateRenderScaleCommand>(memory), threaded, renderer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunCommand(Span<byte> memory, ThreadedRenderer threaded, IRenderer renderer)
        {
            _lookup[memory[memory.Length - 1]](memory, threaded, renderer);
        }
    }
}
