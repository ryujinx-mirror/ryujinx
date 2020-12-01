using Ryujinx.Audio.Renderer.Device;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Server;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Services.Audio.AudioRenderer;

using AudioRendererManagerImpl = Ryujinx.Audio.Renderer.Server.AudioRendererManager;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    class AudioRendererManager : IAudioRendererManager
    {
        private AudioRendererManagerImpl _impl;
        private VirtualDeviceSessionRegistry _registry;

        public AudioRendererManager(AudioRendererManagerImpl impl, VirtualDeviceSessionRegistry registry)
        {
            _impl = impl;
            _registry = registry;
        }

        public ResultCode GetAudioDeviceServiceWithRevisionInfo(ServiceCtx context, out IAudioDevice outObject, int revision, ulong appletResourceUserId)
        {
            outObject = new AudioDevice(_registry, context.Device.System.KernelContext, appletResourceUserId, revision);

            return ResultCode.Success;
        }

        public ulong GetWorkBufferSize(ref AudioRendererConfiguration parameter)
        {
            return AudioRendererManagerImpl.GetWorkBufferSize(ref parameter);
        }

        public ResultCode OpenAudioRenderer(ServiceCtx context, out IAudioRenderer obj, ref AudioRendererConfiguration parameter, ulong workBufferSize, ulong appletResourceUserId, KTransferMemory workBufferTransferMemory, uint processHandle)
        {
            var memoryManager = context.Process.HandleTable.GetKProcess((int)processHandle).CpuMemory;

            ResultCode result = (ResultCode)_impl.OpenAudioRenderer(out AudioRenderSystem renderer, memoryManager, ref parameter, appletResourceUserId, workBufferTransferMemory.Address, workBufferTransferMemory.Size, processHandle);

            if (result == ResultCode.Success)
            {
                obj = new AudioRenderer.AudioRenderer(renderer);
            }
            else
            {
                obj = null;
            }

            return result;
        }
    }
}
