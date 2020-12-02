using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Hid.HidServer;
using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Service("hid")]
    class IHidServer : IpcService
    {
        private KEvent _xpadIdEvent;
        private KEvent _palmaOperationCompleteEvent;

        private int _xpadIdEventHandle;

        private bool _sixAxisSensorFusionEnabled;
        private bool _unintendedHomeButtonInputProtectionEnabled;
        private bool _vibrationPermitted;
        private bool _usbFullKeyControllerEnabled;

        private HidNpadJoyAssignmentMode      _npadJoyAssignmentMode;
        private HidNpadHandheldActivationMode _npadHandheldActivationMode;
        private HidGyroscopeZeroDriftMode     _gyroscopeZeroDriftMode;

        private long  _npadCommunicationMode;
        private uint  _accelerometerPlayMode;
#pragma warning disable CS0649
        private long  _vibrationGcErmCommand;
#pragma warning restore CS0649
        private float _sevenSixAxisSensorFusionStrength;

        private HidSensorFusionParameters  _sensorFusionParams;
        private HidAccelerometerParameters _accelerometerParams;
        private HidVibrationValue          _vibrationValue;

        public IHidServer(ServiceCtx context) : base(context.Device.System.HidServer)
        {
            _xpadIdEvent                 = new KEvent(context.Device.System.KernelContext);
            _palmaOperationCompleteEvent = new KEvent(context.Device.System.KernelContext);

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
        public ResultCode CreateAppletResource(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            MakeObject(context, new IAppletResource(context.Device.System.HidSharedMem));

            return ResultCode.Success;
        }

        [Command(1)]
        // ActivateDebugPad(nn::applet::AppletResourceUserId)
        public ResultCode ActivateDebugPad(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(11)]
        // ActivateTouchScreen(nn::applet::AppletResourceUserId)
        public ResultCode ActivateTouchScreen(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.Device.Hid.Touchscreen.Active = true;

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(21)]
        // ActivateMouse(nn::applet::AppletResourceUserId)
        public ResultCode ActivateMouse(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.Device.Hid.Mouse.Active = true;

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(31)]
        // ActivateKeyboard(nn::applet::AppletResourceUserId)
        public ResultCode ActivateKeyboard(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.Device.Hid.Keyboard.Active = true;

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(32)]
        // SendKeyboardLockKeyEvent(uint flags, pid)
        public ResultCode SendKeyboardLockKeyEvent(ServiceCtx context)
        {
            uint flags = context.RequestData.ReadUInt32();

            // NOTE: This signal the keyboard driver about lock events.

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { flags });

            return ResultCode.Success;
        }

        [Command(40)]
        // AcquireXpadIdEventHandle(ulong XpadId) -> nn::sf::NativeHandle
        public ResultCode AcquireXpadIdEventHandle(ServiceCtx context)
        {
            long xpadId = context.RequestData.ReadInt64();

            if (context.Process.HandleTable.GenerateHandle(_xpadIdEvent.ReadableEvent, out _xpadIdEventHandle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_xpadIdEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { xpadId });

            return ResultCode.Success;
        }

        [Command(41)]
        // ReleaseXpadIdEventHandle(ulong XpadId)
        public ResultCode ReleaseXpadIdEventHandle(ServiceCtx context)
        {
            long xpadId = context.RequestData.ReadInt64();

            context.Process.HandleTable.CloseHandle(_xpadIdEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { xpadId });

            return ResultCode.Success;
        }

        [Command(51)]
        // ActivateXpad(nn::hid::BasicXpadId, nn::applet::AppletResourceUserId)
        public ResultCode ActivateXpad(ServiceCtx context)
        {
            int  basicXpadId          = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, basicXpadId });

            return ResultCode.Success;
        }

        [Command(55)]
        // GetXpadIds() -> long IdsCount, buffer<array<nn::hid::BasicXpadId>, type: 0xa>
        public ResultCode GetXpadIds(ServiceCtx context)
        {
            // There is any Xpad, so we return 0 and write nothing inside the type-0xa buffer.
            context.ResponseData.Write(0L);

            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            return ResultCode.Success;
        }

        [Command(56)]
        // ActivateJoyXpad(nn::hid::JoyXpadId)
        public ResultCode ActivateJoyXpad(ServiceCtx context)
        {
            int joyXpadId = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { joyXpadId });

            return ResultCode.Success;
        }

        [Command(58)]
        // GetJoyXpadLifoHandle(nn::hid::JoyXpadId) -> nn::sf::NativeHandle
        public ResultCode GetJoyXpadLifoHandle(ServiceCtx context)
        {
            int joyXpadId = context.RequestData.ReadInt32();

            int handle = 0;

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { joyXpadId });

            return ResultCode.Success;
        }

        [Command(59)]
        // GetJoyXpadIds() -> long IdsCount, buffer<array<nn::hid::JoyXpadId>, type: 0xa>
        public ResultCode GetJoyXpadIds(ServiceCtx context)
        {
            // There is any JoyXpad, so we return 0 and write nothing inside the type-0xa buffer.
            context.ResponseData.Write(0L);

            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            return ResultCode.Success;
        }

        [Command(60)]
        // ActivateSixAxisSensor(nn::hid::BasicXpadId)
        public ResultCode ActivateSixAxisSensor(ServiceCtx context)
        {
            int basicXpadId = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { basicXpadId });

            return ResultCode.Success;
        }

        [Command(61)]
        // DeactivateSixAxisSensor(nn::hid::BasicXpadId)
        public ResultCode DeactivateSixAxisSensor(ServiceCtx context)
        {
            int basicXpadId = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { basicXpadId });

            return ResultCode.Success;
        }

        [Command(62)]
        // GetSixAxisSensorLifoHandle(nn::hid::BasicXpadId) -> nn::sf::NativeHandle
        public ResultCode GetSixAxisSensorLifoHandle(ServiceCtx context)
        {
            int basicXpadId = context.RequestData.ReadInt32();

            int handle = 0;

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { basicXpadId });

            return ResultCode.Success;
        }

        [Command(63)]
        // ActivateJoySixAxisSensor(nn::hid::JoyXpadId)
        public ResultCode ActivateJoySixAxisSensor(ServiceCtx context)
        {
            int joyXpadId = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { joyXpadId });

            return ResultCode.Success;
        }

        [Command(64)]
        // DeactivateJoySixAxisSensor(nn::hid::JoyXpadId)
        public ResultCode DeactivateJoySixAxisSensor(ServiceCtx context)
        {
            int joyXpadId = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { joyXpadId });

            return ResultCode.Success;
        }

        [Command(65)]
        // GetJoySixAxisSensorLifoHandle(nn::hid::JoyXpadId) -> nn::sf::NativeHandle
        public ResultCode GetJoySixAxisSensorLifoHandle(ServiceCtx context)
        {
            int joyXpadId = context.RequestData.ReadInt32();

            int handle = 0;

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { joyXpadId });

            return ResultCode.Success;
        }

        [Command(66)]
        // StartSixAxisSensor(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public ResultCode StartSixAxisSensor(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle });

            return ResultCode.Success;
        }

        [Command(67)]
        // StopSixAxisSensor(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public ResultCode StopSixAxisSensor(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle });

            return ResultCode.Success;
        }

        [Command(68)]
        // IsSixAxisSensorFusionEnabled(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> bool IsEnabled
        public ResultCode IsSixAxisSensorFusionEnabled(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_sixAxisSensorFusionEnabled);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _sixAxisSensorFusionEnabled });

            return ResultCode.Success;
        }

        [Command(69)]
        // EnableSixAxisSensorFusion(bool Enabled, nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public ResultCode EnableSixAxisSensorFusion(ServiceCtx context)
        {
            _sixAxisSensorFusionEnabled = context.RequestData.ReadBoolean();
            int  sixAxisSensorHandle    = context.RequestData.ReadInt32();
            long appletResourceUserId   = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _sixAxisSensorFusionEnabled });

            return ResultCode.Success;
        }

        [Command(70)]
        // SetSixAxisSensorFusionParameters(nn::hid::SixAxisSensorHandle, float RevisePower, float ReviseRange, nn::applet::AppletResourceUserId)
        public ResultCode SetSixAxisSensorFusionParameters(ServiceCtx context)
        {
            int   sixAxisSensorHandle = context.RequestData.ReadInt32();

            _sensorFusionParams = new HidSensorFusionParameters
            {
                RevisePower = context.RequestData.ReadInt32(),
                ReviseRange = context.RequestData.ReadInt32()
            };

            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _sensorFusionParams.RevisePower, _sensorFusionParams.ReviseRange });

            return ResultCode.Success;
        }

        [Command(71)]
        // GetSixAxisSensorFusionParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> float RevisePower, float ReviseRange)
        public ResultCode GetSixAxisSensorFusionParameters(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_sensorFusionParams.RevisePower);
            context.ResponseData.Write(_sensorFusionParams.ReviseRange);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _sensorFusionParams.RevisePower, _sensorFusionParams.ReviseRange });

            return ResultCode.Success;
        }

        [Command(72)]
        // ResetSixAxisSensorFusionParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public ResultCode ResetSixAxisSensorFusionParameters(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            _sensorFusionParams.RevisePower = 0;
            _sensorFusionParams.ReviseRange = 0;

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _sensorFusionParams.RevisePower, _sensorFusionParams.ReviseRange });

            return ResultCode.Success;
        }

        [Command(73)]
        // SetAccelerometerParameters(nn::hid::SixAxisSensorHandle, float X, float Y, nn::applet::AppletResourceUserId)
        public ResultCode SetAccelerometerParameters(ServiceCtx context)
        {
            int sixAxisSensorHandle = context.RequestData.ReadInt32();

            _accelerometerParams = new HidAccelerometerParameters
            {
                X = context.RequestData.ReadInt32(),
                Y = context.RequestData.ReadInt32()
            };

            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerParams.X, _accelerometerParams.Y });

            return ResultCode.Success;
        }

        [Command(74)]
        // GetAccelerometerParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> float X, float Y
        public ResultCode GetAccelerometerParameters(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_accelerometerParams.X);
            context.ResponseData.Write(_accelerometerParams.Y);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerParams.X, _accelerometerParams.Y });

            return ResultCode.Success;
        }

        [Command(75)]
        // ResetAccelerometerParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public ResultCode ResetAccelerometerParameters(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            _accelerometerParams.X = 0;
            _accelerometerParams.Y = 0;

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerParams.X, _accelerometerParams.Y });

            return ResultCode.Success;
        }

        [Command(76)]
        // SetAccelerometerPlayMode(nn::hid::SixAxisSensorHandle, uint PlayMode, nn::applet::AppletResourceUserId)
        public ResultCode SetAccelerometerPlayMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle    = context.RequestData.ReadInt32();
                 _accelerometerPlayMode = context.RequestData.ReadUInt32();
            long appletResourceUserId   = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerPlayMode });

            return ResultCode.Success;
        }

        [Command(77)]
        // GetAccelerometerPlayMode(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> uint PlayMode
        public ResultCode GetAccelerometerPlayMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_accelerometerPlayMode);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerPlayMode });

            return ResultCode.Success;
        }

        [Command(78)]
        // ResetAccelerometerPlayMode(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public ResultCode ResetAccelerometerPlayMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            _accelerometerPlayMode = 0;

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _accelerometerPlayMode });

            return ResultCode.Success;
        }

        [Command(79)]
        // SetGyroscopeZeroDriftMode(nn::hid::SixAxisSensorHandle, uint GyroscopeZeroDriftMode, nn::applet::AppletResourceUserId)
        public ResultCode SetGyroscopeZeroDriftMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle     = context.RequestData.ReadInt32();
                 _gyroscopeZeroDriftMode = (HidGyroscopeZeroDriftMode)context.RequestData.ReadInt32();
            long appletResourceUserId    = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _gyroscopeZeroDriftMode });

            return ResultCode.Success;
        }

        [Command(80)]
        // GetGyroscopeZeroDriftMode(nn::applet::AppletResourceUserId, nn::hid::SixAxisSensorHandle) -> int GyroscopeZeroDriftMode
        public ResultCode GetGyroscopeZeroDriftMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write((int)_gyroscopeZeroDriftMode);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _gyroscopeZeroDriftMode });

            return ResultCode.Success;
        }

        [Command(81)]
        // ResetGyroscopeZeroDriftMode(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public ResultCode ResetGyroscopeZeroDriftMode(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            _gyroscopeZeroDriftMode = HidGyroscopeZeroDriftMode.Standard;

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, _gyroscopeZeroDriftMode });

            return ResultCode.Success;
        }

        [Command(82)]
        // IsSixAxisSensorAtRest(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> bool IsAsRest
        public ResultCode IsSixAxisSensorAtRest(ServiceCtx context)
        {
            int  sixAxisSensorHandle  = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            bool isAtRest = true;

            context.ResponseData.Write(isAtRest);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, sixAxisSensorHandle, isAtRest });

            return ResultCode.Success;
        }

        [Command(91)]
        // ActivateGesture(nn::applet::AppletResourceUserId, int Unknown0)
        public ResultCode ActivateGesture(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            int  unknown0             = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, unknown0 });

            return ResultCode.Success;
        }

        [Command(100)]
        // SetSupportedNpadStyleSet(nn::applet::AppletResourceUserId, nn::hid::NpadStyleTag)
        public ResultCode SetSupportedNpadStyleSet(ServiceCtx context)
        {
            ControllerType type = (ControllerType)context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new {
                    appletResourceUserId,
                    type
                });

            context.Device.Hid.Npads.SupportedStyleSets = type;

            return ResultCode.Success;
        }

        [Command(101)]
        // GetSupportedNpadStyleSet(nn::applet::AppletResourceUserId) -> uint nn::hid::NpadStyleTag
        public ResultCode GetSupportedNpadStyleSet(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write((int)context.Device.Hid.Npads.SupportedStyleSets);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new {
                    appletResourceUserId,
                    context.Device.Hid.Npads.SupportedStyleSets
                });

            return ResultCode.Success;
        }

        [Command(102)]
        // SetSupportedNpadIdType(nn::applet::AppletResourceUserId, array<NpadIdType, 9>)
        public ResultCode SetSupportedNpadIdType(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            long arraySize = context.Request.PtrBuff[0].Size / 4;

            NpadIdType[] supportedPlayerIds = new NpadIdType[arraySize];

            context.Device.Hid.Npads.ClearSupportedPlayers();

            for (int i = 0; i < arraySize; ++i)
            {
                NpadIdType id = context.Memory.Read<NpadIdType>((ulong)(context.Request.PtrBuff[0].Position + i * 4));

                if (id >= 0)
                {
                    context.Device.Hid.Npads.SetSupportedPlayer(HidUtils.GetIndexFromNpadIdType(id));
                }

                supportedPlayerIds[i] = id;
            }

            Logger.Stub?.PrintStub(LogClass.ServiceHid, $"{arraySize} " + string.Join(",", supportedPlayerIds));

            return ResultCode.Success;
        }

        [Command(103)]
        // ActivateNpad(nn::applet::AppletResourceUserId)
        public ResultCode ActivateNpad(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.Device.Hid.Npads.Active = true;
            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(104)]
        // DeactivateNpad(nn::applet::AppletResourceUserId)
        public ResultCode DeactivateNpad(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.Device.Hid.Npads.Active = false;
            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(106)]
        // AcquireNpadStyleSetUpdateEventHandle(nn::applet::AppletResourceUserId, uint, ulong) -> nn::sf::NativeHandle
        public ResultCode AcquireNpadStyleSetUpdateEventHandle(ServiceCtx context)
        {
            PlayerIndex npadId               = HidUtils.GetIndexFromNpadIdType((NpadIdType)context.RequestData.ReadInt32());
            long        appletResourceUserId = context.RequestData.ReadInt64();
            long        npadStyleSet         = context.RequestData.ReadInt64();

            KEvent evnt = context.Device.Hid.Npads.GetStyleSetUpdateEvent(npadId);
            if (context.Process.HandleTable.GenerateHandle(evnt.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, npadId, npadStyleSet });

            return ResultCode.Success;
        }

        [Command(107)]
        // DisconnectNpad(nn::applet::AppletResourceUserId, uint NpadIdType)
        public ResultCode DisconnectNpad(ServiceCtx context)
        {
            NpadIdType npadIdType           = (NpadIdType)context.RequestData.ReadInt32();
            long       appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, npadIdType });

            return ResultCode.Success;
        }

        [Command(108)]
        // GetPlayerLedPattern(uint NpadId) -> ulong LedPattern
        public ResultCode GetPlayerLedPattern(ServiceCtx context)
        {
            NpadIdType npadId = (NpadIdType)context.RequestData.ReadInt32();

            long ledPattern = HidUtils.GetLedPatternFromNpadId(npadId);

            context.ResponseData.Write(ledPattern);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { npadId, ledPattern });

            return ResultCode.Success;
        }

        [Command(109)] // 5.0.0+
        // ActivateNpadWithRevision(nn::applet::AppletResourceUserId, int Unknown)
        public ResultCode ActivateNpadWithRevision(ServiceCtx context)
        {
            int  revision             = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, revision });

            return ResultCode.Success;
        }

        [Command(120)]
        // SetNpadJoyHoldType(nn::applet::AppletResourceUserId, long NpadJoyHoldType)
        public ResultCode SetNpadJoyHoldType(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            context.Device.Hid.Npads.JoyHold = (NpadJoyHoldType)context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new {
                    appletResourceUserId,
                    context.Device.Hid.Npads.JoyHold
                });

            return ResultCode.Success;
        }

        [Command(121)]
        // GetNpadJoyHoldType(nn::applet::AppletResourceUserId) -> long NpadJoyHoldType
        public ResultCode GetNpadJoyHoldType(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write((long)context.Device.Hid.Npads.JoyHold);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new {
                    appletResourceUserId,
                    context.Device.Hid.Npads.JoyHold
                });

            return ResultCode.Success;
        }

        [Command(122)]
        // SetNpadJoyAssignmentModeSingleByDefault(uint HidControllerId, nn::applet::AppletResourceUserId)
        public ResultCode SetNpadJoyAssignmentModeSingleByDefault(ServiceCtx context)
        {
            PlayerIndex hidControllerId      = (PlayerIndex)context.RequestData.ReadInt32();
            long        appletResourceUserId = context.RequestData.ReadInt64();

            _npadJoyAssignmentMode = HidNpadJoyAssignmentMode.Single;

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, hidControllerId, _npadJoyAssignmentMode });

            return ResultCode.Success;
        }

        [Command(123)]
        // SetNpadJoyAssignmentModeSingle(uint HidControllerId, nn::applet::AppletResourceUserId, long HidNpadJoyDeviceType)
        public ResultCode SetNpadJoyAssignmentModeSingle(ServiceCtx context)
        {
            PlayerIndex          hidControllerId      = (PlayerIndex)context.RequestData.ReadInt32();
            long                 appletResourceUserId = context.RequestData.ReadInt64();
            HidNpadJoyDeviceType hidNpadJoyDeviceType = (HidNpadJoyDeviceType)context.RequestData.ReadInt64();

            _npadJoyAssignmentMode = HidNpadJoyAssignmentMode.Single;

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, hidControllerId, hidNpadJoyDeviceType, _npadJoyAssignmentMode });

            return ResultCode.Success;
        }

        [Command(124)]
        // SetNpadJoyAssignmentModeDual(uint HidControllerId, nn::applet::AppletResourceUserId)
        public ResultCode SetNpadJoyAssignmentModeDual(ServiceCtx context)
        {
            PlayerIndex hidControllerId      = HidUtils.GetIndexFromNpadIdType((NpadIdType)context.RequestData.ReadInt32());
            long        appletResourceUserId = context.RequestData.ReadInt64();

            _npadJoyAssignmentMode = HidNpadJoyAssignmentMode.Dual;

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, hidControllerId, _npadJoyAssignmentMode });

            return ResultCode.Success;
        }

        [Command(125)]
        // MergeSingleJoyAsDualJoy(uint SingleJoyId0, uint SingleJoyId1, nn::applet::AppletResourceUserId)
        public ResultCode MergeSingleJoyAsDualJoy(ServiceCtx context)
        {
            long singleJoyId0         = context.RequestData.ReadInt32();
            long singleJoyId1         = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, singleJoyId0, singleJoyId1 });

            return ResultCode.Success;
        }

        [Command(126)]
        // StartLrAssignmentMode(nn::applet::AppletResourceUserId)
        public ResultCode StartLrAssignmentMode(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(127)]
        // StopLrAssignmentMode(nn::applet::AppletResourceUserId)
        public ResultCode StopLrAssignmentMode(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(128)]
        // SetNpadHandheldActivationMode(nn::applet::AppletResourceUserId, long HidNpadHandheldActivationMode)
        public ResultCode SetNpadHandheldActivationMode(ServiceCtx context)
        {
            long appletResourceUserId   = context.RequestData.ReadInt64();
            _npadHandheldActivationMode = (HidNpadHandheldActivationMode)context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _npadHandheldActivationMode });

            return ResultCode.Success;
        }

        [Command(129)]
        // GetNpadHandheldActivationMode(nn::applet::AppletResourceUserId) -> long HidNpadHandheldActivationMode
        public ResultCode GetNpadHandheldActivationMode(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write((long)_npadHandheldActivationMode);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _npadHandheldActivationMode });

            return ResultCode.Success;
        }

        [Command(130)]
        // SwapNpadAssignment(uint OldNpadAssignment, uint NewNpadAssignment, nn::applet::AppletResourceUserId)
        public ResultCode SwapNpadAssignment(ServiceCtx context)
        {
            int  oldNpadAssignment    = context.RequestData.ReadInt32();
            int  newNpadAssignment    = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, oldNpadAssignment, newNpadAssignment });

            return ResultCode.Success;
        }

        [Command(131)]
        // IsUnintendedHomeButtonInputProtectionEnabled(uint Unknown0, nn::applet::AppletResourceUserId) ->  bool IsEnabled
        public ResultCode IsUnintendedHomeButtonInputProtectionEnabled(ServiceCtx context)
        {
            uint  unknown0            = context.RequestData.ReadUInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_unintendedHomeButtonInputProtectionEnabled);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, unknown0, _unintendedHomeButtonInputProtectionEnabled });

            return ResultCode.Success;
        }

        [Command(132)]
        // EnableUnintendedHomeButtonInputProtection(bool Enable, uint Unknown0, nn::applet::AppletResourceUserId)
        public ResultCode EnableUnintendedHomeButtonInputProtection(ServiceCtx context)
        {
            _unintendedHomeButtonInputProtectionEnabled = context.RequestData.ReadBoolean();
            uint  unknown0                              = context.RequestData.ReadUInt32();
            long appletResourceUserId                   = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, unknown0, _unintendedHomeButtonInputProtectionEnabled });

            return ResultCode.Success;
        }

        [Command(133)] // 5.0.0+
        // SetNpadJoyAssignmentModeSingleWithDestination(uint HidControllerId, long HidNpadJoyDeviceType, nn::applet::AppletResourceUserId) -> bool Unknown0, uint Unknown1
        public ResultCode SetNpadJoyAssignmentModeSingleWithDestination(ServiceCtx context)
        {
            PlayerIndex          hidControllerId      = (PlayerIndex)context.RequestData.ReadInt32();
            HidNpadJoyDeviceType hidNpadJoyDeviceType = (HidNpadJoyDeviceType)context.RequestData.ReadInt64();
            long                 appletResourceUserId = context.RequestData.ReadInt64();

            _npadJoyAssignmentMode = HidNpadJoyAssignmentMode.Single;

            context.ResponseData.Write(0); //Unknown0
            context.ResponseData.Write(0); //Unknown1

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new {
                appletResourceUserId,
                hidControllerId,
                hidNpadJoyDeviceType,
                _npadJoyAssignmentMode,
                Unknown0 = 0,
                Unknown1 = 0
            });

            return ResultCode.Success;
        }

        [Command(200)]
        // GetVibrationDeviceInfo(nn::hid::VibrationDeviceHandle) -> nn::hid::VibrationDeviceInfo
        public ResultCode GetVibrationDeviceInfo(ServiceCtx context)
        {
            int vibrationDeviceHandle = context.RequestData.ReadInt32();

            HidVibrationDeviceValue deviceInfo = new HidVibrationDeviceValue
            {
                DeviceType = HidVibrationDeviceType.None,
                Position   = HidVibrationDevicePosition.None
            };

            context.ResponseData.Write((int)deviceInfo.DeviceType);
            context.ResponseData.Write((int)deviceInfo.Position);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { vibrationDeviceHandle, deviceInfo.DeviceType, deviceInfo.Position });

            return ResultCode.Success;
        }

        [Command(201)]
        // SendVibrationValue(nn::hid::VibrationDeviceHandle, nn::hid::VibrationValue, nn::applet::AppletResourceUserId)
        public ResultCode SendVibrationValue(ServiceCtx context)
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

            Logger.Debug?.PrintStub(LogClass.ServiceHid, new {
                appletResourceUserId,
                vibrationDeviceHandle,
                _vibrationValue.AmplitudeLow,
                _vibrationValue.FrequencyLow,
                _vibrationValue.AmplitudeHigh,
                _vibrationValue.FrequencyHigh
            });

            return ResultCode.Success;
        }

        [Command(202)]
        // GetActualVibrationValue(nn::hid::VibrationDeviceHandle, nn::applet::AppletResourceUserId) -> nn::hid::VibrationValue
        public ResultCode GetActualVibrationValue(ServiceCtx context)
        {
            int vibrationDeviceHandle = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_vibrationValue.AmplitudeLow);
            context.ResponseData.Write(_vibrationValue.FrequencyLow);
            context.ResponseData.Write(_vibrationValue.AmplitudeHigh);
            context.ResponseData.Write(_vibrationValue.FrequencyHigh);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new {
                appletResourceUserId,
                vibrationDeviceHandle,
                _vibrationValue.AmplitudeLow,
                _vibrationValue.FrequencyLow,
                _vibrationValue.AmplitudeHigh,
                _vibrationValue.FrequencyHigh
            });

            return ResultCode.Success;
        }

        [Command(203)]
        // CreateActiveVibrationDeviceList() -> object<nn::hid::IActiveVibrationDeviceList>
        public ResultCode CreateActiveVibrationDeviceList(ServiceCtx context)
        {
            MakeObject(context, new IActiveApplicationDeviceList());

            return ResultCode.Success;
        }

        [Command(204)]
        // PermitVibration(bool Enable)
        public ResultCode PermitVibration(ServiceCtx context)
        {
            _vibrationPermitted = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { _vibrationPermitted });

            return ResultCode.Success;
        }

        [Command(205)]
        // IsVibrationPermitted() -> bool IsEnabled
        public ResultCode IsVibrationPermitted(ServiceCtx context)
        {
            context.ResponseData.Write(_vibrationPermitted);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { _vibrationPermitted });

            return ResultCode.Success;
        }

        [Command(206)]
        // SendVibrationValues(nn::applet::AppletResourceUserId, buffer<array<nn::hid::VibrationDeviceHandle>, type: 9>, buffer<array<nn::hid::VibrationValue>, type: 9>)
        public ResultCode SendVibrationValues(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            byte[] vibrationDeviceHandleBuffer = new byte[context.Request.PtrBuff[0].Size];

            context.Memory.Read((ulong)context.Request.PtrBuff[0].Position, vibrationDeviceHandleBuffer);

            byte[] vibrationValueBuffer = new byte[context.Request.PtrBuff[1].Size];

            context.Memory.Read((ulong)context.Request.PtrBuff[1].Position, vibrationValueBuffer);

            // TODO: Read all handles and values from buffer.

            Logger.Debug?.PrintStub(LogClass.ServiceHid, new {
                appletResourceUserId,
                VibrationDeviceHandleBufferLength = vibrationDeviceHandleBuffer.Length,
                VibrationValueBufferLength = vibrationValueBuffer.Length
            });

            return ResultCode.Success;
        }

        [Command(207)] // 4.0.0+
        // SendVibrationGcErmCommand(nn::hid::VibrationDeviceHandle, nn::hid::VibrationGcErmCommand, nn::applet::AppletResourceUserId)
        public ResultCode SendVibrationGcErmCommand(ServiceCtx context)
        {
            int  vibrationDeviceHandle = context.RequestData.ReadInt32();
            long vibrationGcErmCommand = context.RequestData.ReadInt64();
            long appletResourceUserId  = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, vibrationDeviceHandle, vibrationGcErmCommand });

            return ResultCode.Success;
        }

        [Command(208)] // 4.0.0+
        // GetActualVibrationGcErmCommand(nn::hid::VibrationDeviceHandle, nn::applet::AppletResourceUserId) -> nn::hid::VibrationGcErmCommand
        public ResultCode GetActualVibrationGcErmCommand(ServiceCtx context)
        {
            int  vibrationDeviceHandle = context.RequestData.ReadInt32();
            long appletResourceUserId  = context.RequestData.ReadInt64();

            context.ResponseData.Write(_vibrationGcErmCommand);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, vibrationDeviceHandle, _vibrationGcErmCommand });

            return ResultCode.Success;
        }

        [Command(209)] // 4.0.0+
        // BeginPermitVibrationSession(nn::applet::AppletResourceUserId)
        public ResultCode BeginPermitVibrationSession(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(210)] // 4.0.0+
        // EndPermitVibrationSession()
        public ResultCode EndPermitVibrationSession(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            return ResultCode.Success;
        }

        [Command(300)]
        // ActivateConsoleSixAxisSensor(nn::applet::AppletResourceUserId)
        public ResultCode ActivateConsoleSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(301)]
        // StartConsoleSixAxisSensor(nn::hid::ConsoleSixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public ResultCode StartConsoleSixAxisSensor(ServiceCtx context)
        {
            int  consoleSixAxisSensorHandle = context.RequestData.ReadInt32();
            long appletResourceUserId       = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, consoleSixAxisSensorHandle });

            return ResultCode.Success;
        }

        [Command(302)]
        // StopConsoleSixAxisSensor(nn::hid::ConsoleSixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public ResultCode StopConsoleSixAxisSensor(ServiceCtx context)
        {
            int  consoleSixAxisSensorHandle = context.RequestData.ReadInt32();
            long appletResourceUserId       = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, consoleSixAxisSensorHandle });

            return ResultCode.Success;
        }

        [Command(303)] // 5.0.0+
        // ActivateSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public ResultCode ActivateSevenSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(304)] // 5.0.0+
        // StartSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public ResultCode StartSevenSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(305)] // 5.0.0+
        // StopSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public ResultCode StopSevenSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(306)] // 5.0.0+
        // InitializeSevenSixAxisSensor(array<nn::sf::NativeHandle>, ulong Counter0, array<nn::sf::NativeHandle>, ulong Counter1, nn::applet::AppletResourceUserId)
        public ResultCode InitializeSevenSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            long counter0             = context.RequestData.ReadInt64();
            long counter1             = context.RequestData.ReadInt64();

            // TODO: Determine if array<nn::sf::NativeHandle> is a buffer or not...

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, counter0, counter1 });

            return ResultCode.Success;
        }

        [Command(307)] // 5.0.0+
        // FinalizeSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public ResultCode FinalizeSevenSixAxisSensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(308)] // 5.0.0+
        // SetSevenSixAxisSensorFusionStrength(float Strength, nn::applet::AppletResourceUserId)
        public ResultCode SetSevenSixAxisSensorFusionStrength(ServiceCtx context)
        {
                 _sevenSixAxisSensorFusionStrength = context.RequestData.ReadSingle();
            long appletResourceUserId              = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _sevenSixAxisSensorFusionStrength });

            return ResultCode.Success;
        }

        [Command(309)] // 5.0.0+
        // GetSevenSixAxisSensorFusionStrength(nn::applet::AppletResourceUserId) -> float Strength
        public ResultCode GetSevenSixAxisSensorFusionStrength(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(_sevenSixAxisSensorFusionStrength);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _sevenSixAxisSensorFusionStrength });

            return ResultCode.Success;
        }

        [Command(310)] // 6.0.0+
        // ResetSevenSixAxisSensorTimestamp(pid, nn::applet::AppletResourceUserId)
        public ResultCode ResetSevenSixAxisSensorTimestamp(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [Command(400)]
        // IsUsbFullKeyControllerEnabled() -> bool IsEnabled
        public ResultCode IsUsbFullKeyControllerEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(_usbFullKeyControllerEnabled);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { _usbFullKeyControllerEnabled });

            return ResultCode.Success;
        }

        [Command(401)]
        // EnableUsbFullKeyController(bool Enable)
        public ResultCode EnableUsbFullKeyController(ServiceCtx context)
        {
            _usbFullKeyControllerEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { _usbFullKeyControllerEnabled });

            return ResultCode.Success;
        }

        [Command(402)]
        // IsUsbFullKeyControllerConnected(uint Unknown0) -> bool Connected
        public ResultCode IsUsbFullKeyControllerConnected(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();

            context.ResponseData.Write(true); //FullKeyController is always connected ?

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { unknown0, Connected = true });

            return ResultCode.Success;
        }

        [Command(403)] // 4.0.0+
        // HasBattery(uint NpadId) -> bool HasBattery
        public ResultCode HasBattery(ServiceCtx context)
        {
            int npadId = context.RequestData.ReadInt32();

            context.ResponseData.Write(true); //Npad always got a battery ?

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { npadId, HasBattery = true });

            return ResultCode.Success;
        }

        [Command(404)] // 4.0.0+
        // HasLeftRightBattery(uint NpadId) -> bool HasLeftBattery, bool HasRightBattery
        public ResultCode HasLeftRightBattery(ServiceCtx context)
        {
            int npadId = context.RequestData.ReadInt32();

            context.ResponseData.Write(true); //Npad always got a left battery ?
            context.ResponseData.Write(true); //Npad always got a right battery ?

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { npadId, HasLeftBattery = true, HasRightBattery = true });

            return ResultCode.Success;
        }

        [Command(405)] // 4.0.0+
        // GetNpadInterfaceType(uint NpadId) -> uchar InterfaceType
        public ResultCode GetNpadInterfaceType(ServiceCtx context)
        {
            int npadId = context.RequestData.ReadInt32();

            context.ResponseData.Write((byte)0);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { npadId, NpadInterfaceType = 0 });

            return ResultCode.Success;
        }

        [Command(406)] // 4.0.0+
        // GetNpadLeftRightInterfaceType(uint NpadId) -> uchar LeftInterfaceType, uchar RightInterfaceType
        public ResultCode GetNpadLeftRightInterfaceType(ServiceCtx context)
        {
            int npadId = context.RequestData.ReadInt32();

            context.ResponseData.Write((byte)0);
            context.ResponseData.Write((byte)0);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { npadId, LeftInterfaceType = 0, RightInterfaceType = 0 });

            return ResultCode.Success;
        }

        [Command(500)] // 5.0.0+
        // GetPalmaConnectionHandle(uint Unknown0, nn::applet::AppletResourceUserId) -> nn::hid::PalmaConnectionHandle
        public ResultCode GetPalmaConnectionHandle(ServiceCtx context)
        {
            int  unknown0             = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            int palmaConnectionHandle = 0;

            context.ResponseData.Write(palmaConnectionHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId , unknown0, palmaConnectionHandle });

            return ResultCode.Success;
        }

        [Command(501)] // 5.0.0+
        // InitializePalma(nn::hid::PalmaConnectionHandle)
        public ResultCode InitializePalma(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }

        [Command(502)] // 5.0.0+
        // AcquirePalmaOperationCompleteEvent(nn::hid::PalmaConnectionHandle) -> nn::sf::NativeHandle
        public ResultCode AcquirePalmaOperationCompleteEvent(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            if (context.Process.HandleTable.GenerateHandle(_palmaOperationCompleteEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            return ResultCode.Success;
        }

        [Command(503)] // 5.0.0+
        // GetPalmaOperationInfo(nn::hid::PalmaConnectionHandle) -> long Unknown0, buffer<Unknown>
        public ResultCode GetPalmaOperationInfo(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            long unknown0 = 0; //Counter?

            context.ResponseData.Write(unknown0);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, unknown0 });

            return ResultCode.Success;
        }

        [Command(504)] // 5.0.0+
        // PlayPalmaActivity(nn::hid::PalmaConnectionHandle, ulong Unknown0)
        public ResultCode PlayPalmaActivity(ServiceCtx context)
        {
            int  palmaConnectionHandle = context.RequestData.ReadInt32();
            long unknown0              = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, unknown0 });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }

        [Command(505)] // 5.0.0+
        // SetPalmaFrModeType(nn::hid::PalmaConnectionHandle, ulong FrModeType)
        public ResultCode SetPalmaFrModeType(ServiceCtx context)
        {
            int  palmaConnectionHandle = context.RequestData.ReadInt32();
            long frModeType            = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, frModeType });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }

        [Command(506)] // 5.0.0+
        // ReadPalmaStep(nn::hid::PalmaConnectionHandle)
        public ResultCode ReadPalmaStep(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            return ResultCode.Success;
        }

        [Command(507)] // 5.0.0+
        // EnablePalmaStep(nn::hid::PalmaConnectionHandle, bool Enable)
        public ResultCode EnablePalmaStep(ServiceCtx context)
        {
            int  palmaConnectionHandle = context.RequestData.ReadInt32();
            bool enabledPalmaStep      = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, enabledPalmaStep });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }

        [Command(508)] // 5.0.0+
        // ResetPalmaStep(nn::hid::PalmaConnectionHandle)
        public ResultCode ResetPalmaStep(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }

        [Command(509)] // 5.0.0+
        // ReadPalmaApplicationSection(nn::hid::PalmaConnectionHandle, ulong Unknown0, ulong Unknown1)
        public ResultCode ReadPalmaApplicationSection(ServiceCtx context)
        {
            int  palmaConnectionHandle = context.RequestData.ReadInt32();
            long unknown0              = context.RequestData.ReadInt64();
            long unknown1              = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, unknown0, unknown1 });

            return ResultCode.Success;
        }

        [Command(510)] // 5.0.0+
        // WritePalmaApplicationSection(nn::hid::PalmaConnectionHandle, ulong Unknown0, ulong Unknown1, nn::hid::PalmaApplicationSectionAccessBuffer)
        public ResultCode WritePalmaApplicationSection(ServiceCtx context)
        {
            int  palmaConnectionHandle = context.RequestData.ReadInt32();
            long unknown0              = context.RequestData.ReadInt64();
            long unknown1              = context.RequestData.ReadInt64();
            // nn::hid::PalmaApplicationSectionAccessBuffer cast is unknown

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle, unknown0, unknown1 });

            _palmaOperationCompleteEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }

        [Command(511)] // 5.0.0+
        // ReadPalmaUniqueCode(nn::hid::PalmaConnectionHandle)
        public ResultCode ReadPalmaUniqueCode(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            return ResultCode.Success;
        }

        [Command(512)] // 5.0.0+
        // SetPalmaUniqueCodeInvalid(nn::hid::PalmaConnectionHandle)
        public ResultCode SetPalmaUniqueCodeInvalid(ServiceCtx context)
        {
            int palmaConnectionHandle = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { palmaConnectionHandle });

            return ResultCode.Success;
        }

        [Command(522)] // 5.1.0+
        // SetIsPalmaAllConnectable(nn::applet::AppletResourceUserId, bool, pid)
        public ResultCode SetIsPalmaAllConnectable(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            long unknownBool          = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, unknownBool });

            return ResultCode.Success;
        }

        [Command(525)] // 5.1.0+
        // SetPalmaBoostMode(bool)
        public ResultCode SetPalmaBoostMode(ServiceCtx context)
        {
            // NOTE: Stubbed in system module.

            return ResultCode.Success;
        }

        [Command(1000)]
        // SetNpadCommunicationMode(long CommunicationMode, nn::applet::AppletResourceUserId)
        public ResultCode SetNpadCommunicationMode(ServiceCtx context)
        {
                 _npadCommunicationMode = context.RequestData.ReadInt64();
            long appletResourceUserId   = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, _npadCommunicationMode });

            return ResultCode.Success;
        }

        [Command(1001)]
        // GetNpadCommunicationMode() -> long CommunicationMode
        public ResultCode GetNpadCommunicationMode(ServiceCtx context)
        {
            context.ResponseData.Write(_npadCommunicationMode);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { _npadCommunicationMode });

            return ResultCode.Success;
        }

        [Command(1002)] // 9.0.0+
        // SetTouchScreenConfiguration(nn::hid::TouchScreenConfigurationForNx, nn::applet::AppletResourceUserId)
        public ResultCode SetTouchScreenConfiguration(ServiceCtx context)
        {
            long touchScreenConfigurationForNx = context.RequestData.ReadInt64();
            long appletResourceUserId          = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, touchScreenConfigurationForNx });

            return ResultCode.Success;
        }
    }
}
