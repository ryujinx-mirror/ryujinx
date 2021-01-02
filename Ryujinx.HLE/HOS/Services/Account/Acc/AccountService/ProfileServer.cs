using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.Utilities;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Account.Acc.AccountService
{
    class ProfileServer
    {
        private UserProfile _profile;
        private Stream      _profilePictureStream;

        public ProfileServer(UserProfile profile)
        {
            _profile              = profile;
            _profilePictureStream = Assembly.GetCallingAssembly().GetManifestResourceStream("Ryujinx.HLE.RyujinxProfileImage.jpg");
        }

        public ResultCode Get(ServiceCtx context)
        {
            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize(0x80L);

            long bufferPosition = context.Request.RecvListBuff[0].Position;

            MemoryHelper.FillWithZeros(context.Memory, bufferPosition, 0x80);

            // TODO: Determine the struct.
            context.Memory.Write((ulong)bufferPosition,           0); // Unknown
            context.Memory.Write((ulong)bufferPosition + 4,       1); // Icon ID. 0 = Mii, the rest are character icon IDs.
            context.Memory.Write((ulong)bufferPosition + 8, (byte)1); // Profile icon background color ID
            // 0x07 bytes - Unknown
            // 0x10 bytes - Some ID related to the Mii? All zeros when a character icon is used.
            // 0x60 bytes - Usually zeros?

            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            return GetBase(context);
        }

        public ResultCode GetBase(ServiceCtx context)
        {
            _profile.UserId.Write(context.ResponseData);

            context.ResponseData.Write(_profile.LastModifiedTimestamp);

            byte[] username = StringUtils.GetFixedLengthBytes(_profile.Name, 0x20, Encoding.UTF8);

            context.ResponseData.Write(username);

            return ResultCode.Success;
        }

        public ResultCode GetImageSize(ServiceCtx context)
        {
            context.ResponseData.Write(_profilePictureStream.Length);

            return ResultCode.Success;
        }

        public ResultCode LoadImage(ServiceCtx context)
        {
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferLen      = context.Request.ReceiveBuff[0].Size;

            byte[] profilePictureData = new byte[bufferLen];

            _profilePictureStream.Read(profilePictureData, 0, profilePictureData.Length);

            context.Memory.Write((ulong)bufferPosition, profilePictureData);

            context.ResponseData.Write(_profilePictureStream.Length);

            return ResultCode.Success;
        }

        public ResultCode Store(ServiceCtx context)
        {
            long userDataPosition = context.Request.PtrBuff[0].Position;
            long userDataSize     = context.Request.PtrBuff[0].Size;

            byte[] userData = new byte[userDataSize];

            context.Memory.Read((ulong)userDataPosition, userData);

            // TODO: Read the nn::account::profile::ProfileBase and store everything in the savedata.

            Logger.Stub?.PrintStub(LogClass.ServiceAcc, new { userDataSize });

            return ResultCode.Success;
        }

        public ResultCode StoreWithImage(ServiceCtx context)
        {
            long userDataPosition = context.Request.PtrBuff[0].Position;
            long userDataSize     = context.Request.PtrBuff[0].Size;

            byte[] userData = new byte[userDataSize];

            context.Memory.Read((ulong)userDataPosition, userData);

            long profilePicturePosition = context.Request.SendBuff[0].Position;
            long profilePictureSize     = context.Request.SendBuff[0].Size;

            byte[] profilePictureData = new byte[profilePictureSize];

            context.Memory.Read((ulong)profilePicturePosition, profilePictureData);

            // TODO: Read the nn::account::profile::ProfileBase and store everything in the savedata.

            Logger.Stub?.PrintStub(LogClass.ServiceAcc, new { userDataSize, profilePictureSize });

            return ResultCode.Success;
        }
    }
}