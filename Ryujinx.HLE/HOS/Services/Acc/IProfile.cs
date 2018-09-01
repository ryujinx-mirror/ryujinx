using ChocolArm64.Memory;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Acc
{
    class IProfile : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private UserProfile Profile;

        private Stream ProfilePictureStream;

        public IProfile(UserProfile Profile)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,  Get          },
                { 1,  GetBase      },
                { 10, GetImageSize },
                { 11, LoadImage    },
            };

            this.Profile = Profile;

            ProfilePictureStream = Assembly.GetCallingAssembly().GetManifestResourceStream("Ryujinx.HLE.RyujinxProfileImage.jpg");
        }

        public long Get(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            long Position = Context.Request.ReceiveBuff[0].Position;

            AMemoryHelper.FillWithZeros(Context.Memory, Position, 0x80);

            Context.Memory.WriteInt32(Position, 0);
            Context.Memory.WriteInt32(Position + 4, 1);
            Context.Memory.WriteInt64(Position + 8, 1);

            return GetBase(Context);
        }

        public long GetBase(ServiceCtx Context)
        {
            Profile.Uuid.Write(Context.ResponseData);

            Context.ResponseData.Write(Profile.LastModifiedTimestamp);

            byte[] Username = StringUtils.GetFixedLengthBytes(Profile.Name, 0x20, Encoding.UTF8);

            Context.ResponseData.Write(Username);

            return 0;
        }

        private long LoadImage(ServiceCtx Context)
        {
            long BufferPosition = Context.Request.ReceiveBuff[0].Position;
            long BufferLen      = Context.Request.ReceiveBuff[0].Size;

            byte[] ProfilePictureData = new byte[BufferLen];

            ProfilePictureStream.Read(ProfilePictureData, 0, ProfilePictureData.Length);

            Context.Memory.WriteBytes(BufferPosition, ProfilePictureData);

            Context.ResponseData.Write(ProfilePictureStream.Length);

            return 0;
        }

        private long GetImageSize(ServiceCtx Context)
        {
            Context.ResponseData.Write(ProfilePictureStream.Length);

            return 0;
        }
    }
}