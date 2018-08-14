using ChocolArm64.Memory;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Ipc;
using Ryujinx.HLE.OsHle.Utilities;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.OsHle.Services.Acc
{
    class IProfile : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IProfile()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Get     },
                { 1, GetBase }
            };
        }

        public long Get(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            long Position = Context.Request.ReceiveBuff[0].Position;

            AMemoryHelper.FillWithZeros(Context.Memory, Position, 0x80);

            Context.Memory.WriteInt32(Position, 0);
            Context.Memory.WriteInt32(Position + 4, 1);
            Context.Memory.WriteByte(Position + 8, 1);

            return GetBase(Context);
        }

        public long GetBase(ServiceCtx Context)
        {
            ProfileBase ProfileBase = new ProfileBase(Context.Ns.Settings.User);

            Context.ResponseData.Write(ProfileBase.UserId.ToBytes());
            Context.ResponseData.Write(ProfileBase.Timestamp);

            int    ByteCount = Encoding.UTF8.GetByteCount(ProfileBase.Username);
            byte[] Username  = StringUtils.GetFixedLengthBytes(ProfileBase.Username, 0x20, Encoding.UTF8);

            Context.ResponseData.Write(Username);

            return 0;
        }
    }
}