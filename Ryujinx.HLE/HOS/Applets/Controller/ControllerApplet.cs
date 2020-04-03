using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;

using static Ryujinx.HLE.HOS.Services.Hid.HidServer.HidUtils;

namespace Ryujinx.HLE.HOS.Applets
{
    internal class ControllerApplet : IApplet
    {
        private Horizon _system;

        private AppletSession _normalSession;

        public event EventHandler AppletStateChanged;

        public ControllerApplet(Horizon system)
        {
            _system = system;
        }

        unsafe public ResultCode Start(AppletSession normalSession,
                                       AppletSession interactiveSession)
        {
            _normalSession = normalSession;

            byte[] launchParams = _normalSession.Pop();
            byte[] controllerSupportArgPrivate = _normalSession.Pop();
            ControllerSupportArgPrivate privateArg = IApplet.ReadStruct<ControllerSupportArgPrivate>(controllerSupportArgPrivate);

            Logger.PrintStub(LogClass.ServiceHid, $"ControllerApplet ArgPriv {privateArg.PrivateSize} {privateArg.ArgSize} {privateArg.Mode}" +
                        $"HoldType:{(NpadJoyHoldType)privateArg.NpadJoyHoldType} StyleSets:{(ControllerType)privateArg.NpadStyleSet}");

            if (privateArg.Mode != ControllerSupportMode.ShowControllerSupport)
            {
                _normalSession.Push(BuildResponse()); // Dummy response for other modes
                AppletStateChanged?.Invoke(this, null);

                return ResultCode.Success;
            }

            byte[] controllerSupportArg = _normalSession.Pop();

            ControllerSupportArgHeader argHeader;

            if (privateArg.ArgSize == Marshal.SizeOf<ControllerSupportArg>())
            {
                ControllerSupportArg arg = IApplet.ReadStruct<ControllerSupportArg>(controllerSupportArg);
                argHeader = arg.Header;
                // Read enable text here?
            }
            else
            {
                Logger.PrintStub(LogClass.ServiceHid, $"Unknown revision of ControllerSupportArg.");

                argHeader = IApplet.ReadStruct<ControllerSupportArgHeader>(controllerSupportArg); // Read just the header
            }

            Logger.PrintStub(LogClass.ServiceHid, $"ControllerApplet Arg {argHeader.PlayerCountMin} {argHeader.PlayerCountMax} {argHeader.EnableTakeOverConnection} {argHeader.EnableSingleMode}");

            // Currently, the only purpose of this applet is to help 
            // choose the primary input controller for the game
            // TODO: Ideally should hook back to HID.Controller. When applet is called, can choose appropriate controller and attach to appropriate id.
            if (argHeader.PlayerCountMin > 1)
            {
                Logger.PrintWarning(LogClass.ServiceHid, "More than one controller was requested.");
            }

            ControllerSupportResultInfo result = new ControllerSupportResultInfo
            {
                PlayerCount = 1,
                SelectedId = (uint)GetNpadIdTypeFromIndex(_system.Device.Hid.Npads.PrimaryController)
            };

            Logger.PrintStub(LogClass.ServiceHid, $"ControllerApplet ReturnResult {result.PlayerCount} {result.SelectedId}");

            _normalSession.Push(BuildResponse(result));
            AppletStateChanged?.Invoke(this, null);

            return ResultCode.Success;
        }

        public ResultCode GetResult()
        {
            return ResultCode.Success;
        }

        private byte[] BuildResponse(ControllerSupportResultInfo result)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref result, Unsafe.SizeOf<ControllerSupportResultInfo>())));

                return stream.ToArray();
            }
        }

        private byte[] BuildResponse()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((ulong)ResultCode.Success);

                return stream.ToArray();
            }
        }
    }
}
