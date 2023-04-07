using Ryujinx.Audio.Common;
using Ryujinx.Audio.Input;
using Ryujinx.HLE.HOS.Services.Audio.AudioIn;

using AudioInManagerImpl = Ryujinx.Audio.Input.AudioInputManager;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    class AudioInManager : IAudioInManager
    {
        private AudioInManagerImpl _impl;

        public AudioInManager(AudioInManagerImpl impl)
        {
            _impl = impl;
        }

        public string[] ListAudioIns(bool filtered)
        {
            return _impl.ListAudioIns(filtered);
        }

        public ResultCode OpenAudioIn(ServiceCtx context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioIn obj, string inputDeviceName, ref AudioInputConfiguration parameter, ulong appletResourceUserId, uint processHandle)
        {
            var memoryManager = context.Process.HandleTable.GetKProcess((int)processHandle).CpuMemory;

            ResultCode result = (ResultCode)_impl.OpenAudioIn(out outputDeviceName, out outputConfiguration, out AudioInputSystem inSystem, memoryManager, inputDeviceName, SampleFormat.PcmInt16, ref parameter, appletResourceUserId, processHandle);

            if (result == ResultCode.Success)
            {
                obj = new AudioIn.AudioIn(inSystem, context.Device.System.KernelContext, processHandle);
            }
            else
            {
                obj = null;
            }

            return result;
        }
    }
}
