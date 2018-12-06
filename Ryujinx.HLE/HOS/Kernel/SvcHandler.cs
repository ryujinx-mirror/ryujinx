using ChocolArm64.Events;
using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.Common.Logging;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class SvcHandler
    {
        private delegate void SvcFunc(CpuThreadState threadState);

        private Dictionary<int, SvcFunc> _svcFuncs;

        private Switch        _device;
        private KProcess      _process;
        private Horizon       _system;
        private MemoryManager _memory;

        private struct HleIpcMessage
        {
            public KThread    Thread     { get; private set; }
            public KSession   Session    { get; private set; }
            public IpcMessage Message    { get; private set; }
            public long       MessagePtr { get; private set; }

            public HleIpcMessage(
                KThread    thread,
                KSession   session,
                IpcMessage message,
                long       messagePtr)
            {
                Thread     = thread;
                Session    = session;
                Message    = message;
                MessagePtr = messagePtr;
            }
        }

        public SvcHandler(Switch device, KProcess process)
        {
            _svcFuncs = new Dictionary<int, SvcFunc>
            {
                { 0x01, SvcSetHeapSize                   },
                { 0x03, SvcSetMemoryAttribute            },
                { 0x04, SvcMapMemory                     },
                { 0x05, SvcUnmapMemory                   },
                { 0x06, SvcQueryMemory                   },
                { 0x07, SvcExitProcess                   },
                { 0x08, CreateThread64                   },
                { 0x09, SvcStartThread                   },
                { 0x0a, SvcExitThread                    },
                { 0x0b, SvcSleepThread                   },
                { 0x0c, SvcGetThreadPriority             },
                { 0x0d, SvcSetThreadPriority             },
                { 0x0e, SvcGetThreadCoreMask             },
                { 0x0f, SetThreadCoreMask64              },
                { 0x10, SvcGetCurrentProcessorNumber     },
                { 0x11, SignalEvent64                    },
                { 0x12, ClearEvent64                     },
                { 0x13, SvcMapSharedMemory               },
                { 0x14, SvcUnmapSharedMemory             },
                { 0x15, SvcCreateTransferMemory          },
                { 0x16, SvcCloseHandle                   },
                { 0x17, ResetSignal64                    },
                { 0x18, SvcWaitSynchronization           },
                { 0x19, SvcCancelSynchronization         },
                { 0x1a, SvcArbitrateLock                 },
                { 0x1b, SvcArbitrateUnlock               },
                { 0x1c, SvcWaitProcessWideKeyAtomic      },
                { 0x1d, SvcSignalProcessWideKey          },
                { 0x1e, SvcGetSystemTick                 },
                { 0x1f, SvcConnectToNamedPort            },
                { 0x21, SvcSendSyncRequest               },
                { 0x22, SvcSendSyncRequestWithUserBuffer },
                { 0x24, GetProcessId64                   },
                { 0x25, SvcGetThreadId                   },
                { 0x26, SvcBreak                         },
                { 0x27, SvcOutputDebugString             },
                { 0x29, GetInfo64                        },
                { 0x2c, SvcMapPhysicalMemory             },
                { 0x2d, SvcUnmapPhysicalMemory           },
                { 0x32, SvcSetThreadActivity             },
                { 0x33, SvcGetThreadContext3             },
                { 0x34, SvcWaitForAddress                },
                { 0x35, SvcSignalToAddress               },
                { 0x45, CreateEvent64                    },
                { 0x65, GetProcessList64                 },
                { 0x6f, GetSystemInfo64                  },
                { 0x70, CreatePort64                     },
                { 0x71, ManageNamedPort64                }
            };

            _device  = device;
            _process = process;
            _system  = device.System;
            _memory  = process.CpuMemory;
        }

        public void SvcCall(object sender, InstExceptionEventArgs e)
        {
            CpuThreadState threadState = (CpuThreadState)sender;

            if (_svcFuncs.TryGetValue(e.Id, out SvcFunc func))
            {
                Logger.PrintDebug(LogClass.KernelSvc, $"{func.Method.Name} called.");

                func(threadState);

                Logger.PrintDebug(LogClass.KernelSvc, $"{func.Method.Name} ended.");
            }
            else
            {
                //Process.PrintStackTrace(ThreadState);

                throw new NotImplementedException($"0x{e.Id:x4}");
            }
        }
    }
}