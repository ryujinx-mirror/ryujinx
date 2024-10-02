using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Win32.SafeHandles;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.SystemState;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LibRyujinx
{
    public static partial class LibRyujinx
    {
        public static bool InitializeDevice(bool isHostMapped,
                                            bool useHypervisor,
                                            SystemLanguage systemLanguage,
                                            RegionCode regionCode,
                                            bool enableVsync,
                                            bool enableDockedMode,
                                            bool enablePtc,
                                            bool enableInternetAccess,
                                            string? timeZone,
                                            bool ignoreMissingServices)
        {
            if (SwitchDevice == null)
            {
                return false;
            }

            return SwitchDevice.InitializeContext(isHostMapped,
                                                  useHypervisor,
                                                  systemLanguage,
                                                  regionCode,
                                                  enableVsync,
                                                  enableDockedMode,
                                                  enablePtc,
                                                  enableInternetAccess,
                                                  timeZone,
                                                  ignoreMissingServices);
        }

        public static void InstallFirmware(Stream stream, bool isXci)
        {
            SwitchDevice?.ContentManager.InstallFirmware(stream, isXci);
        }

        public static string GetInstalledFirmwareVersion()
        {
            var version = SwitchDevice?.ContentManager.GetCurrentFirmwareVersion();

            if (version != null)
            {
                return version.VersionString;
            }

            return String.Empty;
        }

        public static SystemVersion? VerifyFirmware(Stream stream, bool isXci)
        {
            return SwitchDevice?.ContentManager?.VerifyFirmwarePackage(stream, isXci) ?? null;
        }

        public static bool LoadApplication(Stream stream, FileType type, Stream? updateStream = null)
        {
            var emulationContext = SwitchDevice.EmulationContext;
            return type switch
            {
                FileType.None => false,
                FileType.Nsp => emulationContext?.LoadNsp(stream, 0, updateStream) ?? false,
                FileType.Xci => emulationContext?.LoadXci(stream, 0, updateStream) ?? false,
                FileType.Nro => emulationContext?.LoadProgram(stream, true, "") ?? false,
            };
        }

        public static bool LaunchMiiEditApplet()
        {
            string contentPath = SwitchDevice.ContentManager.GetInstalledContentPath(0x0100000000001009, StorageId.BuiltInSystem, NcaContentType.Program);

            return LoadApplication(contentPath);
        }

        public static bool LoadApplication(string? path)
        {
            var emulationContext = SwitchDevice.EmulationContext;

            if (Directory.Exists(path))
            {
                string[] romFsFiles = Directory.GetFiles(path, "*.istorage");

                if (romFsFiles.Length == 0)
                {
                    romFsFiles = Directory.GetFiles(path, "*.romfs");
                }

                if (romFsFiles.Length > 0)
                {
                    Logger.Info?.Print(LogClass.Application, "Loading as cart with RomFS.");

                    if (!emulationContext.LoadCart(path, romFsFiles[0]))
                    {
                        SwitchDevice.DisposeContext();

                        return false;
                    }
                }
                else
                {
                    Logger.Info?.Print(LogClass.Application, "Loading as cart WITHOUT RomFS.");

                    if (!emulationContext.LoadCart(path))
                    {
                        SwitchDevice.DisposeContext();

                        return false;
                    }
                }
            }
            else if (File.Exists(path))
            {
                switch (Path.GetExtension(path).ToLowerInvariant())
                {
                    case ".xci":
                        Logger.Info?.Print(LogClass.Application, "Loading as XCI.");

                        if (!emulationContext.LoadXci(path))
                        {
                            SwitchDevice.DisposeContext();

                            return false;
                        }
                        break;
                    case ".nca":
                        Logger.Info?.Print(LogClass.Application, "Loading as NCA.");

                        if (!emulationContext.LoadNca(path))
                        {
                            SwitchDevice.DisposeContext();

                            return false;
                        }
                        break;
                    case ".nsp":
                    case ".pfs0":
                        Logger.Info?.Print(LogClass.Application, "Loading as NSP.");

                        if (!emulationContext.LoadNsp(path))
                        {
                            SwitchDevice.DisposeContext();

                            return false;
                        }
                        break;
                    default:
                        Logger.Info?.Print(LogClass.Application, "Loading as Homebrew.");
                        try
                        {
                            if (!emulationContext.LoadProgram(path))
                            {
                                SwitchDevice.DisposeContext();

                                return false;
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Logger.Error?.Print(LogClass.Application, "The specified file is not supported by Ryujinx.");

                            SwitchDevice.DisposeContext();

                            return false;
                        }
                        break;
                }
            }
            else
            {
                Logger.Warning?.Print(LogClass.Application, $"Couldn't load '{path}'. Please specify a valid XCI/NCA/NSP/PFS0/NRO file.");

                SwitchDevice.DisposeContext();

                return false;
            }

            return true;
        }

        public static void SignalEmulationClose()
        {
            _isStopped = true;
            _isActive = false;
        }

        public static void CloseEmulation()
        {
            if (SwitchDevice == null)
                return;

            _npadManager?.Dispose();
            _npadManager = null;

            _touchScreenManager?.Dispose();
            _touchScreenManager = null;

            SwitchDevice!.InputManager?.Dispose();
            SwitchDevice!.InputManager = null;
            _inputManager = null;

            if (Renderer != null)
            {
                _gpuDoneEvent.WaitOne();
                _gpuDoneEvent.Dispose();
                _gpuDoneEvent = null;
                SwitchDevice?.DisposeContext();
                Renderer = null;
            }
        }

        private static FileStream OpenFile(int descriptor)
        {
            var safeHandle = new SafeFileHandle(descriptor, false);

            return new FileStream(safeHandle, FileAccess.ReadWrite);
        }

        public enum FileType
        {
            None,
            Nsp,
            Xci,
            Nro
        }
    }
}
