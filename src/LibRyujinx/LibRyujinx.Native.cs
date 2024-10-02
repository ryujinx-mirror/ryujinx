using LibRyujinx.Shared;
using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.OpenGL;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibRyujinx
{
    public static partial class LibRyujinx
    {
        private unsafe static IntPtr CreateStringArray(List<string> strings)
        {
            uint size = (uint)(Marshal.SizeOf<IntPtr>() * (strings.Count + 1));
            var array = (char**)Marshal.AllocHGlobal((int)size);
            Unsafe.InitBlockUnaligned(array, 0, size);

            for (int i = 0; i < strings.Count; i++)
            {
                array[i] = (char*)Marshal.StringToHGlobalAnsi(strings[i]);
            }

            return (nint)array;
        }

        [UnmanagedCallersOnly(EntryPoint = "device_initialize")]
        public static bool InitializeDeviceNative(bool isHostMapped,
                                                  bool useHypervisor,
                                                  SystemLanguage systemLanguage,
                                                  RegionCode regionCode,
                                                  bool enableVsync,
                                                  bool enableDockedMode,
                                                  bool enablePtc,
                                                  bool enableInternetAccess,
                                                  IntPtr timeZone,
                                                  bool ignoreMissingServices)
        {
            return InitializeDevice(isHostMapped,
                                    useHypervisor,
                                    systemLanguage,
                                    regionCode,
                                    enableVsync,
                                    enableDockedMode,
                                    enablePtc,
                                    enableInternetAccess,
                                    Marshal.PtrToStringAnsi(timeZone),
                                    ignoreMissingServices);
        }

        [UnmanagedCallersOnly(EntryPoint = "device_reload_file_system")]
        public static void ReloadFileSystemNative()
        {
            SwitchDevice?.ReloadFileSystem();
        }

        [UnmanagedCallersOnly(EntryPoint = "device_load")]
        public static bool LoadApplicationNative(IntPtr pathPtr)
        {
            if (SwitchDevice?.EmulationContext == null)
            {
                return false;
            }

            var path = Marshal.PtrToStringAnsi(pathPtr);

            return LoadApplication(path);
        }

        [UnmanagedCallersOnly(EntryPoint = "device_install_firmware")]
        public static void InstallFirmwareNative(int descriptor, bool isXci)
        {
            var stream = OpenFile(descriptor);

            InstallFirmware(stream, isXci);
        }

        [UnmanagedCallersOnly(EntryPoint = "device_get_installed_firmware_version")]
        public static IntPtr GetInstalledFirmwareVersionNative()
        {
            var result = GetInstalledFirmwareVersion();
            return Marshal.StringToHGlobalAnsi(result);
        }

        [UnmanagedCallersOnly(EntryPoint = "initialize")]
        public static bool InitializeNative(IntPtr basePathPtr)
        {
            var path = Marshal.PtrToStringAnsi(basePathPtr);

            var res = Initialize(path);

            InitializeAudio();

            return res;
        }

        [UnmanagedCallersOnly(EntryPoint = "graphics_initialize")]
        public static bool InitializeGraphicsNative(GraphicsConfiguration graphicsConfiguration)
        {
            if (OperatingSystem.IsIOS())
            {
                // Yes, macOS not iOS
                Silk.NET.Core.Loader.SearchPathContainer.Platform = Silk.NET.Core.Loader.UnderlyingPlatform.MacOS;
            }
            return InitializeGraphics(graphicsConfiguration);
        }

        [UnmanagedCallersOnly(EntryPoint = "graphics_initialize_renderer")]
        public unsafe static bool InitializeGraphicsRendererNative(GraphicsBackend graphicsBackend, NativeGraphicsInterop nativeGraphicsInterop)
        {
            _nativeGraphicsInterop = nativeGraphicsInterop;
            if (Renderer != null)
            {
                return false;
            }

            List<string> extensions = new List<string>();
            var size = Marshal.SizeOf<IntPtr>();
            var extPtr = (IntPtr*)nativeGraphicsInterop.VkRequiredExtensions;
            for (int i = 0; i < nativeGraphicsInterop.VkRequiredExtensionsCount; i++)
            {
                var ptr = extPtr[i];
                extensions.Add(Marshal.PtrToStringAnsi(ptr) ?? string.Empty);
            }

            CreateSurface? createSurfaceFunc = nativeGraphicsInterop.VkCreateSurface == IntPtr.Zero ? default : Marshal.GetDelegateForFunctionPointer<CreateSurface>(nativeGraphicsInterop.VkCreateSurface);

            return InitializeGraphicsRenderer(graphicsBackend, createSurfaceFunc, extensions.ToArray());
        }

        [UnmanagedCallersOnly(EntryPoint = "graphics_renderer_set_size")]
        public static void SetRendererSizeNative(int width, int height)
        {
            SetRendererSize(width, height);
        }

        [UnmanagedCallersOnly(EntryPoint = "graphics_renderer_run_loop")]
        public static void RunLoopNative()
        {
            if (Renderer is OpenGLRenderer)
            {
                var proc = Marshal.GetDelegateForFunctionPointer<GetProcAddress>(_nativeGraphicsInterop.GlGetProcAddress);
                GL.LoadBindings(new OpenTKBindingsContext(x => proc!.Invoke(x)));
            }
            RunLoop();
        }

        [UnmanagedCallersOnly(EntryPoint = "graphics_renderer_set_vsync")]
        public static void SetVsyncStateNative(bool enabled)
        {
            SetVsyncState(enabled);
        }

        [UnmanagedCallersOnly(EntryPoint = "graphics_renderer_set_swap_buffer_callback")]
        public static void SetSwapBuffersCallbackNative(IntPtr swapBuffersCallback)
        {
            _swapBuffersCallback = Marshal.GetDelegateForFunctionPointer<SwapBuffersCallback>(swapBuffersCallback);
        }

        [UnmanagedCallersOnly(EntryPoint = "get_game_info")]
        public static GameInfoNative GetGameInfoNative(int descriptor, IntPtr extensionPtr)
        {
            var extension = Marshal.PtrToStringAnsi(extensionPtr);
            var stream = OpenFile(descriptor);

            var gameInfo = GetGameInfo(stream, extension ?? "");

            return gameInfo == null ? default : new GameInfoNative(gameInfo.FileSize, gameInfo.TitleName, gameInfo.TitleId, gameInfo.Developer, gameInfo.Version, gameInfo.Icon);
        }

        [UnmanagedCallersOnly(EntryPoint = "input_initialize")]
        public static void InitializeInputNative(int width, int height)
        {
            InitializeInput(width, height);
        }

        [UnmanagedCallersOnly(EntryPoint = "input_set_client_size")]
        public static void SetClientSizeNative(int width, int height)
        {
            SetClientSize(width, height);
        }

        [UnmanagedCallersOnly(EntryPoint = "input_set_touch_point")]
        public static void SetTouchPointNative(int x, int y)
        {
            SetTouchPoint(x, y);
        }

        [UnmanagedCallersOnly(EntryPoint = "input_release_touch_point")]
        public static void ReleaseTouchPointNative()
        {
            ReleaseTouchPoint();
        }

        [UnmanagedCallersOnly(EntryPoint = "input_update")]
        public static void UpdateInputNative()
        {
            UpdateInput();
        }

        [UnmanagedCallersOnly(EntryPoint = "input_set_button_pressed")]
        public static void SetButtonPressedNative(GamepadButtonInputId button, int id)
        {
            SetButtonPressed(button, id);
        }

        [UnmanagedCallersOnly(EntryPoint = "input_set_button_released")]
        public static void SetButtonReleasedNative(GamepadButtonInputId button, int id)
        {
            SetButtonReleased(button, id);
        }

        [UnmanagedCallersOnly(EntryPoint = "input_set_accelerometer_data")]
        public static void SetAccelerometerDataNative(Vector3 accel, int id)
        {
            SetAccelerometerData(accel, id);
        }

        [UnmanagedCallersOnly(EntryPoint = "input_set_gyro_data")]
        public static void SetGryoDataNative(Vector3 gyro, int id)
        {
            SetGryoData(gyro, id);
        }

        [UnmanagedCallersOnly(EntryPoint = "input_set_stick_axis")]
        public static void SetStickAxisNative(StickInputId stick, Vector2 axes, int id)
        {
            SetStickAxis(stick, axes, id);
        }

        [UnmanagedCallersOnly(EntryPoint = "input_connect_gamepad")]
        public static IntPtr ConnectGamepadNative(int index)
        {
            return ConnectGamepad(index);
        }

        [UnmanagedCallersOnly(EntryPoint = "device_get_game_fifo")]
        public static double GetGameInfoNative()
        {
            var stats = SwitchDevice?.EmulationContext?.Statistics.GetFifoPercent() ?? 0;

            return stats;
        }

        [UnmanagedCallersOnly(EntryPoint = "device_get_game_frame_time")]
        public static double GetGameFrameTimeNative()
        {
            var stats = SwitchDevice?.EmulationContext?.Statistics.GetGameFrameTime() ?? 0;

            return stats;
        }

        [UnmanagedCallersOnly(EntryPoint = "device_get_game_frame_rate")]
        public static double GetGameFrameRateNative()
        {
            var stats = SwitchDevice?.EmulationContext?.Statistics.GetGameFrameRate() ?? 0;

            return stats;
        }
        [UnmanagedCallersOnly(EntryPoint = "device_launch_mii_editor")]
        public static bool LaunchMiiEditAppletNative()
        {
            if (SwitchDevice?.EmulationContext == null)
            {
                return false;
            }

            return LaunchMiiEditApplet();
        }

        [UnmanagedCallersOnly(EntryPoint = "device_get_dlc_content_list")]
        public static IntPtr GetDlcContentListNative(IntPtr pathPtr, long titleId)
        {
            var list = GetDlcContentList(Marshal.PtrToStringAnsi(pathPtr) ?? "", (ulong)titleId);

            return CreateStringArray(list);
        }

        [UnmanagedCallersOnly(EntryPoint = "device_get_dlc_title_id")]
        public static long GetDlcTitleIdNative(IntPtr pathPtr, IntPtr ncaPath)
        {
            return Marshal.StringToHGlobalAnsi(GetDlcTitleId(Marshal.PtrToStringAnsi(pathPtr) ?? "", Marshal.PtrToStringAnsi(ncaPath) ?? ""));
        }

        [UnmanagedCallersOnly(EntryPoint = "device_signal_emulation_close")]
        public static void SignalEmulationCloseNative()
        {
            SignalEmulationClose();
        }

        [UnmanagedCallersOnly(EntryPoint = "device_close_emulation")]
        public static void CloseEmulationNative()
        {
            CloseEmulation();
        }

        [UnmanagedCallersOnly(EntryPoint = "device_load_descriptor")]
        public static bool LoadApplicationNative(int descriptor, int type, int updateDescriptor)
        {
            if (SwitchDevice?.EmulationContext == null)
            {
                return false;
            }

            var stream = OpenFile(descriptor);
            var update = updateDescriptor == -1 ? null : OpenFile(updateDescriptor);

            return LoadApplication(stream, (FileType)type, update);
        }

        [UnmanagedCallersOnly(EntryPoint = "device_verify_firmware")]
        public static IntPtr VerifyFirmwareNative(int descriptor, bool isXci)
        {
            var stream = OpenFile(descriptor);

            IntPtr stringHandle = 0;
            string? version = "0.0";

            try
            {
                version = VerifyFirmware(stream, isXci)?.VersionString;
            }
            catch (Exception _)
            {
                Logger.Error?.Print(LogClass.Service, $"Unable to verify firmware. Exception: {_}");
            }

            if (version != null)
            {
                stringHandle = Marshal.StringToHGlobalAnsi(version);
            }

            return stringHandle;
        }

        [UnmanagedCallersOnly(EntryPoint = "logging_set_enabled")]
        public static void SetLoggingEnabledNative(int logLevel, bool enabled)
        {
            Logger.SetEnable((LogLevel)logLevel, enabled);
        }

        [UnmanagedCallersOnly(EntryPoint = "logging_enabled_graphics_log")]
        public static void SetLoggingEnabledGraphicsLogNative(bool enabled)
        {
            _enableGraphicsLogging = enabled;
        }

        [UnmanagedCallersOnly(EntryPoint = "device_get_game_info")]
        public unsafe static void GetGameInfoNative(int fileDescriptor, IntPtr extension, IntPtr infoPtr)
        {
            using var stream = OpenFile(fileDescriptor);
            var ext = Marshal.PtrToStringAnsi(extension);
            var info = GetGameInfo(stream, ext.ToLower()) ?? GetDefaultInfo(stream);
            var i = (GameInfoNative*)infoPtr;
            var n = new GameInfoNative(info);
            i->TitleId = n.TitleId;
            i->TitleName = n.TitleName;
            i->Version = n.Version;
            i->FileSize = n.FileSize;
            i->Icon = n.Icon;
            i->Version = n.Version;
            i->Developer = n.Developer;
        }

        [UnmanagedCallersOnly(EntryPoint = "user_get_opened_user")]
        public static IntPtr GetOpenedUserNative()
        {
            var userId = GetOpenedUser();
            var ptr = Marshal.StringToHGlobalAnsi(userId);

            return ptr;
        }

        [UnmanagedCallersOnly(EntryPoint = "user_get_user_picture")]
        public static IntPtr GetUserPictureNative(IntPtr userIdPtr)
        {
            var userId = Marshal.PtrToStringAnsi(userIdPtr) ?? "";

            return Marshal.StringToHGlobalAnsi(GetUserPicture(userId));
        }

        [UnmanagedCallersOnly(EntryPoint = "user_set_user_picture")]
        public static void SetUserPictureNative(IntPtr userIdPtr, IntPtr picturePtr)
        {
            var userId = Marshal.PtrToStringAnsi(userIdPtr) ?? "";
            var picture = Marshal.PtrToStringAnsi(picturePtr) ?? "";

            SetUserPicture(userId, picture);
        }

        [UnmanagedCallersOnly(EntryPoint = "user_get_user_name")]
        public static IntPtr GetUserNameNative(IntPtr userIdPtr)
        {
            var userId = Marshal.PtrToStringAnsi(userIdPtr) ?? "";

            return Marshal.StringToHGlobalAnsi(GetUserName(userId));
        }

        [UnmanagedCallersOnly(EntryPoint = "user_set_user_name")]
        public static void SetUserNameNative(IntPtr userIdPtr, IntPtr userNamePtr)
        {
            var userId = Marshal.PtrToStringAnsi(userIdPtr) ?? "";
            var userName = Marshal.PtrToStringAnsi(userNamePtr) ?? "";

            SetUserName(userId, userName);
        }

        [UnmanagedCallersOnly(EntryPoint = "user_get_all_users")]
        public static IntPtr GetAllUsersNative()
        {
            var users = GetAllUsers();

            return CreateStringArray(users.ToList());
        }

        [UnmanagedCallersOnly(EntryPoint = "user_add_user")]
        public static void AddUserNative(IntPtr userNamePtr, IntPtr picturePtr)
        {
            var userName = Marshal.PtrToStringAnsi(userNamePtr) ?? "";
            var picture = Marshal.PtrToStringAnsi(picturePtr) ?? "";

            AddUser(userName, picture);
        }

        [UnmanagedCallersOnly(EntryPoint = "user_delete_user")]
        public static void DeleteUserNative(IntPtr userIdPtr)
        {
            var userId = Marshal.PtrToStringAnsi(userIdPtr) ?? "";

            DeleteUser(userId);
        }

        [UnmanagedCallersOnly(EntryPoint = "user_open_user")]
        public static void OpenUserNative(IntPtr userIdPtr)
        {
            var userId = Marshal.PtrToStringAnsi(userIdPtr) ?? "";

            OpenUser(userId);
        }

        [UnmanagedCallersOnly(EntryPoint = "user_close_user")]
        public static void CloseUserNative(IntPtr userIdPtr)
        {
            var userId = Marshal.PtrToStringAnsi(userIdPtr) ?? "";

            CloseUser(userId);
        }
    }
}
