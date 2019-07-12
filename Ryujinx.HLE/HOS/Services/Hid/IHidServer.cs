using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.Input;
using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Service("hid")]
    class IHidServer : IpcService
    {
        private KEvent _npadStyleSetUpdateEvent;
        private KEvent _xpadIdEvent;
        private KEvent _palmaOperationCompleteEvent;

        private int _xpadIdEventHandle;

        private bool _sixAxisSensorFusionEnabled;
        private bool _unintendedHomeButtonInputProtectionEnabled;
        private bool _vibrationPermitted;
        private bool _usbFullKeyControllerEnabled;

        private HidNpadJoyHoldType            _npadJoyHoldType;
        private HidNpadStyle                  _npadStyleSet;
        private HidNpadJoyAssignmentMode      _npadJoyAssignmentMode;
        private HidNpadHandheldActivationMode _npadHandheldActivationMode;
        private HidGyroscopeZeroDriftMode     _gyroscopeZeroDriftMode;

        private long  _npadCommunicationMode;
        private uint  _accelerometerPlayMode;
        private long  _vibrationGcErmCommand;
        private float _sevenSixAxisSensorFusionStrength;

        private HidSensorFusionParameters  _sensorFusionParams;
        private HidAccelerometerParameters _accelerometerParams;
        private HidVibrationValue          _vibrationValue;

        public IHidServer(ServiceCtx context)
        {
            _npadStyleSetUpdateEvent     = new KEvent(context.Device.System);
            _xpadIdEvent                 = new KEvent(context.Device.System);
            _palmaOperationCompleteEvent = new KEvent(context.Device.System);

            _npadJoyHoldType            = HidNpadJoyHoldType.Vertical;
            _npadStyleSet               = HidNpadStyle.FullKey | HidNpadStyle.Dual | HidNpadStyle.Left | HidNpadStyle.Right | HidNpadStyle.Handheld;
            _npadJoyAssignmentMode      = HidNpadJoyAssignmentMode.Dual;
            _npadHandheldActivationMode = HidNpadHandheldActivationMode.Dual;
            _gyroscopeZeroDriftMode     = HidGyroscopeZeroDriftMode.Standard;

            _sensorFusionParams  = new HidSensorFusionParameters();
            _accelerometerParams = new HidAccelerometerParameters();
            _vibrationValue      = new HidVibrationValue();

            // TODO: signal event at right place
            _xpadIdEvent.ReadableEvent.Signal();
        }

        [Command(0)]
        // CreateAppletResource(nn::applet::AppletResourceUserId) -> object<nn::hid::IAppletResource>
        public long CreateAppletResource(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            MakeObject(context, new IAppletResource(context.Device.System.HidSharedMem));

            return 0;
        }

        [Command(1)]
        // ActivateDebugPad(nn::applet::AppletResourceUserId)
        public long ActivateDebugPad(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(11)]
        // ActivateTouchScreen(nn::applet::AppletResourceUserId)
        public long ActivateTouchScreen(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(21)]
        // ActivateMouse(nn::applet::AppletResourceUserId)
        public long ActivateMouse(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(31)]
        // ActivateKeyboard(nn::applet::AppletResourceUserId)
        public long ActivateKeyboard(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(40)]
        // AcquireXpadIdEventHandle(ulong XpadId) -> nn::sf::NativeHandle
        public long AcquireXpadIdEventHandle(ServiceCtx context)
        {
            long xpadId = context.RequestData.ReadInt64();

            if (context.Process.HandleTable.GenerateHandle(_xpadIdEvent.ReadableEvent, out _xpadIdEventHandle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_xpadIdEventHandle);

            Logger.PrintStub(LogClass.ServiceHid, new { xpadId });

            return 0;
        }

        [Command(41)]
        // ReleaseXpadIdEventHandle(ulong XpadId)
        public long ReleaseXpadIdEventHandle(ServiceCtx context)
        {
            long xpadId = context.RequestData.ReadInt64();

            context.Process.HandleTable.CloseHandle(_xpadIdEventHandle);

            Logger.PrintStub(LogClass.ServiceHid, new { xpadId });

            return 0;
        }

        [Command(51)]
        // ActivateXpad(nn::hid::BasicXpadId, nn::applet::AppletResourceUserId)
        public long ActivateXpad(ServiceCtx context)
        {
            int  basicXpadId          = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, basicXpadId });

            return 0;
        }

        [Command(55)]
        // GetXpadIds() -> long IdsCount, buffer<array<nn::hid::BasicXpadId>, type: 0xa>
        public long GetXpadIds(ServiceCtx context)
        {
            // There is any Xpad, so we return 0 and write nothing inside the type-0xa buffer.
            context.ResponseData.Write(0L);

            Logger.PrintStub(LogClass.ServiceHid);

            return 0;
        }

        [Command(56)]
        // ActivateJoyXpad(nn::hid::JoyXpadId)
        public long ActivateJoyXpad(ServiceCtx context)
        {
            int joyXpadId = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { joyXpadId });

            return 0;
        }

        [Command(58)]
        // GetJoyXpadLifoHandle(nn::hid::JoyXpadId) -> nn::sf::NativeHandle
        public long GetJoyXpadLifoHandle(ServiceCtx context)
        {
            int joyXpadId = context.RequestData.ReadInt32();

            int handle = 0;

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceHid, new { joyXpadId });

            return 0;
        }

        [Command(59)]
        // GetJoyXpadIds() -> long IdsCount, buffer<array<nn::hid::JoyXpadId>, type: 0xa>
        public long GetJoyXpadIds(ServiceCtx context)
        {
            // There is any JoyXpad, so we return 0 and write nothing inside the type-0xa buffer.
            context.ResponseData.Write(0L);

            Logger.PrintStub(LogClass.ServiceHid);

            return 0;
        }

        [Command(60)]
        // ActivateSixAxisSensor(nn::hid::BasicXpadId)
        public long ActivateSixAxisSensor(ServiceCtx context)
        {
            int basicXpadId = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { basicXpadId });

            return 0;
        }

        [Command(61)]
        // DeactivateSixAxisSensor(nn::hid::BasicXpadId)
        public long DeactivateSixAxisSensor(ServiceCtx context)
        {
            int basicXpadId = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { basicXpadId });

            return 0;
        }

        [Command(62)]
        // GetSixAxisSensorLifoHandle(nn::hid::BasicXpadId) -> nn::sf::NativeHandle
        public long GetSixAxisSensorLifoHandle(ServiceCtx context)
        {
            int basicXpadId = context.RequestData.ReadInt32();

            int handle = 0;

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceHid, new { basicXpadId });

            return 0;
        }

        [Command(63)]
        // ActivateJoySixAxisSensor(nn::hid::JoyXpadId)
        public long ActivateJoySixAxisSensor(ServiceCtx context)
        {
            int joyXpadId = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { joyXpadId });

            return 0;
        }

        [Command(64)]
        // DeactivateJoySixAxisSensor(nn::hid::JoyXpadId)
        public long DeactivateJoySixAxisSensor(ServiceCtx context)
        {
            int joyXpadId = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { joyXpadId });

            return 0;
        }

        [Command(65)]
        // GetJoySixAxisSensorLifoHandle(nn::hid::JoyXpadId) -> nn::sf::NativeHandle
        public long GetJoySixAxisSensorLifoHandle(ServiceCtx context)
        {
            int joyXpadId = context.RequestData.ReadInt32();

            int handle = 0;

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceHid, new { joyXpadId });

            return 0;
        }

        [Command(66)]
        // StartSixAxisSensor(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long StartSixAxisSensor(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle });

            return 0;
        }

        [Command(67)]
        // StopSixAxisSensor(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long StopSixAxisSensor(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle });

            return 0;
        }

        [Command(68)]
        // IsSixAxisSensorFusionEnabled(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> bool IsEnabled
        public long IsSixAxisSensorFusionEnabled(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_sixAxisSensorFusionEnabled);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _sixAxisSensorFusionEnabled });

            return 0;
        }

        [Command(69)]
        // EnableSixAxisSensorFusion(bool Enabled, nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long EnableSixAxisSensorFusion(ServiceCtx context)
        {
            _sixAxisSensorFusionEnabled = context.RequestData.ReadBoolean();
            int  sixAxisSensorHandle    = context.RequestData.ReadInt32();
            long appletResourceUserId   = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _sixAxisSensorFusionEnabled });

            return 0;
        }

        [Command(70)]
        // SetSixAxisSensorFusionParameters(nn::hid::SixAxisSensorHandle, float RevisePower, float ReviseRange, nn::applet::AppletResourceUserId)
        public long SetSixAxisSensorFusionParameters(ServiceCtx context)
        {
            int   sixAxisSensorHandle = context.RequestData.ReadInt32();

            _sensorFusionParams = new HidSensorFusionParameters
            {
                RevisePower = context.RequestData.ReadInt32(),
                ReviseRange = context.RequestData.ReadInt32()
            };

            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _sensorFusionParams.RevisePower, _sensorFusionParams.ReviseRange });

            return 0;
        }

        [Command(71)]
        // GetSixAxisSensorFusionParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> float RevisePower, float ReviseRange)
        public long GetSixAxisSensorFusionParameters(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_sensorFusionParams.RevisePower);
            context.ResponseData.Write(_sensorFusionParams.ReviseRange);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _sensorFusionParams.RevisePower, _sensorFusionParams.ReviseRange });

            return 0;
        }

        [Command(72)]
        // ResetSixAxisSensorFusionParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long ResetSixAxisSensorFusionParameters(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            _sensorFusionParams.RevisePower = 0;
            _sensorFusionParams.ReviseRange = 0;

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _sensorFusionParams.RevisePower, _sensorFusionParams.ReviseRange });

            return 0;
        }

        [Command(73)]
        // SetAccelerometerParameters(nn::hid::SixAxisSensorHandle, float X, float Y, nn::applet::AppletResourceUserId)
        public long SetAccelerometerParameters(ServiceCtx context)
        {
            int sixAxisSensorHandle = context.RequestData.ReadInt32();

            _accelerometerParams = new HidAccelerometerParameters
            {
                X = context.RequestData.ReadInt32(),
                Y = context.RequestData.ReadInt32()
            };

            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerParams.X, _accelerometerParams.Y });

            return 0;
        }

        [Command(74)]
        // GetAccelerometerParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> float X, float Y
        public long GetAccelerometerParameters(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_accelerometerParams.X);
            context.ResponseData.Write(_accelerometerParams.Y);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerParams.X, _accelerometerParams.Y });

            return 0;
        }

        [Command(75)]
        // ResetAccelerometerParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long ResetAccelerometerParameters(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            _accelerometerParams.X = 0;
            _accelerometerParams.Y = 0;

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerParams.X, _accelerometerParams.Y });

            return 0;
        }

        [Command(76)]
        // SetAccelerometerPlayMode(nn::hid::SixAxisSensorHandle, uint PlayMode, nn::applet::AppletResourceUserId)
        public long SetAccelerometerPlayMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle    = context.RequestData.ReadInt32();
                 _accelerometerPlayMode = context.RequestData.ReadUInt32();
            long appletResourceUserId   = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerPlayMode });

            return 0;
        }

        [Command(77)]
        // GetAccelerometerPlayMode(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> uint PlayMode
        public long GetAccelerometerPlayMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_accelerometerPlayMode);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerPlayMode });

            return 0;
        }

        [Command(78)]
        // ResetAccelerometerPlayMode(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long ResetAccelerometerPlayMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            _accelerometerPlayMode = 0;

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerPlayMode });

            return 0;
        }

        [Command(79)]
        // SetGyroscopeZeroDriftMode(nn::hid::SixAxisSensorHandle, uint GyroscopeZeroDriftMode, nn::applet::AppletResourceUserId)
        public long SetGyroscopeZeroDriftMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle     = context.RequestData.ReadInt32();
                 _gyroscopeZeroDriftMode = (HidGyroscopeZeroDriftMode)context.RequestData.ReadInt32();
            long appletResourceUserId    = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _gyroscopeZeroDriftMode });

            return 0;
        }

        [Command(80)]
        // GetGyroscopeZeroDriftMode(nn::applet::AppletResourceUserId, nn::hid::SixAxisSensorHandle) -> int GyroscopeZeroDriftMode
        public long GetGyroscopeZeroDriftMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write((int)_gyroscopeZeroDriftMode);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _gyroscopeZeroDriftMode });

            return 0;
        }

        [Command(81)]
        // ResetGyroscopeZeroDriftMode(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long ResetGyroscopeZeroDriftMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            _gyroscopeZeroDriftMode = HidGyroscopeZeroDriftMode.Standard;

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _gyroscopeZeroDriftMode });

            return 0;
        }

        [Command(82)]
        // IsSixAxisSensorAtRest(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> bool IsAsRest
        public long IsSixAxisSensorAtRest(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            bool isAtRest = true;

            context.ResponseData.Write(isAtRest);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, isAtRest });

            return 0;
        }

        [Command(91)]
        // ActivateGesture(nn::applet::AppletResourceUserId, int Unknown0)
        public long ActivateGesture(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            int  unknown0             = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, unknown0 });

            return 0;
        }

        [Command(100)]
        // SetSupportedNpadStyleSet(nn::applet::AppletResourceUserId, nn::hid::NpadStyleTag)
        public long SetSupportedNpadStyleSet(ServiceCtx context)
        {
            _npadStyleSet = (HidNpadStyle)context.RequestData.ReadInt32();

            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _npadStyleSet });

            _npadStyleSetUpdateEvent.ReadableEvent.Signal();

            return 0;
        }

        [Command(101)]
        // GetSupportedNpadStyleSet(nn::applet::AppletResourceUserId) -> uint nn::hid::NpadStyleTag
        public long GetSupportedNpadStyleSet(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write((int)_npadStyleSet);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _npadStyleSet });

            return 0;
        }

        [Command(102)]
        // SetSupportedNpadIdType(nn::applet::AppletResourceUserId, array<NpadIdType, 9>)
        public long SetSupportedNpadIdType(ServiceCtx context)
        {
            long appletResourceUserId  = context.RequestData.ReadInt64();
            HidControllerId npadIdType = (HidControllerId)context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, npadIdType });

            return 0;
        }

        [Command(103)]
        // ActivateNpad(nn::applet::AppletResourceUserId)
        public long ActivateNpad(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(104)]
        // DeactivateNpad(nn::applet::AppletResourceUserId)
        public long DeactivateNpad(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(106)]
        // AcquireNpadStyleSetUpdateEventHandle(nn::applet::AppletResourceUserId, uint, ulong) -> nn::sf::NativeHandle
        public long AcquireNpadStyleSetUpdateEventHandle(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            int  npadId               = context.RequestData.ReadInt32();
            long npadStyleSet         = context.RequestData.ReadInt64();

            if (context.Process.HandleTable.GenerateHandle(_npadStyleSetUpdateEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, npadId, npadStyleSet });

            return 0;
        }

        [Command(107)]
        // DisconnectNpad(nn::applet::AppletResourceUserId, uint NpadIdType)
        public long DisconnectNpad(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            int  npadIdType           = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, npadIdType });

            return 0;
        }

        [Command(108)]
        // GetPlayerLedPattern(uint NpadId) -> ulong LedPattern
        public long GetPlayerLedPattern(ServiceCtx context)
        {
            int npadId = context.RequestData.ReadInt32();

            long ledPattern = 0;

            context.ResponseData.Write(ledPattern);

            Logger.PrintStub(LogClass.ServiceHid, new { npadId, ledPattern });

            return 0;
        }

        [Command(109)] // 5.0.0+
        // ActivateNpadWithRevision(nn::applet::AppletResourceUserId, int Unknown)
        public long ActivateNpadWithRevision(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            int  unknown              = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, unknown });

            return 0;
        }

        [Command(120)]
        // SetNpadJoyHoldType(nn::applet::AppletResourceUserId, long NpadJoyHoldType)
        public long SetNpadJoyHoldType(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            _npadJoyHoldType          = (HidNpadJoyHoldType)context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _npadJoyHoldType });

            return 0;
        }

        [Command(121)]
        // GetNpadJoyHoldType(nn::applet::AppletResourceUserId) -> long NpadJoyHoldType
        public long GetNpadJoyHoldType(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write((long)_npadJoyHoldType);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _npadJoyHoldType });

            return 0;
        }

        [Command(122)]
        // SetNpadJoyAssignmentModeSingleByDefault(uint HidControllerId, nn::applet::AppletResourceUserId)
        public long SetNpadJoyAssignmentModeSingleByDefault(ServiceCtx context)
        {
            HidControllerId hidControllerId      = (HidControllerId)context.RequestData.ReadInt32();
            long            appletResourceUserId = context.RequestData.ReadInt64();

            _npadJoyAssignmentMode = HidNpadJoyAssignmentMode.Single;

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, hidControllerId, _npadJoyAssignmentMode });

            return 0;
        }

        [Command(123)]
        // SetNpadJoyAssignmentModeSingle(uint HidControllerId, nn::applet::AppletResourceUserId, long HidNpadJoyDeviceType)
        public long SetNpadJoyAssignmentModeSingle(ServiceCtx context)
        {
            HidControllerId      hidControllerId      = (HidControllerId)context.RequestData.ReadInt32();
            long                 appletResourceUserId = context.RequestData.ReadInt64();
            HidNpadJoyDeviceType hidNpadJoyDeviceType = (HidNpadJoyDeviceType)context.RequestData.ReadInt64();

            _npadJoyAssignmentMode = HidNpadJoyAssignmentMode.Single;

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, hidControllerId, hidNpadJoyDeviceType, _npadJoyAssignmentMode });

            return 0;
        }

        [Command(124)]
        // SetNpadJoyAssignmentModeDual(uint HidControllerId, nn::applet::AppletResourceUserId)
        public long SetNpadJoyAssignmentModeDual(ServiceCtx context)
        {
            HidControllerId hidControllerId      = (HidControllerId)context.RequestData.ReadInt32();
            long            appletResourceUserId = context.RequestData.ReadInt64();

            _npadJoyAssignmentMode = HidNpadJoyAssignmentMode.Dual;

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, hidControllerId, _npadJoyAssignmentMode });

            return 0;
        }

        [Command(125)]
        // MergeSingleJoyAsDualJoy(uint SingleJoyId0, uint SingleJoyId1, nn::applet::AppletResourceUserId)
        public long MergeSingleJoyAsDualJoy(ServiceCtx context)
        {
            long singleJoyId0         = context.RequestData.ReadInt32();
            long singleJoyId1         = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, singleJoyId0, singleJoyId1 });

            return 0;
        }

        [Command(126)]
        // StartLrAssignmentMode(nn::applet::AppletResourceUserId)
        public long StartLrAssignmentMode(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(127)]
        // StopLrAssignmentMode(nn::applet::AppletResourceUserId)
        public long StopLrAssignmentMode(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(128)]
        // SetNpadHandheldActivationMode(nn::applet::AppletResourceUserId, long HidNpadHandheldActivationMode)
        public long SetNpadHandheldActivationMode(ServiceCtx context)
        {
            long appletResourceUserId   = context.RequestData.ReadInt64();
            _npadHandheldActivationMode = (HidNpadHandheldActivationMode)context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _npadHandheldActivationMode });

            return 0;
        }

        [Command(129)]
        // GetNpadHandheldActivationMode(nn::applet::AppletResourceUserId) -> long HidNpadHandheldActivationMode
        public long GetNpadHandheldActivationMode(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write((long)_npadHandheldActivationMode);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _npadHandheldActivationMode });

            return 0;
        }

        [Command(130)]
        // SwapNpadAssignment(uint OldNpadAssignment, uint NewNpadAssignment, nn::applet::AppletResourceUserId)
        public long SwapNpadAssignment(ServiceCtx context)
        {
            int  oldNpadAssignment    = context.RequestData.ReadInt32();
            int  newNpadAssignment    = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, oldNpadAssignment, newNpadAssignment });

            return 0;
        }

        [Command(131)]
        // IsUnintendedHomeButtonInputProtectionEnabled(uint Unknown0, nn::applet::AppletResourceUserId) ->  bool IsEnabled
        public long IsUnintendedHomeButtonInputProtectionEnabled(ServiceCtx context)
        {
            uint  unknown0            = context.RequestData.ReadUInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_unintendedHomeButtonInputProtectionEnabled);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, unknown0, _unintendedHomeButtonInputProtectionEnabled });

            return 0;
        }

        [Command(132)]
        // EnableUnintendedHomeButtonInputProtection(bool Enable, uint Unknown0, nn::applet::AppletResourceUserId)
        public long EnableUnintendedHomeButtonInputProtection(ServiceCtx context)
        {
            _unintendedHomeButtonInputProtectionEnabled = context.RequestData.ReadBoolean();
            uint  unknown0                              = context.RequestData.ReadUInt32();
            long appletResourceUserId                   = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, unknown0, _unintendedHomeButtonInputProtectionEnabled });

            return 0;
        }

        [Command(133)] // 5.0.0+
        // SetNpadJoyAssignmentModeSingleWithDestination(uint HidControllerId, long HidNpadJoyDeviceType, nn::applet::AppletResourceUserId) -> bool Unknown0, uint Unknown1
        public long SetNpadJoyAssignmentModeSingleWithDestination(ServiceCtx context)
        {
            HidControllerId      hidControllerId      = (HidControllerId)context.RequestData.ReadInt32();
            HidNpadJoyDeviceType hidNpadJoyDeviceType = (HidNpadJoyDeviceType)context.RequestData.ReadInt64();
            long                 appletResourceUserId = context.RequestData.ReadInt64();

            _npadJoyAssignmentMode = HidNpadJoyAssignmentMode.Single;

            context.ResponseData.Write(0); //Unknown0
            context.ResponseData.Write(0); //Unknown1

            Logger.PrintStub(LogClass.ServiceHid, new {
                appletResourceUserId,
                hidControllerId,
                hidNpadJoyDeviceType,
                _npadJoyAssignmentMode,
                Unknown0 = 0,
                Unknown1 = 0
            });

            return 0;
        }

        [Command(200)]
        // GetVibrationDeviceInfo(nn::hid::VibrationDeviceHandle) -> nn::hid::VibrationDeviceInfo
        public long GetVibrationDeviceInfo(ServiceCtx context)
        {
            int vibrationDeviceHandle = context.RequestData.ReadInt32();

            HidVibrationDeviceValue deviceInfo = new HidVibrationDeviceValue
            {
                DeviceType = HidVibrationDeviceType.None,
                Position   = HidVibrationDevicePosition.None
            };

            context.ResponseData.Write((int)deviceInfo.DeviceType);
            context.ResponseData.Write((int)deviceInfo.Position);

            Logger.PrintStub(LogClass.ServiceHid, new { vibrationDeviceHandle, deviceInfo.DeviceType, deviceInfo.Position });

            return 0;
        }

        [Command(201)]
        // SendVibrationValue(nn::hid::VibrationDeviceHandle, nn::hid::VibrationValue, nn::applet::AppletResourceUserId)
        public long SendVibrationValue(ServiceCtx context)
        {
            int vibrationDeviceHandle = context.RequestData.ReadInt32();

            _vibrationValue = new HidVibrationValue
            {
                AmplitudeLow  = context.RequestData.ReadSingle(),
                FrequencyLow  = context.RequestData.ReadSingle(),
                AmplitudeHigh = context.RequestData.ReadSingle(),
                FrequencyHigh = context.RequestData.ReadSingle()
            };

            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new {
                appletResourceUserId,
                vibrationDeviceHandle,
                _vibrationValue.AmplitudeLow,
                _vibrationValue.FrequencyLow,
                _vibrationValue.AmplitudeHigh,
                _vibrationValue.FrequencyHigh
            });

            return 0;
        }

        [Command(202)]
        // GetActualVibrationValue(nn::hid::VibrationDeviceHandle, nn::applet::AppletResourceUserId) -> nn::hid::VibrationValue
        public long GetActualVibrationValue(ServiceCtx context)
        {
            int vibrationDeviceHandle = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_vibrationValue.AmplitudeLow);
            context.ResponseData.Write(_vibrationValue.FrequencyLow);
            context.ResponseData.Write(_vibrationValue.AmplitudeHigh);
            context.ResponseData.Write(_vibrationValue.FrequencyHigh);

            Logger.PrintStub(LogClass.ServiceHid, new {
                appletResourceUserId,
                vibrationDeviceHandle,
                _vibrationValue.AmplitudeLow,
                _vibrationValue.FrequencyLow,
                _vibrationValue.AmplitudeHigh,
                _vibrationValue.FrequencyHigh
            });

            return 0;
        }

        [Command(203)]
        // CreateActiveVibrationDeviceList() -> object<nn::hid::IActiveVibrationDeviceList>
        public long CreateActiveVibrationDeviceList(ServiceCtx context)
        {
            MakeObject(context, new IActiveApplicationDeviceList());

            return 0;
        }

        [Command(204)]
        // PermitVibration(bool Enable)
        public long PermitVibration(ServiceCtx context)
        {
            _vibrationPermitted = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServiceHid, new { _vibrationPermitted });

            return 0;
        }

        [Command(205)]
        // IsVibrationPermitted() -> bool IsEnabled
        public long IsVibrationPermitted(ServiceCtx context)
        {
            context.ResponseData.Write(_vibrationPermitted);

            Logger.PrintStub(LogClass.ServiceHid, new { _vibrationPermitted });

            return 0;
        }

        [Command(206)]
        // SendVibrationValues(nn::applet::AppletResourceUserId, buffer<array<nn::hid::VibrationDeviceHandle>, type: 9>, buffer<array<nn::hid::VibrationValue>, type: 9>)
        public long SendVibrationValues(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            byte[] vibrationDeviceHandleBuffer = context.Memory.ReadBytes(
                context.Request.PtrBuff[0].Position,
                context.Request.PtrBuff[0].Size);

            byte[] vibrationValueBuffer = context.Memory.ReadBytes(
                context.Request.PtrBuff[1].Position,
                context.Request.PtrBuff[1].Size);

            // TODO: Read all handles and values from buffer.

            Logger.PrintStub(LogClass.ServiceHid, new {
                appletResourceUserId,
                VibrationDeviceHandleBufferLength = vibrationDeviceHandleBuffer.Length,
                VibrationValueBufferLength = vibrationValueBuffer.Length
            });

            return 0;
        }

        [Command(207)] // 4.0.0+
        // SendVibrationGcErmCommand(nn::hid::VibrationDeviceHandle, nn::hid::VibrationGcErmCommand, nn::applet::AppletResourceUserId)
        public long SendVibrationGcErmCommand(ServiceCtx context)
        {
            int  vibrationDeviceHandle = context.RequestData.ReadInt32();
            long vibrationGcErmCommand = context.RequestData.ReadInt64();
            long appletResourceUserId  = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, vibrationDeviceHandle, vibrationGcErmCommand });

            return 0;
        }

        [Command(208)] // 4.0.0+
        // GetActualVibrationGcErmCommand(nn::hid::VibrationDeviceHandle, nn::applet::AppletResourceUserId) -> nn::hid::VibrationGcErmCommand
        public long GetActualVibrationGcErmCommand(ServiceCtx context)
        {
            int  vibrationDeviceHandle = context.RequestData.ReadInt32();
            long appletResourceUserId  = context.RequestData.ReadInt64();

            context.ResponseData.Write(_vibrationGcErmCommand);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, vibrationDeviceHandle, _vibrationGcErmCommand });

            return 0;
        }

        [Command(209)] // 4.0.0+
        // BeginPermitVibrationSession(nn::applet::AppletResourceUserId)
        public long BeginPermitVibrationSession(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(210)] // 4.0.0+
        // EndPermitVibrationSession()
        public long EndPermitVibrationSession(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceHid);

            return 0;
        }

        [Command(300)]
        // ActivateConsoleSixAxisSensor(nn::applet::AppletResourceUserId)
        public long ActivateConsoleSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(301)]
        // StartConsoleSixAxisSensor(nn::hid::ConsoleSixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long StartConsoleSixAxisSensor(ServiceCtx context)
        {
            int  consoleSixAxisSensorHandle = context.RequestData.ReadInt32();
            long appletResourceUserId       = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, consoleSixAxisSensorHandle });

            return 0;
        }

        [Command(302)]
        // StopConsoleSixAxisSensor(nn::hid::ConsoleSixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long StopConsoleSixAxisSensor(ServiceCtx context)
        {
            int  consoleSixAxisSensorHandle = context.RequestData.ReadInt32();
            long appletResourceUserId       = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, consoleSixAxisSensorHandle });

            return 0;
        }

        [Command(303)] // 5.0.0+
        // ActivateSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public long ActivateSevenSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(304)] // 5.0.0+
        // StartSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public long StartSevenSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(305)] // 5.0.0+
        // StopSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public long StopSevenSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(306)] // 5.0.0+
        // InitializeSevenSixAxisSensor(array<nn::sf::NativeHandle>, ulong Counter0, array<nn::sf::NativeHandle>, ulong Counter1, nn::applet::AppletResourceUserId)
        public long InitializeSevenSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            long counter0             = context.RequestData.ReadInt64();
            long counter1             = context.RequestData.ReadInt64();

            // TODO: Determine if array<nn::sf::NativeHandle> is a buffer or not...

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, counter0, counter1 });

            return 0;
        }

        [Command(307)] // 5.0.0+
        // FinalizeSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public long FinalizeSevenSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return 0;
        }

        [Command(308)] // 5.0.0+
        // SetSevenSixAxisSensorFusionStrength(float Strength, nn::applet::AppletResourceUserId)
        public long SetSevenSixAxisSensorFusionStrength(ServiceCtx context)
        {
                 _sevenSixAxisSensorFusionStrength = context.RequestData.ReadSingle();
            long appletResourceUserId              = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _sevenSixAxisSensorFusionStrength });

            return 0;
        }

        [Command(309)] // 5.0.0+
        // GetSevenSixAxisSensorFusionStrength(nn::applet::AppletResourceUserId) -> float Strength
        public long GetSevenSixAxisSensorFusionStrength(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_sevenSixAxisSensorFusionStrength);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _sevenSixAxisSensorFusionStrength });

            return 0;
        }

        [Command(400)]
        // IsUsbFullKeyControllerEnabled() -> bool IsEnabled
        public long IsUsbFullKeyControllerEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(_usbFullKeyControllerEnabled);

            Logger.PrintStub(LogClass.ServiceHid, new { _usbFullKeyControllerEnabled });

            return 0;
        }

        [Command(401)]
        // EnableUsbFullKeyController(bool Enable)
        public long EnableUsbFullKeyController(ServiceCtx context)
        {
            _usbFullKeyControllerEnabled = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServiceHid, new { _usbFullKeyControllerEnabled });

            return 0;
        }

        [Command(402)]
        // IsUsbFullKeyControllerConnected(uint Unknown0) -> bool Connected
        public long IsUsbFullKeyControllerConnected(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();

            context.ResponseData.Write(true); //FullKeyController is always connected ?

            Logger.PrintStub(LogClass.ServiceHid, new { unknown0, Connected = true });

            return 0;
        }

        [Command(403)] // 4.0.0+
        // HasBattery(uint NpadId) -> bool HasBattery
        public long HasBattery(ServiceCtx context)
        {
            int npadId = context.RequestData.ReadInt32();

            context.ResponseData.Write(true); //Npad always got a battery ?

            Logger.PrintStub(LogClass.ServiceHid, new { npadId, HasBattery = true });

            return 0;
        }

        [Command(404)] // 4.0.0+
        // HasLeftRightBattery(uint NpadId) -> bool HasLeftBattery, bool HasRightBattery
        public long HasLeftRightBattery(ServiceCtx context)
        {
            int npadId = context.RequestData.ReadInt32();

            context.ResponseData.Write(true); //Npad always got a left battery ?
            context.ResponseData.Write(true); //Npad always got a right battery ?

            Logger.PrintStub(LogClass.ServiceHid, new { npadId, HasLeftBattery = true, HasRightBattery = true });

            return 0;
        }

        [Command(405)] // 4.0.0+
        // GetNpadInterfaceType(uint NpadId) -> uchar InterfaceType
        public long GetNpadInterfaceType(ServiceCtx context)
        {
            int npadId = context.RequestData.ReadInt32();

            context.ResponseData.Write((byte)0);

            Logger.PrintStub(LogClass.ServiceHid, new { npadId, NpadInterfaceType = 0 });

            return 0;
        }

        [Command(406)] // 4.0.0+
        // GetNpadLeftRightInterfaceType(uint NpadId) -> uchar LeftInterfaceType, uchar RightInterfaceType
        public long GetNpadLeftRightInterfaceType(ServiceCtx context)
        {
            int npadId = context.RequestData.ReadInt32();

            context.ResponseData.Write((byte)0);
            context.ResponseData.Write((byte)0);

            Logger.PrintStub(LogClass.ServiceHid, new { npadId, LeftInterfaceType = 0, RightInterfaceType = 0 });

            return 0;
        }

        [Command(500)] // 5.0.0+
        // GetPalmaConnectionHandle(uint Unknown0, nn::applet::AppletResourceUserId) -> nn::hid::PalmaConnectionHandle
        public long GetPalmaConnectionHandle(ServiceCtx context)
        {
            int  unknown0             = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            int palmaConnectionHandle = 0;

            context.ResponseData.Write(palmaConnectionHandle);

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId , unknown0, palmaConnectionHandle });

            return 0;
        }

        [Command(501)] // 5.0.0+
        // InitializePalma(nn::hid::PalmaConnectionHandle)
        public long InitializePalma(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return 0;
        }

        [Command(502)] // 5.0.0+
        // AcquirePalmaOperationCompleteEvent(nn::hid::PalmaConnectionHandle) -> nn::sf::NativeHandle
        public long AcquirePalmaOperationCompleteEvent(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            if (context.Process.HandleTable.GenerateHandle(_palmaOperationCompleteEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            return 0;
        }

        [Command(503)] // 5.0.0+
        // GetPalmaOperationInfo(nn::hid::PalmaConnectionHandle) -> long Unknown0, buffer<Unknown>
        public long GetPalmaOperationInfo(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            long unknown0 = 0; //Counter?

            context.ResponseData.Write(unknown0);

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, unknown0 });

            return 0;
        }

        [Command(504)] // 5.0.0+
        // PlayPalmaActivity(nn::hid::PalmaConnectionHandle, ulong Unknown0)
        public long PlayPalmaActivity(ServiceCtx context)
        {
            int  palmaConnectionHandle = context.RequestData.ReadInt32();
            long unknown0              = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, unknown0 });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return 0;
        }

        [Command(505)] // 5.0.0+
        // SetPalmaFrModeType(nn::hid::PalmaConnectionHandle, ulong FrModeType)
        public long SetPalmaFrModeType(ServiceCtx context)
        {
            int  palmaConnectionHandle = context.RequestData.ReadInt32();
            long frModeType            = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, frModeType });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return 0;
        }

        [Command(506)] // 5.0.0+
        // ReadPalmaStep(nn::hid::PalmaConnectionHandle)
        public long ReadPalmaStep(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            return 0;
        }

        [Command(507)] // 5.0.0+
        // EnablePalmaStep(nn::hid::PalmaConnectionHandle, bool Enable)
        public long EnablePalmaStep(ServiceCtx context)
        {
            int  palmaConnectionHandle = context.RequestData.ReadInt32();
            bool enabledPalmaStep      = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, enabledPalmaStep });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return 0;
        }

        [Command(508)] // 5.0.0+
        // ResetPalmaStep(nn::hid::PalmaConnectionHandle)
        public long ResetPalmaStep(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return 0;
        }

        [Command(509)] // 5.0.0+
        // ReadPalmaApplicationSection(nn::hid::PalmaConnectionHandle, ulong Unknown0, ulong Unknown1)
        public long ReadPalmaApplicationSection(ServiceCtx context)
        {
            int  palmaConnectionHandle = context.RequestData.ReadInt32();
            long unknown0              = context.RequestData.ReadInt64();
            long unknown1              = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, unknown0, unknown1 });

            return 0;
        }

        [Command(510)] // 5.0.0+
        // WritePalmaApplicationSection(nn::hid::PalmaConnectionHandle, ulong Unknown0, ulong Unknown1, nn::hid::PalmaApplicationSectionAccessBuffer)
        public long WritePalmaApplicationSection(ServiceCtx context)
        {
            int  palmaConnectionHandle = context.RequestData.ReadInt32();
            long unknown0              = context.RequestData.ReadInt64();
            long unknown1              = context.RequestData.ReadInt64();
            // nn::hid::PalmaApplicationSectionAccessBuffer cast is unknown

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, unknown0, unknown1 });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return 0;
        }

        [Command(511)] // 5.0.0+
        // ReadPalmaUniqueCode(nn::hid::PalmaConnectionHandle)
        public long ReadPalmaUniqueCode(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            return 0;
        }

        [Command(512)] // 5.0.0+
        // SetPalmaUniqueCodeInvalid(nn::hid::PalmaConnectionHandle)
        public long SetPalmaUniqueCodeInvalid(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            return 0;
        }

        [Command(1000)]
        // SetNpadCommunicationMode(long CommunicationMode, nn::applet::AppletResourceUserId)
        public long SetNpadCommunicationMode(ServiceCtx context)
        {
                 _npadCommunicationMode = context.RequestData.ReadInt64();
            long appletResourceUserId   = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _npadCommunicationMode });

            return 0;
        }

        [Command(1001)]
        // GetNpadCommunicationMode() -> long CommunicationMode
        public long GetNpadCommunicationMode(ServiceCtx context)
        {
            context.ResponseData.Write(_npadCommunicationMode);

            Logger.PrintStub(LogClass.ServiceHid, new { _npadCommunicationMode });

            return 0;
        }
    }
}
