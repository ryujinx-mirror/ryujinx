using ChocolArm64.Events;
using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.Logging;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class SvcHandler
    {
        private delegate void SvcFunc(AThreadState ThreadState);

        private Dictionary<int, SvcFunc> SvcFuncs;

        private Switch  Device;
        private Process Process;
        private Horizon System;
        private AMemory Memory;

        private struct HleIpcMessage
        {
            public KThread    Thread     { get; private set; }
            public KSession   Session    { get; private set; }
            public IpcMessage Message    { get; private set; }
            public long       MessagePtr { get; private set; }

            public HleIpcMessage(
                KThread    Thread,
                KSession   Session,
                IpcMessage Message,
                long       MessagePtr)
            {
                this.Thread     = Thread;
                this.Session    = Session;
                this.Message    = Message;
                this.MessagePtr = MessagePtr;
            }
        }

        private const uint SelfThreadHandle  = 0xffff8000;
        private const uint SelfProcessHandle = 0xffff8001;

        private static Random Rng;

        public SvcHandler(Switch Device, Process Process)
        {
            SvcFuncs = new Dictionary<int, SvcFunc>()
            {
                { 0x01, SvcSetHeapSize                   },
                { 0x03, SvcSetMemoryAttribute            },
                { 0x04, SvcMapMemory                     },
                { 0x05, SvcUnmapMemory                   },
                { 0x06, SvcQueryMemory                   },
                { 0x07, SvcExitProcess                   },
                { 0x08, SvcCreateThread                  },
                { 0x09, SvcStartThread                   },
                { 0x0a, SvcExitThread                    },
                { 0x0b, SvcSleepThread                   },
                { 0x0c, SvcGetThreadPriority             },
                { 0x0d, SvcSetThreadPriority             },
                { 0x0e, SvcGetThreadCoreMask             },
                { 0x0f, SvcSetThreadCoreMask             },
                { 0x10, SvcGetCurrentProcessorNumber     },
                { 0x12, SvcClearEvent                    },
                { 0x13, SvcMapSharedMemory               },
                { 0x14, SvcUnmapSharedMemory             },
                { 0x15, SvcCreateTransferMemory          },
                { 0x16, SvcCloseHandle                   },
                { 0x17, SvcResetSignal                   },
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
                { 0x25, SvcGetThreadId                   },
                { 0x26, SvcBreak                         },
                { 0x27, SvcOutputDebugString             },
                { 0x29, SvcGetInfo                       },
                { 0x2c, SvcMapPhysicalMemory             },
                { 0x2d, SvcUnmapPhysicalMemory           },
                { 0x32, SvcSetThreadActivity             },
                { 0x33, SvcGetThreadContext3             },
                { 0x34, SvcWaitForAddress                },
                { 0x35, SvcSignalToAddress               }
            };

            this.Device  = Device;
            this.Process = Process;
            this.System  = Process.Device.System;
            this.Memory  = Process.Memory;
        }

        static SvcHandler()
        {
            Rng = new Random();
        }

        public void SvcCall(object sender, AInstExceptionEventArgs e)
        {
            AThreadState ThreadState = (AThreadState)sender;

            Process.GetThread(ThreadState.Tpidr).LastPc = e.Position;

            if (SvcFuncs.TryGetValue(e.Id, out SvcFunc Func))
            {
                Device.Log.PrintDebug(LogClass.KernelSvc, $"{Func.Method.Name} called.");

                Func(ThreadState);

                Device.Log.PrintDebug(LogClass.KernelSvc, $"{Func.Method.Name} ended.");
            }
            else
            {
                Process.PrintStackTrace(ThreadState);

                throw new NotImplementedException($"0x{e.Id:x4}");
            }
        }

        private KThread GetThread(long Tpidr, int Handle)
        {
            if ((uint)Handle == SelfThreadHandle)
            {
                return Process.GetThread(Tpidr);
            }
            else
            {
                return Process.HandleTable.GetData<KThread>(Handle);
            }
        }
    }
}