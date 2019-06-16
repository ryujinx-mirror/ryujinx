using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class ISelfController : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private KEvent _libraryAppletLaunchableEvent;

        private KEvent _accumulatedSuspendedTickChangedEvent;
        private int    _accumulatedSuspendedTickChangedEventHandle = 0;

        private int _idleTimeDetectionExtension;

        public ISelfController(Horizon system)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,    Exit                                        },
                { 1,    LockExit                                    },
                { 2,    UnlockExit                                  },
              //{ 3,    EnterFatalSection                           }, // 2.0.0+
              //{ 4,    LeaveFatalSection                           }, // 2.0.0+
                { 9,    GetLibraryAppletLaunchableEvent             },
                { 10,   SetScreenShotPermission                     },
                { 11,   SetOperationModeChangedNotification         },
                { 12,   SetPerformanceModeChangedNotification       },
                { 13,   SetFocusHandlingMode                        },
                { 14,   SetRestartMessageEnabled                    },
              //{ 15,   SetScreenShotAppletIdentityInfo             }, // 2.0.0+
                { 16,   SetOutOfFocusSuspendingEnabled              }, // 2.0.0+
              //{ 17,   SetControllerFirmwareUpdateSection          }, // 3.0.0+
              //{ 18,   SetRequiresCaptureButtonShortPressedMessage }, // 3.0.0+
                { 19,   SetScreenShotImageOrientation               }, // 3.0.0+
              //{ 20,   SetDesirableKeyboardLayout                  }, // 4.0.0+
              //{ 40,   CreateManagedDisplayLayer                   },
              //{ 41,   IsSystemBufferSharingEnabled                }, // 4.0.0+
              //{ 42,   GetSystemSharedLayerHandle                  }, // 4.0.0+
              //{ 43,   GetSystemSharedBufferHandle                 }, // 5.0.0+
                { 50,   SetHandlesRequestToDisplay                  },
              //{ 51,   ApproveToDisplay                            },
              //{ 60,   OverrideAutoSleepTimeAndDimmingTime         },
              //{ 61,   SetMediaPlaybackState                       },
                { 62,   SetIdleTimeDetectionExtension               },
                { 63,   GetIdleTimeDetectionExtension               },
              //{ 64,   SetInputDetectionSourceSet                  },
              //{ 65,   ReportUserIsActive                          }, // 2.0.0+
              //{ 66,   GetCurrentIlluminance                       }, // 3.0.0+
              //{ 67,   IsIlluminanceAvailable                      }, // 3.0.0+
              //{ 68,   SetAutoSleepDisabled                        }, // 5.0.0+
              //{ 69,   IsAutoSleepDisabled                         }, // 5.0.0+
              //{ 70,   ReportMultimediaError                       }, // 4.0.0+
              //{ 71,   GetCurrentIlluminanceEx                     }, // 5.0.0+
              //{ 80,   SetWirelessPriorityMode                     }, // 4.0.0+
              //{ 90,   GetAccumulatedSuspendedTickValue            }, // 6.0.0+
                { 91,   GetAccumulatedSuspendedTickChangedEvent     }, // 6.0.0+
              //{ 100,  SetAlbumImageTakenNotificationEnabled       }, // 7.0.0+
              //{ 110,  SetApplicationAlbumUserData                 }, // 8.0.0+
              //{ 1000, GetDebugStorageChannel                      }, // 7.0.0+
            };

            _libraryAppletLaunchableEvent = new KEvent(system);
        }

        public long Exit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long LockExit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long UnlockExit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long GetLibraryAppletLaunchableEvent(ServiceCtx context)
        {
            _libraryAppletLaunchableEvent.ReadableEvent.Signal();

            if (context.Process.HandleTable.GenerateHandle(_libraryAppletLaunchableEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long SetScreenShotPermission(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long SetOperationModeChangedNotification(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long SetPerformanceModeChangedNotification(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long SetFocusHandlingMode(ServiceCtx context)
        {
            bool flag1 = context.RequestData.ReadByte() != 0;
            bool flag2 = context.RequestData.ReadByte() != 0;
            bool flag3 = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long SetRestartMessageEnabled(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long SetOutOfFocusSuspendingEnabled(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long SetScreenShotImageOrientation(ServiceCtx context)
        {
            int orientation = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long SetHandlesRequestToDisplay(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        // SetIdleTimeDetectionExtension(u32)
        public long SetIdleTimeDetectionExtension(ServiceCtx context)
        {
            _idleTimeDetectionExtension = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceAm, new { _idleTimeDetectionExtension });

            return 0;
        }

        // GetIdleTimeDetectionExtension() -> u32
        public long GetIdleTimeDetectionExtension(ServiceCtx context)
        {
            context.ResponseData.Write(_idleTimeDetectionExtension);

            Logger.PrintStub(LogClass.ServiceAm, new { _idleTimeDetectionExtension });

            return 0;
        }

        // GetAccumulatedSuspendedTickChangedEvent() -> handle<copy>
        public long GetAccumulatedSuspendedTickChangedEvent(ServiceCtx context)
        {
            if (_accumulatedSuspendedTickChangedEventHandle == 0)
            {
                _accumulatedSuspendedTickChangedEvent = new KEvent(context.Device.System);

                _accumulatedSuspendedTickChangedEvent.ReadableEvent.Signal();

                if (context.Process.HandleTable.GenerateHandle(_accumulatedSuspendedTickChangedEvent.ReadableEvent, out _accumulatedSuspendedTickChangedEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_accumulatedSuspendedTickChangedEventHandle);

            return 0;
        }
    }
}
