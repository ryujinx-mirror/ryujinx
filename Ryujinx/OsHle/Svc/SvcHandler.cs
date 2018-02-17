using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Collections.Generic;

namespace Ryujinx.OsHle.Svc
{
    partial class SvcHandler
    {
        private delegate void SvcFunc(ARegisters Registers);

        private Dictionary<int, SvcFunc> SvcFuncs;

        private Switch  Ns;
        private Process Process;
        private AMemory Memory;

        private static Random Rng;

        public SvcHandler(Switch Ns, Process Process)
        {
            SvcFuncs = new Dictionary<int, SvcFunc>()
            {
                { 0x01, SvcSetHeapSize                   },
                { 0x03, SvcSetMemoryAttribute            },
                { 0x04, SvcMapMemory                     },
                { 0x06, SvcQueryMemory                   },
                { 0x07, SvcExitProcess                   },
                { 0x08, SvcCreateThread                  },
                { 0x09, SvcStartThread                   },
                { 0x0b, SvcSleepThread                   },
                { 0x0c, SvcGetThreadPriority             },
                { 0x13, SvcMapSharedMemory               },
                { 0x14, SvcUnmapSharedMemory             },
                { 0x15, SvcCreateTransferMemory          },
                { 0x16, SvcCloseHandle                   },
                { 0x17, SvcResetSignal                   },
                { 0x18, SvcWaitSynchronization           },
                { 0x1a, SvcArbitrateLock                 },
                { 0x1b, SvcArbitrateUnlock               },
                { 0x1c, SvcWaitProcessWideKeyAtomic      },
                { 0x1d, SvcSignalProcessWideKey          },
                { 0x1e, SvcGetSystemTick                 },
                { 0x1f, SvcConnectToNamedPort            },
                { 0x21, SvcSendSyncRequest               },
                { 0x22, SvcSendSyncRequestWithUserBuffer },
                { 0x26, SvcBreak                         },
                { 0x27, SvcOutputDebugString             },
                { 0x29, SvcGetInfo                       }
            };

            this.Ns      = Ns;
            this.Process = Process;
            this.Memory  = Process.Memory;
        }

        static SvcHandler()
        {
            Rng = new Random();
        }

        public void SvcCall(object sender, AInstExceptEventArgs e)
        {
            ARegisters Registers = (ARegisters)sender;

            if (SvcFuncs.TryGetValue(e.Id, out SvcFunc Func))
            {
                Logging.Trace($"(Thread {Registers.ThreadId}) {Func.Method.Name} called.");

                Func(Registers);

                Logging.Trace($"(Thread {Registers.ThreadId}) {Func.Method.Name} ended.");
            }
            else
            {
                throw new NotImplementedException(e.Id.ToString("x4"));
            }
        }
    }
}