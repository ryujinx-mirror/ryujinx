using Ryujinx.Audio;
using Ryujinx.Audio.Input;
using Ryujinx.Audio.Integration;
using Ryujinx.Audio.Output;
using Ryujinx.Audio.Renderer.Device;
using Ryujinx.Audio.Renderer.Server;
using Ryujinx.Cpu;
using Ryujinx.Horizon.Sdk.Audio;
using System;

namespace Ryujinx.Horizon.Audio
{
    class AudioManagers : IDisposable
    {
        public AudioManager AudioManager { get; }
        public AudioOutputManager AudioOutputManager { get; }
        public AudioInputManager AudioInputManager { get; }
        public AudioRendererManager AudioRendererManager { get; }
        public VirtualDeviceSessionRegistry AudioDeviceSessionRegistry { get; }

        public AudioManagers(IHardwareDeviceDriver audioDeviceDriver, ITickSource tickSource)
        {
            AudioManager = new AudioManager();
            AudioOutputManager = new AudioOutputManager();
            AudioInputManager = new AudioInputManager();
            AudioRendererManager = new AudioRendererManager(tickSource);
            AudioDeviceSessionRegistry = new VirtualDeviceSessionRegistry(audioDeviceDriver);

            IWritableEvent[] audioOutputRegisterBufferEvents = new IWritableEvent[Constants.AudioOutSessionCountMax];

            for (int i = 0; i < audioOutputRegisterBufferEvents.Length; i++)
            {
                audioOutputRegisterBufferEvents[i] = new AudioEvent();
            }

            AudioOutputManager.Initialize(audioDeviceDriver, audioOutputRegisterBufferEvents);

            IWritableEvent[] audioInputRegisterBufferEvents = new IWritableEvent[Constants.AudioInSessionCountMax];

            for (int i = 0; i < audioInputRegisterBufferEvents.Length; i++)
            {
                audioInputRegisterBufferEvents[i] = new AudioEvent();
            }

            AudioInputManager.Initialize(audioDeviceDriver, audioInputRegisterBufferEvents);

            IWritableEvent[] systemEvents = new IWritableEvent[Constants.AudioRendererSessionCountMax];

            for (int i = 0; i < systemEvents.Length; i++)
            {
                systemEvents[i] = new AudioEvent();
            }

            AudioManager.Initialize(audioDeviceDriver.GetUpdateRequiredEvent(), AudioOutputManager.Update, AudioInputManager.Update);

            AudioRendererManager.Initialize(systemEvents, audioDeviceDriver);

            AudioManager.Start();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                AudioManager.Dispose();
                AudioOutputManager.Dispose();
                AudioInputManager.Dispose();
                AudioRendererManager.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
