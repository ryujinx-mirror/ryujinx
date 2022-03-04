using Ryujinx.Common;
using Ryujinx.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static SDL2.SDL;

namespace Ryujinx.SDL2.Common
{
    public class SDL2Driver : IDisposable
    {
        private static SDL2Driver _instance;

        public static bool IsInitialized => _instance != null;

        public static SDL2Driver Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SDL2Driver();
                }

                return _instance;
            }
        }

        private const uint SdlInitFlags = SDL_INIT_EVENTS | SDL_INIT_GAMECONTROLLER | SDL_INIT_JOYSTICK | SDL_INIT_AUDIO | SDL_INIT_VIDEO;

        private bool _isRunning;
        private uint _refereceCount;
        private Thread _worker;

        public event Action<int, int> OnJoyStickConnected;
        public event Action<int> OnJoystickDisconnected;

        private ConcurrentDictionary<uint, Action<SDL_Event>> _registeredWindowHandlers;

        private object _lock = new object();

        private SDL2Driver() {}

        public void Initialize()
        {
            lock (_lock)
            {
                _refereceCount++;

                if (_isRunning)
                {
                    return;
                }

                SDL_SetHint(SDL_HINT_JOYSTICK_HIDAPI_PS4_RUMBLE, "1");
                SDL_SetHint(SDL_HINT_JOYSTICK_HIDAPI_PS5_RUMBLE, "1");
                SDL_SetHint(SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS, "1");
                SDL_SetHint(SDL_HINT_JOYSTICK_HIDAPI_SWITCH_HOME_LED, "0");
                SDL_SetHint(SDL_HINT_JOYSTICK_HIDAPI_JOY_CONS, "1");

                if (SDL_Init(SdlInitFlags) != 0)
                {
                    string errorMessage = $"SDL2 initlaization failed with error \"{SDL_GetError()}\"";

                    Logger.Error?.Print(LogClass.Application, errorMessage);

                    throw new Exception(errorMessage);
                }

                // First ensure that we only enable joystick events (for connected/disconnected).
                SDL_GameControllerEventState(SDL_DISABLE);
                SDL_JoystickEventState(SDL_ENABLE);

                // Disable all joysticks information, we don't need them no need to flood the event queue for that.
                SDL_EventState(SDL_EventType.SDL_JOYAXISMOTION, SDL_DISABLE);
                SDL_EventState(SDL_EventType.SDL_JOYBALLMOTION, SDL_DISABLE);
                SDL_EventState(SDL_EventType.SDL_JOYHATMOTION, SDL_DISABLE);
                SDL_EventState(SDL_EventType.SDL_JOYBUTTONDOWN, SDL_DISABLE);
                SDL_EventState(SDL_EventType.SDL_JOYBUTTONUP, SDL_DISABLE);

                SDL_EventState(SDL_EventType.SDL_CONTROLLERSENSORUPDATE, SDL_DISABLE);

                string gamepadDbPath = Path.Combine(ReleaseInformations.GetBaseApplicationDirectory(), "SDL_GameControllerDB.txt");

                if (File.Exists(gamepadDbPath))
                {
                    SDL_GameControllerAddMappingsFromFile(gamepadDbPath);
                }

                _registeredWindowHandlers = new ConcurrentDictionary<uint, Action<SDL_Event>>();
                _worker = new Thread(EventWorker);
                _isRunning = true;
                _worker.Start();
            }
        }

        public bool RegisterWindow(uint windowId, Action<SDL_Event> windowEventHandler)
        {
            return _registeredWindowHandlers.TryAdd(windowId, windowEventHandler);
        }

        public void UnregisterWindow(uint windowId)
        {
            _registeredWindowHandlers.Remove(windowId, out _);
        }

        private void HandleSDLEvent(ref SDL_Event evnt)
        {
            if (evnt.type == SDL_EventType.SDL_JOYDEVICEADDED)
            {
                int deviceId = evnt.cbutton.which;

                // SDL2 loves to be inconsistent here by providing the device id instead of the instance id (like on removed event), as such we just grab it and send it inside our system.
                int instanceId = SDL_JoystickGetDeviceInstanceID(deviceId);

                if (instanceId == -1)
                {
                    return;
                }

                Logger.Debug?.Print(LogClass.Application, $"Added joystick instance id {instanceId}");

                OnJoyStickConnected?.Invoke(deviceId, instanceId);
            }
            else if (evnt.type == SDL_EventType.SDL_JOYDEVICEREMOVED)
            {
                Logger.Debug?.Print(LogClass.Application, $"Removed joystick instance id {evnt.cbutton.which}");

                OnJoystickDisconnected?.Invoke(evnt.cbutton.which);
            }
            else if (evnt.type == SDL_EventType.SDL_WINDOWEVENT || evnt.type == SDL_EventType.SDL_MOUSEBUTTONDOWN || evnt.type == SDL_EventType.SDL_MOUSEBUTTONUP)
            {
                if (_registeredWindowHandlers.TryGetValue(evnt.window.windowID, out Action<SDL_Event> handler))
                {
                    handler(evnt);
                }
            }
        }

        private void EventWorker()
        {
            const int WaitTimeMs = 10;

            using ManualResetEventSlim waitHandle = new ManualResetEventSlim(false);

            while (_isRunning)
            {
                while (SDL_PollEvent(out SDL_Event evnt) != 0)
                {
                    HandleSDLEvent(ref evnt);
                }

                waitHandle.Wait(WaitTimeMs);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            lock (_lock)
            {
                if (_isRunning)
                {
                    _refereceCount--;

                    if (_refereceCount == 0)
                    {
                        _isRunning = false;

                        _worker?.Join();

                        SDL_Quit();

                        OnJoyStickConnected = null;
                        OnJoystickDisconnected = null;
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
