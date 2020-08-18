using Ryujinx.Audio.Renderer.Device;
using Ryujinx.Audio.Renderer.Server;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRenderer
{
    class AudioDevice : IAudioDevice
    {
        private VirtualDeviceSession[] _sessions;
        private ulong _appletResourceId;
        private int _revision;
        private bool _isUsbDeviceSupported;

        private VirtualDeviceSessionRegistry _registry;
        private KEvent _systemEvent;

        public AudioDevice(VirtualDeviceSessionRegistry registry, KernelContext context, ulong appletResourceId, int revision)
        {
            _registry = registry;
            _appletResourceId = appletResourceId;
            _revision = revision;

            BehaviourContext behaviourContext = new BehaviourContext();
            behaviourContext.SetUserRevision(revision);

            _isUsbDeviceSupported = behaviourContext.IsAudioUsbDeviceOutputSupported();
            _sessions = _registry.GetSessionByAppletResourceId(appletResourceId);

            // TODO: support the 3 different events correctly when we will have hot plugable audio devices.
            _systemEvent = new KEvent(context);
            _systemEvent.ReadableEvent.Signal();
        }

        private bool TryGetDeviceByName(out VirtualDeviceSession result, string name, bool ignoreRevLimitation = false)
        {
            result = null;

            foreach (VirtualDeviceSession session in _sessions)
            {
                if (session.Device.Name.Equals(name))
                {
                    if (!ignoreRevLimitation && !_isUsbDeviceSupported && session.Device.IsUsbDevice())
                    {
                        return false;
                    }

                    result = session;

                    return true;
                }
            }

            return false;
        }

        public string GetActiveAudioDeviceName()
        {
            VirtualDevice device = _registry.ActiveDevice;

            if (!_isUsbDeviceSupported && device.IsUsbDevice())
            {
                device = _registry.DefaultDevice;
            }

            return device.Name;
        }

        public uint GetActiveChannelCount()
        {
            VirtualDevice device = _registry.ActiveDevice;

            if (!_isUsbDeviceSupported && device.IsUsbDevice())
            {
                device = _registry.DefaultDevice;
            }

            return device.ChannelCount;
        }

        public ResultCode GetAudioDeviceOutputVolume(string name, out float volume)
        {
            if (TryGetDeviceByName(out VirtualDeviceSession result, name))
            {
                volume = result.Volume;
            }
            else
            {
                volume = 0.0f;
            }

            return ResultCode.Success;
        }

        public ResultCode SetAudioDeviceOutputVolume(string name, float volume)
        {
            if (TryGetDeviceByName(out VirtualDeviceSession result, name, true))
            {
                if (!_isUsbDeviceSupported && result.Device.IsUsbDevice())
                {
                    result = _sessions[0];
                }

                result.Volume = volume;
            }

            return ResultCode.Success;
        }

        public ResultCode GetAudioSystemMasterVolumeSetting(string name, out float systemMasterVolume)
        {
            if (TryGetDeviceByName(out VirtualDeviceSession result, name, true))
            {
                systemMasterVolume = result.Device.MasterVolume;
            }
            else
            {
                systemMasterVolume = 0.0f;
            }

            return ResultCode.Success;
        }

        public string[] ListAudioDeviceName()
        {
            int deviceCount = _sessions.Length;

            if (!_isUsbDeviceSupported)
            {
                deviceCount--;
            }

            string[] result = new string[deviceCount];

            for (int i = 0; i < deviceCount; i++)
            {
                result[i] = _sessions[i].Device.Name;
            }

            return result;
        }

        public KEvent QueryAudioDeviceInputEvent()
        {
            return _systemEvent;
        }

        public KEvent QueryAudioDeviceOutputEvent()
        {
            return _systemEvent;
        }

        public KEvent QueryAudioDeviceSystemEvent()
        {
            return _systemEvent;
        }
    }
}
