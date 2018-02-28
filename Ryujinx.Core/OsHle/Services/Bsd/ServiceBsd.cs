using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Bsd
{
    class ServiceBsd : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceBsd()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {  0, Initialize      },
                {  1, StartMonitoring },
                {  2, Socket          },
                { 10, Send            },
                { 14, Connect         }
            };
        }

        //Initialize(u32, u32, u32, u32, u32, u32, u32, u32, u64 pid, u64 transferMemorySize, pid, KObject) -> u32 bsd_errno
        public long Initialize(ServiceCtx Context)
        {
            /*
            typedef struct  {
                u32 version;                // Observed 1 on 2.0 LibAppletWeb, 2 on 3.0.

                u32 tcp_tx_buf_size;        // Size of the TCP transfer (send) buffer (initial or fixed).
                u32 tcp_rx_buf_size;        // Size of the TCP recieve buffer (initial or fixed).
                u32 tcp_tx_buf_max_size;    // Maximum size of the TCP transfer (send) buffer. If it is 0, the size of the buffer is fixed to its initial value.
                u32 tcp_rx_buf_max_size;    // Maximum size of the TCP receive buffer. If it is 0, the size of the buffer is fixed to its initial value.

                u32 udp_tx_buf_size;        // Size of the UDP transfer (send) buffer (typically 0x2400 bytes).
                u32 udp_rx_buf_size;        // Size of the UDP receive buffer (typically 0xA500 bytes).

                u32 sb_efficiency;          // Number of buffers for each socket (standard values range from 1 to 8).
            } BsdBufferConfig;
            */

            long Pid = Context.RequestData.ReadInt64();
            long TransferMemorySize = Context.RequestData.ReadInt64();

            // Two other args are unknown!

            Context.ResponseData.Write(0);

            //Todo: Stub

            return 0;
        }

        //StartMonitoring(u64, pid)
        public long StartMonitoring(ServiceCtx Context)
        {
            //Todo: Stub

            return 0;
        }

        //Socket(u32 domain, u32 type, u32 protocol) -> (i32 ret, u32 bsd_errno)
        public long Socket(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);
            Context.ResponseData.Write(0);

            //Todo: Stub

            return 0;
        }

        //Connect(u32 socket, buffer<sockaddr, 0x21, 0>) -> (i32 ret, u32 bsd_errno)
        public long Connect(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);
            Context.ResponseData.Write(0);

            //Todo: Stub

            return 0;
        }

        //Send(u32 socket, u32 flags, buffer<i8, 0x21, 0>) -> (i32 ret, u32 bsd_errno)
        public long Send(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);
            Context.ResponseData.Write(0);

            //Todo: Stub

            return 0;
        }
    }
}