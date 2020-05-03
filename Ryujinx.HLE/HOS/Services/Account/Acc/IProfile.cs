using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.Utilities;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    class IProfile : IpcService
    {
        private UserProfile _profile;
        private Stream      _profilePictureStream;

        public IProfile(UserProfile profile)
        {
            _profile              = profile;
            _profilePictureStream = Assembly.GetCallingAssembly().GetManifestResourceStream("Ryujinx.HLE.RyujinxProfileImage.jpg");
        }

        [Command(0)]
        // Get() -> (nn::account::profile::ProfileBase, buffer<nn::account::profile::UserData, 0x1a>)
        public ResultCode Get(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAcc);

            long position = context.Request.ReceiveBuff[0].Position;

            MemoryHelper.FillWithZeros(context.Memory, position, 0x80);

            context.Memory.Write((ulong)position, 0);
            context.Memory.Write((ulong)position + 4, 1);
            context.Memory.Write((ulong)position + 8, 1L);

            return GetBase(context);
        }

        [Command(1)]
        // GetBase() -> nn::account::profile::ProfileBase
        public ResultCode GetBase(ServiceCtx context)
        {
            _profile.UserId.Write(context.ResponseData);

            context.ResponseData.Write(_profile.LastModifiedTimestamp);

            byte[] username = StringUtils.GetFixedLengthBytes(_profile.Name, 0x20, Encoding.UTF8);

            context.ResponseData.Write(username);

            return ResultCode.Success;
        }

        [Command(10)]
        // GetImageSize() -> u32
        public ResultCode GetImageSize(ServiceCtx context)
        {
            context.ResponseData.Write(_profilePictureStream.Length);

            return ResultCode.Success;
        }

        [Command(11)]
        // LoadImage() -> (u32, buffer<bytes, 6>)
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
    }
}