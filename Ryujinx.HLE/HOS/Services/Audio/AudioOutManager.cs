using Ryujinx.Audio.Common;
using Ryujinx.Audio.Output;
using Ryujinx.HLE.HOS.Services.Audio.AudioOut;

using AudioOutManagerImpl = Ryujinx.Audio.Output.AudioOutputManager;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    class AudioOutManager : IAudioOutManager
    {
        private AudioOutManagerImpl _impl;

        public AudioOutManager(AudioOutManagerImpl impl)
        {
            _impl = impl;
        }

        public string[] ListAudioOuts()
        {
            return _impl.ListAudioOuts();
        }

        public ResultCode OpenAudioOut(ServiceCtx context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioOut obj, string inputDeviceName, ref AudioInputConfiguration parameter, ulong appletResourceUserId, uint processHandle)
        {
            var memoryManager = context.Process.HandleTable.GetKProcess((int)processHandle).CpuMemory;

            ResultCode result = (ResultCode)_impl.OpenAudioOut(out outputDeviceName, out outputConfiguration, out AudioOutputSystem outSystem, memoryManager, inputDeviceName, SampleFormat.PcmInt16, ref parameter, appletResourceUserId, processHandle);

            if (result == ResultCode.Success)
            {
                obj = new AudioOut.AudioOut(outSystem, context.Device.System.KernelContext, processHandle);
            }
            else
            {
                obj = null;
            }

            return result;
        }
    }
}
