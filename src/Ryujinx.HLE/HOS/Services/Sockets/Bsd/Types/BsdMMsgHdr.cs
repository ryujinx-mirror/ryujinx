using System;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd.Types
{
    class BsdMMsgHdr
    {
        public BsdMsgHdr[] Messages { get; }

        private BsdMMsgHdr(BsdMsgHdr[] messages)
        {
            Messages = messages;
        }

        public static LinuxError Serialize(Span<byte> rawData, BsdMMsgHdr message)
        {
            rawData[0] = 0x8;
            rawData = rawData[1..];

            for (int index = 0; index < message.Messages.Length; index++)
            {
                LinuxError res = BsdMsgHdr.Serialize(ref rawData, message.Messages[index]);

                if (res != LinuxError.SUCCESS)
                {
                    return res;
                }
            }

            return LinuxError.SUCCESS;
        }

        public static LinuxError Deserialize(out BsdMMsgHdr message, ReadOnlySpan<byte> rawData, int vlen)
        {
            message = null;

            BsdMsgHdr[] messages = new BsdMsgHdr[vlen];

            // Skip "header" byte (Nintendo also ignore it)
            rawData = rawData[1..];

            for (int index = 0; index < messages.Length; index++)
            {
                LinuxError res = BsdMsgHdr.Deserialize(out messages[index], ref rawData);

                if (res != LinuxError.SUCCESS)
                {
                    return res;
                }
            }

            message = new BsdMMsgHdr(messages);

            return LinuxError.SUCCESS;
        }
    }
}
