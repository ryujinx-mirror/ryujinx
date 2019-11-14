using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using System;
using System.IO;

namespace Ryujinx.HLE.HOS.Applets
{
    internal class PlayerSelectApplet : IApplet
    {
        private Horizon _system;

        private AppletFifo<byte[]> _inputData;
        private AppletFifo<byte[]> _outputData;

        public event EventHandler AppletStateChanged;

        public PlayerSelectApplet(Horizon system)
        {
            _system = system;
        }

        public ResultCode Start(AppletFifo<byte[]> inData, AppletFifo<byte[]> outData)
        {
            _inputData  = inData;
            _outputData = outData;

            // TODO(jduncanator): Parse PlayerSelectConfig from input data
            _outputData.Push(BuildResponse());

            AppletStateChanged?.Invoke(this, null);

            return ResultCode.Success;
        }

        public ResultCode GetResult()
        {
            return ResultCode.Success;
        }

        private byte[] BuildResponse()
        {
            UserProfile currentUser = _system.State.Account.LastOpenedUser;

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((ulong)PlayerSelectResult.Success);

                currentUser.UserId.Write(writer);

                return stream.ToArray();
            }
        }
    }
}
