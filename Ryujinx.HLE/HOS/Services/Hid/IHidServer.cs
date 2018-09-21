using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.Input;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    class IHidServer : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        private KEvent NpadStyleSetUpdateEvent;
        private KEvent XpadIdEvent;
        private KEvent PalmaOperationCompleteEvent;

        private int XpadIdEventHandle;

        private bool SixAxisSensorFusionEnabled;
        private bool UnintendedHomeButtonInputProtectionEnabled;
        private bool VibrationPermitted;
        private bool UsbFullKeyControllerEnabled;

        private HidNpadJoyHoldType            NpadJoyHoldType;
        private HidNpadStyle                  NpadStyleTag;
        private HidNpadJoyAssignmentMode      NpadJoyAssignmentMode;
        private HidNpadHandheldActivationMode NpadHandheldActivationMode;
        private HidGyroscopeZeroDriftMode     GyroscopeZeroDriftMode;

        private long  NpadCommunicationMode;
        private uint  AccelerometerPlayMode;
        private long  VibrationGcErmCommand;
        private float SevenSixAxisSensorFusionStrength;

        private HidSensorFusionParameters  SensorFusionParams;
        private HidAccelerometerParameters AccelerometerParams;
        private HidVibrationValue          VibrationValue;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IHidServer(Horizon System)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,    CreateAppletResource                          },
                { 1,    ActivateDebugPad                              },
                { 11,   ActivateTouchScreen                           },
                { 21,   ActivateMouse                                 },
                { 31,   ActivateKeyboard                              },
                { 40,   AcquireXpadIdEventHandle                      },
                { 41,   ReleaseXpadIdEventHandle                      },
                { 51,   ActivateXpad                                  },
                { 55,   GetXpadIds                                    },
                { 56,   ActivateJoyXpad                               },
                { 58,   GetJoyXpadLifoHandle                          },
                { 59,   GetJoyXpadIds                                 },
                { 60,   ActivateSixAxisSensor                         },
                { 61,   DeactivateSixAxisSensor                       },
                { 62,   GetSixAxisSensorLifoHandle                    },
                { 63,   ActivateJoySixAxisSensor                      },
                { 64,   DeactivateJoySixAxisSensor                    },
                { 65,   GetJoySixAxisSensorLifoHandle                 },
                { 66,   StartSixAxisSensor                            },
                { 67,   StopSixAxisSensor                             },
                { 68,   IsSixAxisSensorFusionEnabled                  },
                { 69,   EnableSixAxisSensorFusion                     },
                { 70,   SetSixAxisSensorFusionParameters              },
                { 71,   GetSixAxisSensorFusionParameters              },
                { 72,   ResetSixAxisSensorFusionParameters            },
                { 73,   SetAccelerometerParameters                    },
                { 74,   GetAccelerometerParameters                    },
                { 75,   ResetAccelerometerParameters                  },
                { 76,   SetAccelerometerPlayMode                      },
                { 77,   GetAccelerometerPlayMode                      },
                { 78,   ResetAccelerometerPlayMode                    },
                { 79,   SetGyroscopeZeroDriftMode                     },
                { 80,   GetGyroscopeZeroDriftMode                     },
                { 81,   ResetGyroscopeZeroDriftMode                   },
                { 82,   IsSixAxisSensorAtRest                         },
                { 91,   ActivateGesture                               },
                { 100,  SetSupportedNpadStyleSet                      },
                { 101,  GetSupportedNpadStyleSet                      },
                { 102,  SetSupportedNpadIdType                        },
                { 103,  ActivateNpad                                  },
                { 104,  DeactivateNpad                                },
                { 106,  AcquireNpadStyleSetUpdateEventHandle          },
                { 107,  DisconnectNpad                                },
                { 108,  GetPlayerLedPattern                           },
                { 109,  ActivateNpadWithRevision                      },
                { 120,  SetNpadJoyHoldType                            },
                { 121,  GetNpadJoyHoldType                            },
                { 122,  SetNpadJoyAssignmentModeSingleByDefault       },
                { 123,  SetNpadJoyAssignmentModeSingle                },
                { 124,  SetNpadJoyAssignmentModeDual                  },
                { 125,  MergeSingleJoyAsDualJoy                       },
                { 126,  StartLrAssignmentMode                         },
                { 127,  StopLrAssignmentMode                          },
                { 128,  SetNpadHandheldActivationMode                 },
                { 129,  GetNpadHandheldActivationMode                 },
                { 130,  SwapNpadAssignment                            },
                { 131,  IsUnintendedHomeButtonInputProtectionEnabled  },
                { 132,  EnableUnintendedHomeButtonInputProtection     },
                { 133,  SetNpadJoyAssignmentModeSingleWithDestination },
                { 200,  GetVibrationDeviceInfo                        },
                { 201,  SendVibrationValue                            },
                { 202,  GetActualVibrationValue                       },
                { 203,  CreateActiveVibrationDeviceList               },
                { 204,  PermitVibration                               },
                { 205,  IsVibrationPermitted                          },
                { 206,  SendVibrationValues                           },
                { 207,  SendVibrationGcErmCommand                     },
                { 208,  GetActualVibrationGcErmCommand                },
                { 209,  BeginPermitVibrationSession                   },
                { 210,  EndPermitVibrationSession                     },
                { 300,  ActivateConsoleSixAxisSensor                  },
                { 301,  StartConsoleSixAxisSensor                     },
                { 302,  StopConsoleSixAxisSensor                      },
                { 303,  ActivateSevenSixAxisSensor                    },
                { 304,  StartSevenSixAxisSensor                       },
                { 305,  StopSevenSixAxisSensor                        },
                { 306,  InitializeSevenSixAxisSensor                  },
                { 307,  FinalizeSevenSixAxisSensor                    },
                { 308,  SetSevenSixAxisSensorFusionStrength           },
                { 309,  GetSevenSixAxisSensorFusionStrength           },
                { 400,  IsUsbFullKeyControllerEnabled                 },
                { 401,  EnableUsbFullKeyController                    },
                { 402,  IsUsbFullKeyControllerConnected               },
                { 403,  HasBattery                                    },
                { 404,  HasLeftRightBattery                           },
                { 405,  GetNpadInterfaceType                          },
                { 406,  GetNpadLeftRightInterfaceType                 },
                { 500,  GetPalmaConnectionHandle                      },
                { 501,  InitializePalma                               },
                { 502,  AcquirePalmaOperationCompleteEvent            },
                { 503,  GetPalmaOperationInfo                         },
                { 504,  PlayPalmaActivity                             },
                { 505,  SetPalmaFrModeType                            },
                { 506,  ReadPalmaStep                                 },
                { 507,  EnablePalmaStep                               },
                { 508,  SuspendPalmaStep                              },
                { 509,  ResetPalmaStep                                },
                { 510,  ReadPalmaApplicationSection                   },
                { 511,  WritePalmaApplicationSection                  },
                { 512,  ReadPalmaUniqueCode                           },
                { 513,  SetPalmaUniqueCodeInvalid                     },
                { 1000, SetNpadCommunicationMode                      },
                { 1001, GetNpadCommunicationMode                      },
            };

            NpadStyleSetUpdateEvent     = new KEvent(System);
            XpadIdEvent                 = new KEvent(System);
            PalmaOperationCompleteEvent = new KEvent(System);

            NpadJoyHoldType            = HidNpadJoyHoldType.Vertical;
            NpadStyleTag               = HidNpadStyle.FullKey | HidNpadStyle.Dual | HidNpadStyle.Left | HidNpadStyle.Right;
            NpadJoyAssignmentMode      = HidNpadJoyAssignmentMode.Dual;
            NpadHandheldActivationMode = HidNpadHandheldActivationMode.Dual;
            GyroscopeZeroDriftMode     = HidGyroscopeZeroDriftMode.Standard;

            SensorFusionParams  = new HidSensorFusionParameters();
            AccelerometerParams = new HidAccelerometerParameters();
            VibrationValue      = new HidVibrationValue();
        }

        // CreateAppletResource(nn::applet::AppletResourceUserId) -> object<nn::hid::IAppletResource>
        public long CreateAppletResource(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            MakeObject(Context, new IAppletResource(Context.Device.System.HidSharedMem));

            return 0;
        }

        // ActivateDebugPad(nn::applet::AppletResourceUserId)
        public long ActivateDebugPad(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // ActivateTouchScreen(nn::applet::AppletResourceUserId)
        public long ActivateTouchScreen(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // ActivateMouse(nn::applet::AppletResourceUserId)
        public long ActivateMouse(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // ActivateKeyboard(nn::applet::AppletResourceUserId)
        public long ActivateKeyboard(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // AcquireXpadIdEventHandle(ulong XpadId) -> nn::sf::NativeHandle
        public long AcquireXpadIdEventHandle(ServiceCtx Context)
        {
            long XpadId = Context.RequestData.ReadInt64();

            XpadIdEventHandle = Context.Process.HandleTable.OpenHandle(XpadIdEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(XpadIdEventHandle);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. XpadId: {XpadId}");

            return 0;
        }

        // ReleaseXpadIdEventHandle(ulong XpadId)
        public long ReleaseXpadIdEventHandle(ServiceCtx Context)
        {
            long XpadId = Context.RequestData.ReadInt64();

            Context.Process.HandleTable.CloseHandle(XpadIdEventHandle);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. XpadId: {XpadId}");

            return 0;
        }

        // ActivateXpad(nn::hid::BasicXpadId, nn::applet::AppletResourceUserId)
        public long ActivateXpad(ServiceCtx Context)
        {
            int  BasicXpadId          = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"BasicXpadId: {BasicXpadId}");

            return 0;
        }

        // GetXpadIds() -> long IdsCount, buffer<array<nn::hid::BasicXpadId>, type: 0xa>
        public long GetXpadIds(ServiceCtx Context)
        {
            // There is any Xpad, so we return 0 and write nothing inside the type-0xa buffer.
            Context.ResponseData.Write(0L);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed.");

            return 0;
        }

        // ActivateJoyXpad(nn::hid::JoyXpadId)
        public long ActivateJoyXpad(ServiceCtx Context)
        {
            int JoyXpadId = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. JoyXpadId: {JoyXpadId}");

            return 0;
        }

        // GetJoyXpadLifoHandle(nn::hid::JoyXpadId) -> nn::sf::NativeHandle
        public long GetJoyXpadLifoHandle(ServiceCtx Context)
        {
            int JoyXpadId = Context.RequestData.ReadInt32();

            int Handle = 0;

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. JoyXpadId: {JoyXpadId}");

            return 0;
        }

        // GetJoyXpadIds() -> long IdsCount, buffer<array<nn::hid::JoyXpadId>, type: 0xa>
        public long GetJoyXpadIds(ServiceCtx Context)
        {
            // There is any JoyXpad, so we return 0 and write nothing inside the type-0xa buffer.
            Context.ResponseData.Write(0L);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed.");

            return 0;
        }

        // ActivateSixAxisSensor(nn::hid::BasicXpadId)
        public long ActivateSixAxisSensor(ServiceCtx Context)
        {
            int BasicXpadId = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. BasicXpadId: {BasicXpadId}");

            return 0;
        }

        // DeactivateSixAxisSensor(nn::hid::BasicXpadId)
        public long DeactivateSixAxisSensor(ServiceCtx Context)
        {
            int BasicXpadId = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. BasicXpadId: {BasicXpadId}");

            return 0;
        }

        // GetSixAxisSensorLifoHandle(nn::hid::BasicXpadId) -> nn::sf::NativeHandle
        public long GetSixAxisSensorLifoHandle(ServiceCtx Context)
        {
            int BasicXpadId = Context.RequestData.ReadInt32();

            int Handle = 0;

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. BasicXpadId: {BasicXpadId}");

            return 0;
        }

        // ActivateJoySixAxisSensor(nn::hid::JoyXpadId)
        public long ActivateJoySixAxisSensor(ServiceCtx Context)
        {
            int JoyXpadId = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. JoyXpadId: {JoyXpadId}");

            return 0;
        }

        // DeactivateJoySixAxisSensor(nn::hid::JoyXpadId)
        public long DeactivateJoySixAxisSensor(ServiceCtx Context)
        {
            int JoyXpadId = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. JoyXpadId: {JoyXpadId}");

            return 0;
        }

        // GetJoySixAxisSensorLifoHandle(nn::hid::JoyXpadId) -> nn::sf::NativeHandle
        public long GetJoySixAxisSensorLifoHandle(ServiceCtx Context)
        {
            int JoyXpadId = Context.RequestData.ReadInt32();

            int Handle = 0;

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. JoyXpadId: {JoyXpadId}");

            return 0;
        }

        // StartSixAxisSensor(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long StartSixAxisSensor(ServiceCtx Context)
        {
            int  SixAxisSensorHandle  = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle}");

            return 0;
        }

        // StopSixAxisSensor(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long StopSixAxisSensor(ServiceCtx Context)
        {
            int  SixAxisSensorHandle  = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle}");

            return 0;
        }

        // IsSixAxisSensorFusionEnabled(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> bool IsEnabled
        public long IsSixAxisSensorFusionEnabled(ServiceCtx Context)
        {
            int SixAxisSensorHandle = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(SixAxisSensorFusionEnabled);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"SixAxisSensorFusionEnabled: {SixAxisSensorFusionEnabled}");

            return 0;
        }

        // EnableSixAxisSensorFusion(bool Enabled, nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long EnableSixAxisSensorFusion(ServiceCtx Context)
        {
            SixAxisSensorFusionEnabled = Context.RequestData.ReadBoolean();
            int  SixAxisSensorHandle   = Context.RequestData.ReadInt32();
            long AppletResourceUserId  = Context.RequestData.ReadInt64();
            
            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"SixAxisSensorFusionEnabled: {SixAxisSensorFusionEnabled}");

            return 0;
        }

        // SetSixAxisSensorFusionParameters(nn::hid::SixAxisSensorHandle, float RevisePower, float ReviseRange, nn::applet::AppletResourceUserId)
        public long SetSixAxisSensorFusionParameters(ServiceCtx Context)
        {
            int   SixAxisSensorHandle = Context.RequestData.ReadInt32();

            SensorFusionParams = new HidSensorFusionParameters()
            {
                RevisePower = Context.RequestData.ReadInt32(),
                ReviseRange = Context.RequestData.ReadInt32(),
            };

            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"RevisePower: {SensorFusionParams.RevisePower} - " +
                                                              $"ReviseRange: {SensorFusionParams.ReviseRange}");

            return 0;
        }

        // GetSixAxisSensorFusionParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> float RevisePower, float ReviseRange)
        public long GetSixAxisSensorFusionParameters(ServiceCtx Context)
        {
            int  SixAxisSensorHandle  = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(SensorFusionParams.RevisePower);
            Context.ResponseData.Write(SensorFusionParams.ReviseRange);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"RevisePower: {SensorFusionParams.RevisePower} - " +
                                                              $"ReviseRange: {SensorFusionParams.ReviseRange}");

            return 0;
        }

        // ResetSixAxisSensorFusionParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long ResetSixAxisSensorFusionParameters(ServiceCtx Context)
        {
            int  SixAxisSensorHandle  = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            SensorFusionParams.RevisePower = 0;
            SensorFusionParams.ReviseRange = 0;

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"RevisePower: {SensorFusionParams.RevisePower} - " +
                                                              $"ReviseRange: {SensorFusionParams.ReviseRange}");

            return 0;
        }

        // SetAccelerometerParameters(nn::hid::SixAxisSensorHandle, float X, float Y, nn::applet::AppletResourceUserId)
        public long SetAccelerometerParameters(ServiceCtx Context)
        {
            int SixAxisSensorHandle = Context.RequestData.ReadInt32();

            AccelerometerParams = new HidAccelerometerParameters()
            {
                X = Context.RequestData.ReadInt32(),
                Y = Context.RequestData.ReadInt32(),
            };

            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"X: {AccelerometerParams.X} - " +
                                                              $"Y: {AccelerometerParams.Y}");

            return 0;
        }

        // GetAccelerometerParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> float X, float Y
        public long GetAccelerometerParameters(ServiceCtx Context)
        {
            int  SixAxisSensorHandle  = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(AccelerometerParams.X);
            Context.ResponseData.Write(AccelerometerParams.Y);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"X: {AccelerometerParams.X} - " +
                                                              $"Y: {AccelerometerParams.Y}");

            return 0;
        }

        // ResetAccelerometerParameters(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long ResetAccelerometerParameters(ServiceCtx Context)
        {
            int  SixAxisSensorHandle  = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            AccelerometerParams.X = 0;
            AccelerometerParams.Y = 0;

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"X: {AccelerometerParams.X} - " +
                                                              $"Y: {AccelerometerParams.Y}");

            return 0;
        }

        // SetAccelerometerPlayMode(nn::hid::SixAxisSensorHandle, uint PlayMode, nn::applet::AppletResourceUserId)
        public long SetAccelerometerPlayMode(ServiceCtx Context)
        {
            int  SixAxisSensorHandle   = Context.RequestData.ReadInt32();
                 AccelerometerPlayMode = Context.RequestData.ReadUInt32();
            long AppletResourceUserId  = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"PlayMode: {AccelerometerPlayMode}");

            return 0;
        }

        // GetAccelerometerPlayMode(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> uint PlayMode
        public long GetAccelerometerPlayMode(ServiceCtx Context)
        {
            int SixAxisSensorHandle = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(AccelerometerPlayMode);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"PlayMode: {AccelerometerPlayMode}");

            return 0;
        }

        // ResetAccelerometerPlayMode(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long ResetAccelerometerPlayMode(ServiceCtx Context)
        {
            int  SixAxisSensorHandle  = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            AccelerometerPlayMode = 0;

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"PlayMode: {AccelerometerPlayMode}");

            return 0;
        }

        // SetGyroscopeZeroDriftMode(nn::hid::SixAxisSensorHandle, uint GyroscopeZeroDriftMode, nn::applet::AppletResourceUserId)
        public long SetGyroscopeZeroDriftMode(ServiceCtx Context)
        {
            int  SixAxisSensorHandle    = Context.RequestData.ReadInt32();
                 GyroscopeZeroDriftMode = (HidGyroscopeZeroDriftMode)Context.RequestData.ReadInt32();
            long AppletResourceUserId   = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"GyroscopeZeroDriftMode: {GyroscopeZeroDriftMode}");

            return 0;
        }

        // GetGyroscopeZeroDriftMode(nn::applet::AppletResourceUserId, nn::hid::SixAxisSensorHandle) -> int GyroscopeZeroDriftMode
        public long GetGyroscopeZeroDriftMode(ServiceCtx Context)
        {
            int  SixAxisSensorHandle  = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write((int)GyroscopeZeroDriftMode);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"GyroscopeZeroDriftMode: {GyroscopeZeroDriftMode}");

            return 0;
        }

        // ResetGyroscopeZeroDriftMode(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long ResetGyroscopeZeroDriftMode(ServiceCtx Context)
        {
            int  SixAxisSensorHandle  = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            GyroscopeZeroDriftMode = HidGyroscopeZeroDriftMode.Standard;

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"GyroscopeZeroDriftMode: {GyroscopeZeroDriftMode}");

            return 0;
        }

        // IsSixAxisSensorAtRest(nn::hid::SixAxisSensorHandle, nn::applet::AppletResourceUserId) -> bool IsAsRest
        public long IsSixAxisSensorAtRest(ServiceCtx Context)
        { 
            int  SixAxisSensorHandle  = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            bool IsAtRest = true;

            Context.ResponseData.Write(IsAtRest);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SixAxisSensorHandle: {SixAxisSensorHandle} - " +
                                                              $"IsAtRest: {IsAtRest}");

            return 0;
        }

        // ActivateGesture(nn::applet::AppletResourceUserId, int Unknown0)
        public long ActivateGesture(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();
            int  Unknown0             = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"Unknown0: {Unknown0}");

            return 0;
        }


        // SetSupportedNpadStyleSet(nn::applet::AppletResourceUserId, nn::hid::NpadStyleTag)
        public long SetSupportedNpadStyleSet(ServiceCtx Context)
        {
            NpadStyleTag = (HidNpadStyle)Context.RequestData.ReadInt32();

            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"NpadStyleTag: {NpadStyleTag}");

            return 0;
        }

        // GetSupportedNpadStyleSet(nn::applet::AppletResourceUserId) -> uint nn::hid::NpadStyleTag
        public long GetSupportedNpadStyleSet(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write((int)NpadStyleTag);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"NpadStyleTag: {NpadStyleTag}");

            return 0;
        }

        // SetSupportedNpadIdType(nn::applet::AppletResourceUserId, array<NpadIdType, 9>)
        public long SetSupportedNpadIdType(ServiceCtx Context)
        {
            long AppletResourceUserId  = Context.RequestData.ReadInt64();
            HidControllerId NpadIdType = (HidControllerId)Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"NpadIdType: {NpadIdType}");

            return 0;
        }

        // ActivateNpad(nn::applet::AppletResourceUserId)
        public long ActivateNpad(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // DeactivateNpad(nn::applet::AppletResourceUserId)
        public long DeactivateNpad(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // AcquireNpadStyleSetUpdateEventHandle(nn::applet::AppletResourceUserId, uint, ulong) -> nn::sf::NativeHandle
        public long AcquireNpadStyleSetUpdateEventHandle(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();
            int  NpadId               = Context.RequestData.ReadInt32();
            long NpadStyleSet         = Context.RequestData.ReadInt64();

            int Handle = Context.Process.HandleTable.OpenHandle(NpadStyleSetUpdateEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"NpadId: {NpadId} - " +
                                                              $"NpadStyleSet: {NpadStyleSet}");

            return 0;
        }

        // DisconnectNpad(nn::applet::AppletResourceUserId, uint NpadIdType)
        public long DisconnectNpad(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();
            int  NpadIdType           = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"NpadIdType: {NpadIdType}");

            return 0;
        }

        // GetPlayerLedPattern(uint NpadId) -> ulong LedPattern
        public long GetPlayerLedPattern(ServiceCtx Context)
        {
            int NpadId = Context.RequestData.ReadInt32();

            long LedPattern = 0;

            Context.ResponseData.Write(LedPattern);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. NpadId: {NpadId} - Pattern: {LedPattern}");

            return 0;
        }

        // ActivateNpadWithRevision(nn::applet::AppletResourceUserId, int Unknown)
        public long ActivateNpadWithRevision(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();
            int  Unknown              = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - Unknown: {Unknown}");

            return 0;
        }

        // SetNpadJoyHoldType(nn::applet::AppletResourceUserId, long NpadJoyHoldType)
        public long SetNpadJoyHoldType(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();
            NpadJoyHoldType           = (HidNpadJoyHoldType)Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"NpadJoyHoldType: {NpadJoyHoldType}");

            return 0;
        }

        // GetNpadJoyHoldType(nn::applet::AppletResourceUserId) -> long NpadJoyHoldType
        public long GetNpadJoyHoldType(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write((long)NpadJoyHoldType);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"NpadJoyHoldTypeValue: {NpadJoyHoldType}");

            return 0;
        }

        // SetNpadJoyAssignmentModeSingleByDefault(uint HidControllerId, nn::applet::AppletResourceUserId)
        public long SetNpadJoyAssignmentModeSingleByDefault(ServiceCtx Context)
        {
            HidControllerId HidControllerId      = (HidControllerId)Context.RequestData.ReadInt32();
            long            AppletResourceUserId = Context.RequestData.ReadInt64();

            NpadJoyAssignmentMode = HidNpadJoyAssignmentMode.Single;

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"HidControllerId: {HidControllerId} - " +
                                                              $"NpadJoyAssignmentModeValue: {NpadJoyAssignmentMode}");

            return 0;
        }

        // SetNpadJoyAssignmentModeSingle(uint HidControllerId, nn::applet::AppletResourceUserId, long HidNpadJoyDeviceType)
        public long SetNpadJoyAssignmentModeSingle(ServiceCtx Context)
        {
            HidControllerId      HidControllerId      = (HidControllerId)Context.RequestData.ReadInt32();
            long                 AppletResourceUserId = Context.RequestData.ReadInt64();
            HidNpadJoyDeviceType HidNpadJoyDeviceType = (HidNpadJoyDeviceType)Context.RequestData.ReadInt64();

            NpadJoyAssignmentMode = HidNpadJoyAssignmentMode.Single;

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"HidControllerId: {HidControllerId} - " +
                                                              $"HidNpadJoyDeviceType: {HidNpadJoyDeviceType} - " +
                                                              $"NpadJoyAssignmentModeValue: {NpadJoyAssignmentMode}");

            return 0;
        }

        // SetNpadJoyAssignmentModeDual(uint HidControllerId, nn::applet::AppletResourceUserId)
        public long SetNpadJoyAssignmentModeDual(ServiceCtx Context)
        {
            HidControllerId HidControllerId      = (HidControllerId)Context.RequestData.ReadInt32();
            long            AppletResourceUserId = Context.RequestData.ReadInt64();

            NpadJoyAssignmentMode = HidNpadJoyAssignmentMode.Dual;

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"HidControllerId: {HidControllerId} - " +
                                                              $"NpadJoyAssignmentModeValue: {NpadJoyAssignmentMode}");

            return 0;
        }

        // MergeSingleJoyAsDualJoy(uint SingleJoyId0, uint SingleJoyId1, nn::applet::AppletResourceUserId)
        public long MergeSingleJoyAsDualJoy(ServiceCtx Context)
        {
            long SingleJoyId0         = Context.RequestData.ReadInt32();
            long SingleJoyId1         = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SingleJoyId0: {SingleJoyId0} - " +
                                                              $"SingleJoyId1: {SingleJoyId1}");

            return 0;
        }

        // StartLrAssignmentMode(nn::applet::AppletResourceUserId)
        public long StartLrAssignmentMode(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // StopLrAssignmentMode(nn::applet::AppletResourceUserId)
        public long StopLrAssignmentMode(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // SetNpadHandheldActivationMode(nn::applet::AppletResourceUserId, long HidNpadHandheldActivationMode)
        public long SetNpadHandheldActivationMode(ServiceCtx Context)
        {
            long AppletResourceUserId  = Context.RequestData.ReadInt64();
            NpadHandheldActivationMode = (HidNpadHandheldActivationMode)Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"NpadHandheldActivationMode: {NpadHandheldActivationMode}");

            return 0;
        }

        // GetNpadHandheldActivationMode(nn::applet::AppletResourceUserId) -> long HidNpadHandheldActivationMode
        public long GetNpadHandheldActivationMode(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write((long)NpadHandheldActivationMode);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"NpadHandheldActivationMode: {NpadHandheldActivationMode}");

            return 0;
        }

        // SwapNpadAssignment(uint OldNpadAssignment, uint NewNpadAssignment, nn::applet::AppletResourceUserId)
        public long SwapNpadAssignment(ServiceCtx Context)
        {
            int  OldNpadAssignment    = Context.RequestData.ReadInt32();
            int  NewNpadAssignment    = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"OldNpadAssignment: {OldNpadAssignment} - " +
                                                              $"NewNpadAssignment: {NewNpadAssignment}");

            return 0;
        }

        // IsUnintendedHomeButtonInputProtectionEnabled(uint Unknown0, nn::applet::AppletResourceUserId) ->  bool IsEnabled
        public long IsUnintendedHomeButtonInputProtectionEnabled(ServiceCtx Context)
        {
            uint  Unknown0            = Context.RequestData.ReadUInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(UnintendedHomeButtonInputProtectionEnabled);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"Unknown0: {Unknown0} - " +
                                                              $"UnintendedHomeButtonInputProtectionEnabled: {UnintendedHomeButtonInputProtectionEnabled}");

            return 0;
        }

        // EnableUnintendedHomeButtonInputProtection(bool Enable, uint Unknown0, nn::applet::AppletResourceUserId)
        public long EnableUnintendedHomeButtonInputProtection(ServiceCtx Context)
        {
            UnintendedHomeButtonInputProtectionEnabled = Context.RequestData.ReadBoolean();
            uint  Unknown0                             = Context.RequestData.ReadUInt32();
            long AppletResourceUserId                  = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"Unknown0: {Unknown0} - " +
                                                              $"UnintendedHomeButtonInputProtectionEnable: {UnintendedHomeButtonInputProtectionEnabled}");

            return 0;
        }

        // SetNpadJoyAssignmentModeSingleWithDestination(uint HidControllerId, long HidNpadJoyDeviceType, nn::applet::AppletResourceUserId) -> bool Unknown0, uint Unknown1
        public long SetNpadJoyAssignmentModeSingleWithDestination(ServiceCtx Context)
        {
            HidControllerId      HidControllerId      = (HidControllerId)Context.RequestData.ReadInt32();
            HidNpadJoyDeviceType HidNpadJoyDeviceType = (HidNpadJoyDeviceType)Context.RequestData.ReadInt64();
            long                 AppletResourceUserId = Context.RequestData.ReadInt64();

            NpadJoyAssignmentMode = HidNpadJoyAssignmentMode.Single;

            Context.ResponseData.Write(0); //Unknown0
            Context.ResponseData.Write(0); //Unknown1

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"HidControllerId: {HidControllerId} - " +
                                                              $"HidNpadJoyDeviceType: {HidNpadJoyDeviceType} - " +
                                                              $"NpadJoyAssignmentModeValue: {NpadJoyAssignmentMode} - " +
                                                              $"Unknown0: 0 - " +
                                                              $"Unknown1: 0");

            return 0;
        }

        // GetVibrationDeviceInfo(nn::hid::VibrationDeviceHandle) -> nn::hid::VibrationDeviceInfo
        public long GetVibrationDeviceInfo(ServiceCtx Context)
        {
            int VibrationDeviceHandle = Context.RequestData.ReadInt32();

            HidVibrationDeviceValue DeviceInfo = new HidVibrationDeviceValue
            {
                DeviceType = HidVibrationDeviceType.None,
                Position   = HidVibrationDevicePosition.None
            };

            Context.ResponseData.Write((int)DeviceInfo.DeviceType);
            Context.ResponseData.Write((int)DeviceInfo.Position);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. VibrationDeviceHandle: {VibrationDeviceHandle} - " +
                                                              $"DeviceType: {DeviceInfo.DeviceType} - " +
                                                              $"Position: {DeviceInfo.Position}");

            return 0;
        }

        // SendVibrationValue(nn::hid::VibrationDeviceHandle, nn::hid::VibrationValue, nn::applet::AppletResourceUserId)
        public long SendVibrationValue(ServiceCtx Context)
        {
            int VibrationDeviceHandle = Context.RequestData.ReadInt32();

            VibrationValue = new HidVibrationValue
            {
                AmplitudeLow  = Context.RequestData.ReadSingle(),
                FrequencyLow  = Context.RequestData.ReadSingle(),
                AmplitudeHigh = Context.RequestData.ReadSingle(),
                FrequencyHigh = Context.RequestData.ReadSingle()
            };

            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"VibrationDeviceHandle: {VibrationDeviceHandle} - " +
                                                              $"AmplitudeLow: {VibrationValue.AmplitudeLow} - " +
                                                              $"FrequencyLow: {VibrationValue.FrequencyLow} - " +
                                                              $"AmplitudeHigh: {VibrationValue.AmplitudeHigh} - " +
                                                              $"FrequencyHigh: {VibrationValue.FrequencyHigh}");

            return 0;
        }

        // GetActualVibrationValue(nn::hid::VibrationDeviceHandle, nn::applet::AppletResourceUserId) -> nn::hid::VibrationValue
        public long GetActualVibrationValue(ServiceCtx Context)
        {
            int VibrationDeviceHandle = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(VibrationValue.AmplitudeLow);
            Context.ResponseData.Write(VibrationValue.FrequencyLow);
            Context.ResponseData.Write(VibrationValue.AmplitudeHigh);
            Context.ResponseData.Write(VibrationValue.FrequencyHigh);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"VibrationDeviceHandle: {VibrationDeviceHandle} - " +
                                                              $"AmplitudeLow: {VibrationValue.AmplitudeLow} - " +
                                                              $"FrequencyLow: {VibrationValue.FrequencyLow} - " +
                                                              $"AmplitudeHigh: {VibrationValue.AmplitudeHigh} - " +
                                                              $"FrequencyHigh: {VibrationValue.FrequencyHigh}");

            return 0;
        }

        // CreateActiveVibrationDeviceList() -> object<nn::hid::IActiveVibrationDeviceList>
        public long CreateActiveVibrationDeviceList(ServiceCtx Context)
        {
            MakeObject(Context, new IActiveApplicationDeviceList());

            return 0;
        }

        // PermitVibration(bool Enable)
        public long PermitVibration(ServiceCtx Context)
        {
            VibrationPermitted = Context.RequestData.ReadBoolean();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. VibrationPermitted: {VibrationPermitted}");

            return 0;
        }

        // IsVibrationPermitted() -> bool IsEnabled
        public long IsVibrationPermitted(ServiceCtx Context)
        {
            Context.ResponseData.Write(VibrationPermitted);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. VibrationPermitted: {VibrationPermitted}");

            return 0;
        }

        // SendVibrationValues(nn::applet::AppletResourceUserId, buffer<array<nn::hid::VibrationDeviceHandle>, type: 9>, buffer<array<nn::hid::VibrationValue>, type: 9>)
        public long SendVibrationValues(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            byte[] VibrationDeviceHandleBuffer = Context.Memory.ReadBytes(
                Context.Request.PtrBuff[0].Position,
                Context.Request.PtrBuff[0].Size);

            byte[] VibrationValueBuffer = Context.Memory.ReadBytes(
                Context.Request.PtrBuff[1].Position,
                Context.Request.PtrBuff[1].Size);

            //Todo: Read all handles and values from buffer.

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"VibrationDeviceHandleBufferLength: {VibrationDeviceHandleBuffer.Length} - " +
                                                              $"VibrationValueBufferLength: {VibrationValueBuffer.Length}");

            return 0;
        }

        // SendVibrationGcErmCommand(nn::hid::VibrationDeviceHandle, nn::hid::VibrationGcErmCommand, nn::applet::AppletResourceUserId)
        public long SendVibrationGcErmCommand(ServiceCtx Context)
        {
            int  VibrationDeviceHandle = Context.RequestData.ReadInt32();
            long VibrationGcErmCommand = Context.RequestData.ReadInt64();
            long AppletResourceUserId  = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"VibrationDeviceHandle: {VibrationDeviceHandle} - " +
                                                              $"VibrationGcErmCommand: {VibrationGcErmCommand}");

            return 0;
        }

        // GetActualVibrationGcErmCommand(nn::hid::VibrationDeviceHandle, nn::applet::AppletResourceUserId) -> nn::hid::VibrationGcErmCommand
        public long GetActualVibrationGcErmCommand(ServiceCtx Context)
        {
            int  VibrationDeviceHandle = Context.RequestData.ReadInt32();
            long AppletResourceUserId  = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(VibrationGcErmCommand);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"VibrationDeviceHandle: {VibrationDeviceHandle} - " +
                                                              $"VibrationGcErmCommand: {VibrationGcErmCommand}");

            return 0;
        }

        // BeginPermitVibrationSession(nn::applet::AppletResourceUserId)
        public long BeginPermitVibrationSession(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // EndPermitVibrationSession()
        public long EndPermitVibrationSession(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed.");

            return 0;
        }

        // ActivateConsoleSixAxisSensor(nn::applet::AppletResourceUserId)
        public long ActivateConsoleSixAxisSensor(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // StartConsoleSixAxisSensor(nn::hid::ConsoleSixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long StartConsoleSixAxisSensor(ServiceCtx Context)
        {
            int  ConsoleSixAxisSensorHandle = Context.RequestData.ReadInt32();
            long AppletResourceUserId       = Context.RequestData.ReadInt64();
            
            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"ConsoleSixAxisSensorHandle: {ConsoleSixAxisSensorHandle}");

            return 0;
        }

        // StopConsoleSixAxisSensor(nn::hid::ConsoleSixAxisSensorHandle, nn::applet::AppletResourceUserId)
        public long StopConsoleSixAxisSensor(ServiceCtx Context)
        {
            int  ConsoleSixAxisSensorHandle = Context.RequestData.ReadInt32();
            long AppletResourceUserId       = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"ConsoleSixAxisSensorHandle: {ConsoleSixAxisSensorHandle}");

            return 0;
        }

        // ActivateSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public long ActivateSevenSixAxisSensor(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // StartSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public long StartSevenSixAxisSensor(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // StopSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public long StopSevenSixAxisSensor(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // InitializeSevenSixAxisSensor(array<nn::sf::NativeHandle>, ulong Counter0, array<nn::sf::NativeHandle>, ulong Counter1, nn::applet::AppletResourceUserId)
        public long InitializeSevenSixAxisSensor(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();
            long Counter0             = Context.RequestData.ReadInt64();
            long Counter1             = Context.RequestData.ReadInt64();

            // Todo: Determine if array<nn::sf::NativeHandle> is a buffer or not...

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"Counter0: {Counter0} - " +
                                                              $"Counter1: {Counter1}");

            return 0;
        }

        // FinalizeSevenSixAxisSensor(nn::applet::AppletResourceUserId)
        public long FinalizeSevenSixAxisSensor(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId}");

            return 0;
        }

        // SetSevenSixAxisSensorFusionStrength(float Strength, nn::applet::AppletResourceUserId)
        public long SetSevenSixAxisSensorFusionStrength(ServiceCtx Context)
        {
                 SevenSixAxisSensorFusionStrength = Context.RequestData.ReadSingle();
            long AppletResourceUserId             = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SevenSixAxisSensorFusionStrength: {SevenSixAxisSensorFusionStrength}");

            return 0;
        }

        // GetSevenSixAxisSensorFusionStrength(nn::applet::AppletResourceUserId) -> float Strength
        public long GetSevenSixAxisSensorFusionStrength(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(SevenSixAxisSensorFusionStrength);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"SevenSixAxisSensorFusionStrength: {SevenSixAxisSensorFusionStrength}");

            return 0;
        }

        // IsUsbFullKeyControllerEnabled() -> bool IsEnabled
        public long IsUsbFullKeyControllerEnabled(ServiceCtx Context)
        {
            Context.ResponseData.Write(UsbFullKeyControllerEnabled);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. UsbFullKeyControllerEnabled: {UsbFullKeyControllerEnabled}");

            return 0;
        }

        // EnableUsbFullKeyController(bool Enable)
        public long EnableUsbFullKeyController(ServiceCtx Context)
        {
            UsbFullKeyControllerEnabled = Context.RequestData.ReadBoolean();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. UsbFullKeyControllerEnabled: {UsbFullKeyControllerEnabled}");

            return 0;
        }

        // IsUsbFullKeyControllerConnected(uint Unknown0) -> bool Connected
        public long IsUsbFullKeyControllerConnected(ServiceCtx Context)
        {
            int Unknown0 = Context.RequestData.ReadInt32();

            Context.ResponseData.Write((byte)0x1); //FullKeyController is always connected ?

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. Unknown0: {Unknown0} - Connected: true");

            return 0;
        }

        // HasBattery(uint NpadId) -> bool HasBattery
        public long HasBattery(ServiceCtx Context)
        {
            int NpadId = Context.RequestData.ReadInt32();

            Context.ResponseData.Write((byte)0x1); //Npad always got a battery ?

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. NpadId: {NpadId} - HasBattery: true");

            return 0;
        }

        // HasLeftRightBattery(uint NpadId) -> bool HasLeftBattery, bool HasRightBattery
        public long HasLeftRightBattery(ServiceCtx Context)
        {
            int NpadId = Context.RequestData.ReadInt32();

            Context.ResponseData.Write((byte)0x1); //Npad always got a left battery ?
            Context.ResponseData.Write((byte)0x1); //Npad always got a right battery ?

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. NpadId: {NpadId} - HasLeftBattery: true - HasRightBattery: true");

            return 0;
        }

        // GetNpadInterfaceType(uint NpadId) -> uchar InterfaceType
        public long GetNpadInterfaceType(ServiceCtx Context)
        {
            int NpadId = Context.RequestData.ReadInt32();

            Context.ResponseData.Write((byte)0);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. NpadId: {NpadId} - NpadInterfaceType: 0");

            return 0;
        }

        // GetNpadLeftRightInterfaceType(uint NpadId) -> uchar LeftInterfaceType, uchar RightInterfaceType
        public long GetNpadLeftRightInterfaceType(ServiceCtx Context)
        {
            int NpadId = Context.RequestData.ReadInt32();

            Context.ResponseData.Write((byte)0);
            Context.ResponseData.Write((byte)0);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. NpadId: {NpadId} - " +
                                                              $"LeftInterfaceType: 0 - " +
                                                              $"RightInterfaceType: 0");

            return 0;
        }

        // GetPalmaConnectionHandle(uint Unknown0, nn::applet::AppletResourceUserId) -> nn::hid::PalmaConnectionHandle
        public long GetPalmaConnectionHandle(ServiceCtx Context)
        {
            int  Unknown0             = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            int PalmaConnectionHandle = 0;

            Context.ResponseData.Write(PalmaConnectionHandle);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"Unknown0: {Unknown0} - " +
                                                              $"PalmaConnectionHandle: {PalmaConnectionHandle}");

            return 0;
        }

        // InitializePalma(nn::hid::PalmaConnectionHandle)
        public long InitializePalma(ServiceCtx Context)
        {
            int PalmaConnectionHandle = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle}");

            return 0;
        }

        // AcquirePalmaOperationCompleteEvent(nn::hid::PalmaConnectionHandle) -> nn::sf::NativeHandle
        public long AcquirePalmaOperationCompleteEvent(ServiceCtx Context)
        {
            int PalmaConnectionHandle = Context.RequestData.ReadInt32();

            int Handle = Context.Process.HandleTable.OpenHandle(PalmaOperationCompleteEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle}");

            return 0;
        }

        // GetPalmaOperationInfo(nn::hid::PalmaConnectionHandle) -> long Unknown0, buffer<Unknown>
        public long GetPalmaOperationInfo(ServiceCtx Context)
        {
            int PalmaConnectionHandle = Context.RequestData.ReadInt32();

            long Unknown0 = 0; //Counter?

            Context.ResponseData.Write(Unknown0);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle} - " +
                                                              $"Unknown0: {Unknown0}");

            return 0;
        }

        // PlayPalmaActivity(nn::hid::PalmaConnectionHandle, ulong Unknown0)
        public long PlayPalmaActivity(ServiceCtx Context)
        {
            int  PalmaConnectionHandle = Context.RequestData.ReadInt32();
            long Unknown0              = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle} - " +
                                                              $"Unknown0: {Unknown0}");

            return 0;
        }

        // SetPalmaFrModeType(nn::hid::PalmaConnectionHandle, ulong FrModeType)
        public long SetPalmaFrModeType(ServiceCtx Context)
        {
            int  PalmaConnectionHandle = Context.RequestData.ReadInt32();
            long FrModeType            = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle} - " + 
                                                              $"FrModeType: {FrModeType}");

            return 0;
        }

        // ReadPalmaStep(nn::hid::PalmaConnectionHandle)
        public long ReadPalmaStep(ServiceCtx Context)
        {
            int PalmaConnectionHandle = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle}");

            return 0;
        }

        // EnablePalmaStep(nn::hid::PalmaConnectionHandle, bool Enable)
        public long EnablePalmaStep(ServiceCtx Context)
        {
            int  PalmaConnectionHandle = Context.RequestData.ReadInt32();
            bool EnabledPalmaStep      = Context.RequestData.ReadBoolean();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle} - " +
                                                              $"EnabledPalmaStep: {EnabledPalmaStep}");

            return 0;
        }

        // SuspendPalmaStep(nn::hid::PalmaConnectionHandle)
        public long SuspendPalmaStep(ServiceCtx Context)
        {
            int PalmaConnectionHandle = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle}");

            return 0;
        }

        // ResetPalmaStep(nn::hid::PalmaConnectionHandle)
        public long ResetPalmaStep(ServiceCtx Context)
        {
            int PalmaConnectionHandle = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle}");

            return 0;
        }

        // ReadPalmaApplicationSection(nn::hid::PalmaConnectionHandle, ulong Unknown0, ulong Unknown1)
        public long ReadPalmaApplicationSection(ServiceCtx Context)
        {
            int  PalmaConnectionHandle = Context.RequestData.ReadInt32();
            long Unknown0              = Context.RequestData.ReadInt64();
            long Unknown1              = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle} - " +
                                                              $"Unknown0: {Unknown0} - " +
                                                              $"Unknown1: {Unknown1}");

            return 0;
        }

        // WritePalmaApplicationSection(nn::hid::PalmaConnectionHandle, ulong Unknown0, ulong Unknown1, nn::hid::PalmaApplicationSectionAccessBuffer)
        public long WritePalmaApplicationSection(ServiceCtx Context)
        {
            int  PalmaConnectionHandle = Context.RequestData.ReadInt32();
            long Unknown0              = Context.RequestData.ReadInt64();
            long Unknown1              = Context.RequestData.ReadInt64();
            // nn::hid::PalmaApplicationSectionAccessBuffer cast is unknown

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle} - " +
                                                              $"Unknown0: {Unknown0} - " +
                                                              $"Unknown1: {Unknown1}");

            return 0;
        }

        // ReadPalmaUniqueCode(nn::hid::PalmaConnectionHandle)
        public long ReadPalmaUniqueCode(ServiceCtx Context)
        {
            int PalmaConnectionHandle = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle}");

            return 0;
        }

        // SetPalmaUniqueCodeInvalid(nn::hid::PalmaConnectionHandle)
        public long SetPalmaUniqueCodeInvalid(ServiceCtx Context)
        {
            int PalmaConnectionHandle = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. PalmaConnectionHandle: {PalmaConnectionHandle}");

            return 0;
        }

        // SetNpadCommunicationMode(long CommunicationMode, nn::applet::AppletResourceUserId)
        public long SetNpadCommunicationMode(ServiceCtx Context)
        {
                 NpadCommunicationMode = Context.RequestData.ReadInt64();
            long AppletResourceUserId  = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. AppletResourceUserId: {AppletResourceUserId} - " +
                                                              $"NpadCommunicationMode: {NpadCommunicationMode}");

            return 0;
        }

        // GetNpadCommunicationMode() -> long CommunicationMode
        public long GetNpadCommunicationMode(ServiceCtx Context)
        {
            Context.ResponseData.Write(NpadCommunicationMode);

            Context.Device.Log.PrintStub(LogClass.ServiceHid, $"Stubbed. CommunicationMode: {NpadCommunicationMode}");

            return 0;
        }
    }
}
